package main

import (
	"fmt"
	"io/ioutil"
	"time"

	"context"
	"encoding/base64"
	"os"

	fr "github.com/fastreports/gofrcloud"
)

const server_url = "https://fastreport.cloud"
const apiKey = "***PUT YOUR API KEY HERE***"

const reportFileName = "report.frx"

func check(err error) {
	if err != nil {
		panic(err)
	}
}

func main() {
	config := fr.NewConfiguration()
	// set-up the server
	config.Servers = fr.ServerConfigurations{
		{
			URL:         server_url,
			Description: "",
		},
	}

	fmt.Fprintf(os.Stdout, "App is worikng with %s\n", server_url)

	client := fr.NewAPIClient(config)

	// set-up the API key
	auth := context.WithValue(context.Background(), fr.ContextBasicAuth, fr.BasicAuth{
		UserName: "apikey",
		Password: apiKey,
	})

	// get the root forlder of template storage
	templateRoot, r, err := client.TemplatesApi.TemplateFoldersGetRootFolder(auth).SubscriptionId("").Execute()
	if err != nil {
		fmt.Fprintf(os.Stdout, "Error when calling `TemplatesApi.TemplateFoldersGetRootFolder`: %v\n", err)
		fmt.Fprintf(os.Stdout, "Full HTTP response: %v\n", r)
		return
	}

	fmt.Println("Get the template root folder")

	// load report template from file
	reportFile, err := ioutil.ReadFile(reportFileName)
	check(err)

	// we need base64-encoded content
	report_content := base64.StdEncoding.EncodeToString(reportFile)

	// create a new template file in client
	templateFile := *fr.NewTemplateCreateVM()
	templateFile.SetName(reportFileName)
	templateFile.SetContent(report_content) // base64 string here

	// upload a template file in storage
	template, r, err := client.TemplatesApi.TemplatesUploadFile(auth, templateRoot.GetId()).FileVM(templateFile).Execute()
	if err != nil {
		fmt.Fprintf(os.Stdout, "Error when calling `TemplatesApi.TemplatesUploadFile`: %v\n", err)
		fmt.Fprintf(os.Stdout, "Full HTTP response: %v\n", r)
		return
	}

	fmt.Println("Upload the template file")

	// get the export root folder
	exportRoot, r, err := client.ExportsApi.ExportFoldersGetRootFolder(auth).SubscriptionId("").Execute()
	if err != nil {
		fmt.Fprintf(os.Stdout, "Error when calling `ExportsApi.ExportFoldersGetRootFolder`: %v\n", err)
		fmt.Fprintf(os.Stdout, "Full HTTP response: %v\n", r)
		return
	}
	fmt.Println("Get the export root folder")

	exportTask := *fr.NewExportTemplateTaskVM()

	// you can replace PDF to any supported format, for example: XLSX, DOCX, etc.
	exportTask.SetFileName("report.pdf")
	exportTask.SetFormat("pdf")
	exportTask.FolderId = exportRoot.Id

	export, r, err := client.TemplatesApi.TemplatesExport(auth, template.GetId()).ExportTask(exportTask).Execute()
	if err != nil {
		fmt.Fprintf(os.Stdout, "Error when calling `TemplatesApi.TemplatesExport`: %v\n", err)
		fmt.Fprintf(os.Stdout, "Full HTTP response: %v\n", r)
		return
	}

	fmt.Println("Start the export task")

	// wait for result
	i := 0
	for export.GetStatus() != "Success" && i < 100 {
		export, r, err = client.ExportsApi.ExportsGetFile(auth, export.GetId()).Execute()
		if err != nil {
			fmt.Fprintf(os.Stdout, "Error when calling `ExportsApi.ExportsGetFile`: %v\n", err)
			fmt.Fprintf(os.Stdout, "Full HTTP response: %v\n", r)
			return
		}
		// slip some time and repeat
		time.Sleep(100)
		i++
		fmt.Print("*")
	}

	if i == 100 {
		fmt.Println("Timeout expired!")
		return
	} else {
		fmt.Println()
	}

	fmt.Println("File exported succesfully")

	_, r, err = client.DownloadApi.DownloadGetExport(auth, export.GetId()).Execute()

	// now it is without error checking and without os.File (resulting file came in r.Body)
	// to-do after future fix for octect-stream in gofrcloud/client.go/decode

	buf, err := ioutil.ReadAll(r.Body)
	check(err)

	err = ioutil.WriteFile(*exportTask.FileName, buf, 0644)
	check(err)

	fmt.Fprintf(os.Stdout, "File saved in %s\n", *exportTask.FileName)
}
