package cloud.fastreport.androiddemo

import android.os.Bundle
import android.os.Environment
import android.widget.Button
import android.widget.EditText
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import okhttp3.Credentials
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.MultipartBody
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody
import okhttp3.RequestBody.Companion.toRequestBody
import java.io.IOException
import org.json.JSONObject
import java.io.File

class MainActivity : AppCompatActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        var apiKey = ""
        var templateId = ""
        var exportId = ""
        var subId = ""
        var templateFolder = ""
        var exportFolder = ""

        val toast = Toast.makeText(this, "Success!", Toast.LENGTH_SHORT)
        val toastError = Toast.makeText(this, "Error!", Toast.LENGTH_SHORT)
        val client = OkHttpClient()
        val mediaType = "application/json; charset=utf-8".toMediaType()

        val uploadButton: Button = findViewById(R.id.uploadButton)
        uploadButton.setOnClickListener {
            val editTextInputApiKey: EditText = findViewById(R.id.editTextInputApiKey)
            apiKey = editTextInputApiKey.text.toString()
            val editTextInputSubId: EditText = findViewById(R.id.editTextInputSubId)
            subId = editTextInputSubId.text.toString()

            GlobalScope.launch {
                try {
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

                    val fileName = "Template.frx"
                    val fileContent = assets.open(fileName).bufferedReader().use { it.readText() }
                    val requestBody = MultipartBody.Builder()
                        .setType(MultipartBody.FORM)
                        .addFormDataPart(
                            "FileContent",
                            fileName,
                            fileContent.toRequestBody(mediaType)
                        )
                        .build()
                    val request = Request.Builder()
                        .url("https://fastreport.cloud/api/rp/v2/Templates/Folder/${templateFolder}/File")
                        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                        .addHeader("Accept", "application/json")
                        .post(requestBody)
                        .build()
                    client.newCall(request).execute().use { response ->
                        if (!response.isSuccessful) {
                            throw IOException("Error: ${response.code} ${response.message}")
                        }
                        templateId = JSONObject(response.body!!.string()).getString("id")
                    }
                    toast.show()
                } catch (e: Exception) {
                    e.printStackTrace()
                    toastError.show()
                }
            }
        }

        val exportButton: Button = findViewById(R.id.exportButton)
        exportButton.setOnClickListener {
            GlobalScope.launch {
                try {
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

                    val editParameterKey: EditText = findViewById(R.id.editParameterKey)
                    val parameterKey = editParameterKey.text.toString()
                    val editParameterValue: EditText = findViewById(R.id.editParameterValue)
                    val parameterValue = editParameterValue.text.toString()

                    val jsonRequest = JSONObject().apply {
                        put("\$t", "ExportTemplateVM")
                        put("fileName", "Exported file")
                        put("format", "Pdf")
                        put("folderId", exportFolder)
                        put("reportParameters", JSONObject().put(parameterKey, parameterValue))
                    }
                    val body: RequestBody = jsonRequest.toString().toRequestBody(mediaType)
                    val request = Request.Builder()
                        .url("https://fastreport.cloud/api/rp/v1/Templates/File/${templateId}/Export")
                        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                        .post(body)
                        .build()
                    client.newCall(request).execute().use { response ->
                        if (!response.isSuccessful) {
                            toastError.show()
                        }
                        exportId = JSONObject(response.body!!.string()).getString("id")
                    }
                    toast.show()
                } catch (e: Exception) {
                    e.printStackTrace()
                    toastError.show()
                }
            }
        }

        val downloadButton: Button = findViewById(R.id.downloadButton)
        downloadButton.setOnClickListener {
            GlobalScope.launch {
                try {
                    val downloadPdf = Request.Builder()
                        .url("https://fastreport.cloud/download/e/$exportId?preview=false")
                        .addHeader("Authorization", Credentials.basic("apikey", apiKey))
                        .build()
                    try {
                        client.newCall(downloadPdf).execute().use { response ->
                            if (!response.isSuccessful) {
                                throw IOException("Error: ${response.code} ${response.message}")
                            }

                            val responseBody = response.body
                            val inputStream = responseBody?.byteStream()
                            val downloadsPath =
                                Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS)
                            val file1 = File(downloadsPath, "Exported file.pdf")
                            file1.outputStream().use { output ->
                                inputStream?.copyTo(output)
                            }
                        }
                        toast.show()
                    } catch (e: IOException) {
                        toastError.show()
                    }
                } catch (e: Exception) {
                    e.printStackTrace()
                    toastError.show()
                }
            }
        }
    }
}

