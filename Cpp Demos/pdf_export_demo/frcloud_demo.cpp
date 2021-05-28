#include <iostream>
#include <fstream>
#include <string>
#include "../ApiConfiguration.h"
#include "../api/TemplatesApi.h"
#include "../api/ExportsApi.h"
#include "../api/DownloadApi.h"

#include <boost/filesystem.hpp>
#include <unistd.h>

using namespace fastreport::cloud::client;
using namespace std;

extern string report;

static const string base64_chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    "abcdefghijklmnopqrstuvwxyz"
    "0123456789+/";

/* ==========================================================
        Base64 encoder
 ========================================================== */
static string base64_encode(unsigned char const* bytes_to_encode, unsigned int in_len) 
{
    std::string ret;
    int i = 0;
    int j = 0;
    unsigned char char_array_3[3];
    unsigned char char_array_4[4];

    while (in_len--) 
    {
        char_array_3[i++] = *(bytes_to_encode++);
        if (i == 3) {
            char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
            char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
            char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
            char_array_4[3] = char_array_3[2] & 0x3f;

            for (i = 0; (i < 4); i++)
                ret += base64_chars[char_array_4[i]];
            i = 0;
        }
    }

    if (i)
    {
        for (j = i; j < 3; j++)
            char_array_3[j] = '\0';

        char_array_4[0] = (char_array_3[0] & 0xfc) >> 2;
        char_array_4[1] = ((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4);
        char_array_4[2] = ((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6);
        char_array_4[3] = char_array_3[2] & 0x3f;

        for (j = 0; (j < i + 1); j++)
            ret += base64_chars[char_array_4[j]];

        while ((i++ < 3))
            ret += '=';
    }
    return ret;
}

/* ==========================================================
        Set session key (cloud authorization)
 ========================================================== */
static void set_key(shared_ptr<ApiClient> apiClient, const char * key)
{
    shared_ptr<ApiConfiguration> apiConfig(new ApiConfiguration);

    std::string  key_string = "apikey:";
    key_string += key;
    std::string  auth_string = "Basic " + base64_encode( (const unsigned char *)  key_string.c_str(), key_string.length() );

    apiConfig->setBaseUrl("https://fastreport.cloud");
    apiConfig->setApiKey("Authorization", auth_string );

    apiClient->setConfiguration(apiConfig);
}

/* ==========================================================
        Load report from file
 ========================================================== */
static bool set_report(shared_ptr<TemplateCreateVM> file_vm, const char * filename)
{
    ifstream report_stream(filename);
    if (!report_stream.is_open())
    {
        cerr << "Unable load file '" << filename << "'\n";
        return false;
    }
    std::stringstream buffer;
    buffer << report_stream.rdbuf();

    boost::filesystem::path path(filename);
    file_vm->setName(path.filename().c_str());
    file_vm->setContent( base64_encode( (const unsigned char *) buffer.str().c_str(), report_stream.tellg() ) );
    return true;
}

/* ==========================================================
        Generate export file name from report name
 ========================================================== */
static string get_export_filename(const char *filename, const char * ext)
{
    boost::filesystem::path path(filename);
    string result = path.stem().c_str();
    result += '.';
    result += ext;
    return result;
}

/* ==========================================================
        Set export properties
 ========================================================== */
static void config_export_task(
    shared_ptr<ExportTemplateTaskVM>  export_task, 
    utility::string_t export_folder_id,
    string export_file)
{
    export_task->setFolderId(export_folder_id);
    export_task->setFileName(export_file);
    export_task->setPagesCount(999);
    export_task->setFormat("Pdf");
}

/* ==========================================================
        Download prepared file
 ========================================================== */
static bool download_prepared_file(
    shared_ptr<ApiClient> apiClient, 
    utility::string_t export_id, 
    string file_name)
{
    DownloadApi 	downloads(apiClient);

    auto  pdf = downloads.downloadGetExport(export_id);
    auto  http_ptr = pdf.get();
    auto  stream = http_ptr->getData();

    //    cout << "Size = " << stream->gcount() << " Is valid = " << stream->good() << endl;

    if (!stream->good())
        return false;

    ofstream ofs(file_name, ios::binary); // binary mode!!
    ofs << stream->rdbuf();
    ofs.close();

    return true;
}

/* ==========================================================
        Programm entry point
 ========================================================== */
int main(int argc, char *argv[])
{
    const char * api_key = "9qptbsmjd114k93a4t9f8kh1tzfkzhq9udt8fwja5o45drgrcnmy";

    if (argc < 2)
    {
        cerr << "Use case:  ./frcloud report.frx" << endl;
        return 1;
    }

    const char * frx_file = argv[1]; 

    shared_ptr<ApiClient> apiClient(new ApiClient);
    
    set_key(apiClient, api_key);

    TemplatesApi  	templates(apiClient);
    ExportsApi 		exports(apiClient);


    shared_ptr<TemplateCreateVM>    file_vm(new TemplateCreateVM);

    if (!set_report(file_vm, frx_file))
    {
        cerr << "Unable open report template '" << frx_file << "'\n";
        return 2;
    }

    auto export_file_name = get_export_filename(frx_file, "pdf");

    auto export_root = exports.exportFoldersGetRootFolder(boost::none);
    auto root_folder_id = export_root.get()->getId();

    auto templates_folder = templates.templateFoldersGetRootFolder(boost::none);
    auto templates_folder_id = templates_folder.get()->getId();

    auto report_file = templates.templatesUploadFile(templates_folder_id, file_vm);
    auto report_id = report_file.get()->getId();

    shared_ptr<ExportTemplateTaskVM>  export_task(new ExportTemplateTaskVM);

    config_export_task(export_task, root_folder_id, export_file_name);

    auto export_code = templates.templatesExport(report_id, export_task);
    auto export_id = export_code.get()->getId();

    string status;
    for (int count = 0; count < 15; count++)
    {
        status = export_code.get()->getStatus();
        if (status == "Success" || status == "Failed")
            break;
        sleep(1);
        export_code = exports.exportsGetFile(export_id);
    }

    if (status != "Success")
    {
        cout << "Unable generate report. Server status: '" << status << "' Status reason: '" << export_code.get()->getStatusReason() << "'\n";
        return 3;
    }

    if (!download_prepared_file( apiClient, export_id, export_file_name))
    {
        cerr << "Unable download file" << endl;
        return 4;
    }

    return  0;
}

