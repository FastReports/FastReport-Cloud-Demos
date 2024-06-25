package org.example

import okhttp3.*
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.asRequestBody
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject
import java.io.*

fun main() {
    var templateFolder = ""
    var exportFolder = ""
    var templateId = ""
    var exportId = ""
    val apiKey = "***PUT YOUR API KEY HERE***"
    val subId = "***PUT YOUR SUBSCRIPTION ID HERE***"

    val file = File("Template.frx")

    val client = OkHttpClient()
    val mediaType = "application/json; charset=utf-8".toMediaType()

    val getTemplateRoot = Request.Builder()
        .url("https://fastreport.cloud/api/rp/v1/Templates/Root?subscriptionId=${subId}")
        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
        .build()

    try {
        client.newCall(getTemplateRoot).execute().use { response ->
            templateFolder = JSONObject(response.body!!.string()).getString("id")
        }
    } catch (e: IOException) {
        println("Error: $e")
    }

    val getExportRoot = Request.Builder()
        .url("https://fastreport.cloud/api/rp/v1/Exports/Root?subscriptionId=${subId}")
        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
        .build()

    try {
        client.newCall(getExportRoot).execute().use { response ->
            exportFolder = JSONObject(response.body!!.string()).getString("id")
        }
    } catch (e: IOException) {
        println("Error: $e")
    }

    // Uploading template
    val uploadRequestBody = MultipartBody.Builder()
        .setType(MultipartBody.FORM)
        .addFormDataPart("FileContent", file.name, file.asRequestBody("application/json".toMediaType()))
        .build()

    val uploadTemplate = Request.Builder()
        .url("https://fastreport.cloud/api/rp/v2/Templates/Folder/$templateFolder/File")
        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
        .addHeader("accept", "application/json")
        .addHeader("Content-Type", "multipart/form-data")
        .post(uploadRequestBody)
        .build()

    try {
        client.newCall(uploadTemplate).execute().use { response ->
            templateId = JSONObject(response.body!!.string()).getString("id")
            println("Uploading pdf!")
        }
    } catch (e: IOException) {
        println("Error: $e")
    }

    // Exporting pdf
    val jsonRequest = JSONObject().apply {
        put("\$t", "ExportTemplateVM")
        put("fileName", "Exported file")
        put("format", "Pdf")
        put("folderId", exportFolder)
        put("reportParameters", JSONObject().put("Parameter1", "Value"))
    }
    val body: RequestBody = jsonRequest.toString().toRequestBody(mediaType)
    val exportTemplate = Request.Builder()
        .url("https://fastreport.cloud/api/rp/v1/Templates/File/${templateId}/Export")
        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
        .post(body)
        .build()
    try {
        client.newCall(exportTemplate).execute().use { response ->
            exportId = JSONObject(response.body!!.string()).getString("id")
            println("Exporting pdf!")
        }
    } catch (e: IOException) {
        println("Error: $e")
    }

    // Downloading pdf
    val downloadPdf = Request.Builder()
        .url("https://fastreport.cloud/download/e/$exportId?preview=false")
        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
        .build()
    try {
        client.newCall(downloadPdf).execute().use { response ->
            val responseBody = response.body
            val inputStream = responseBody?.byteStream()
            val file1 = File("Exported file.pdf")
            file1.outputStream().use { output ->
                inputStream?.copyTo(output)
            }
            println("Downloading pdf")
        }
    } catch (e: IOException) {
        println("Error: $e")
    }
}