# Go example for FastReport Cloud SDK

## Overview

This example shows how to generate a PDF file using the [FastReport Cloud](https://fastreport.cloud/en/) service.

## How to build an example

### Get the FastReport Cloud SDK

Install the following dependencies:

```shell
go get github.com/fastreports/gofrcloud
```

### Change the API key and server URL

To work with the service you need an API key. You can get it from the owners of the service, or generate it yourself in the control panel.

Then you should change the `server_url` and `apiKey` constants in the source code of the example in the `main.go` file

## Compilation and execution

Run the following commands in the console:

```shell
go build
go run main.go
```

You can replace the `report.frx` file with any template file and also change the save format in the source code.

## Documentation

[FastReport Cloud Golang SDK documentation](https://github.com/FastReports/gofrcloud/blob/main/README.md)
[REST API documentation](https://fastreport.cloud/en/docs/guides/rest_api)

## Support 

Contact us with any questions using [our website](https://www.fast-report.com/en/support/) or [GitHub issues](https://github.com/FastReports/FastReport-Cloud/issues). 

## Useful links

[FastReport Designer Community Edition](https://github.com/FastReports/FastReport/releases)

[FastReport Cloud home site](https://fastreport.cloud)

[FastReport Cloud SDK for Golang](https://github.com/FastReports/gofrcloud)



