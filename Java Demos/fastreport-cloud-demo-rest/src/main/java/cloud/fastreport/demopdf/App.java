package cloud.fastreport.demopdf;

import okhttp3.*;
import org.json.JSONObject;

import java.io.*;
import java.lang.Thread;

public class App {
    public static void main(String[] args) {
        String templateFolder = "";
        String exportFolder = "";
        String templateId = "";
        String exportId = "";
        String apiKey = "pr35g73rk5jgcazjb75e8wb8otdm3zauh9p5xbddm9ygtmu9s1ao";
        String subId = "6051f2a06c07a10001737394";

        File file = new File("Template.frx");

        OkHttpClient client = new OkHttpClient();
        MediaType mediaType = MediaType.parse("application/json; charset=utf-8");

        Request getTemplateRoot = new Request.Builder()
                .url("https://fastreport.cloud/api/rp/v1/Templates/Root?subscriptionId=" + subId)
                .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                .build();

        try {
            Response response = client.newCall(getTemplateRoot).execute();
            if (response.body() != null) {
                templateFolder = new JSONObject(response.body().string()).getString("id");
            }
        } catch (IOException e) {
            System.out.println("Error: " + e);
        }

        Request getExportRoot = new Request.Builder()
                .url("https://fastreport.cloud/api/rp/v1/Exports/Root?subscriptionId=" + subId)
                .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                .build();

        try {
            Response response = client.newCall(getExportRoot).execute();
            if (response.body() != null) {
                exportFolder = new JSONObject(response.body().string()).getString("id");
            }
        } catch (IOException e) {
            System.out.println("Error: " + e);
        }

        // Uploading template
        RequestBody uploadRequestBody = new MultipartBody.Builder()
                .setType(MultipartBody.FORM)
                .addFormDataPart("FileContent", file.getName(), RequestBody.create(MediaType.parse("application/json"), file))
                .build();

        Request uploadTemplate = new Request.Builder()
                .url("https://fastreport.cloud/api/rp/v2/Templates/Folder/" + templateFolder + "/File")
                .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                .addHeader("accept", "application/json")
                .addHeader("Content-Type", "multipart/form-data")
                .post(uploadRequestBody)
                .build();

        try {
            Response response = client.newCall(uploadTemplate).execute();
            if (response.body() != null) {
                templateId = new JSONObject(response.body().string()).getString("id");

                System.out.println("Uploading pdf!");
            }
        } catch (IOException e) {
            System.out.println("Error: " + e);
        }

        // Exporting pdf
        JSONObject jsonRequest = new JSONObject();
        jsonRequest.put("$t", "ExportTemplateVM");
        jsonRequest.put("fileName", "Exported file");
        jsonRequest.put("format", "Pdf");
        jsonRequest.put("folderId", exportFolder);
        jsonRequest.put("reportParameters", new JSONObject().put("Parameter1", "Value"));
        RequestBody body = RequestBody.create(jsonRequest.toString(), mediaType);
        Request exportTemplate = new Request.Builder()
                .url("https://fastreport.cloud/api/rp/v1/Templates/File/" + templateId + "/Export")
                .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                .post(body)
                .build();
        try {
            Response response = client.newCall(exportTemplate).execute();
            if (response.body() != null) {
                exportId = new JSONObject(response.body().string()).getString("id");
                System.out.println("Exporting pdf!");
            }
        } catch (IOException e) {
            System.out.println("Error: " + e);
        }


        // Downloading pdf
        Request request = new Request.Builder()
                .url("https://fastreport.cloud/download/e/" + exportId)
                .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                .build();

        try {
            Response response = client.newCall(request).execute();
            while(response.code() != 200){
                try {
                    Thread.sleep(1000);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                response = client.newCall(request).execute();
            }
            ResponseBody responseBody = response.body();
            if (responseBody != null) {
                InputStream inputStream = responseBody.byteStream();
                File file1 = new File("Exported file.pdf");

                try (FileOutputStream outputStream = new FileOutputStream(file1)) {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = inputStream.read(buffer)) != -1) {
                        outputStream.write(buffer, 0, bytesRead);
                    }
                    System.out.println("Downloading pdf");
                }
            } else {
                System.out.println("Response body is null");
            }
        } catch (IOException e) {
            System.out.println("Error: " + e.getMessage());
            e.printStackTrace();
        }
    }
}