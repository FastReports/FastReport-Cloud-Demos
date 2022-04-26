<?php
require_once('./vendor/autoload.php');

$config = OpenAPI\Client\Configuration::getDefaultConfiguration()
   ->setHost("https://fastreport.cloud");

$templatesInstance = new OpenAPI\Client\client\TemplatesApi(
// If you want use custom http client, pass your client which implements `GuzzleHttp\ClientInterface`.
// This is optional, `GuzzleHttp\Client` will be used as default.
    new GuzzleHttp\Client([
        'auth' => ['apikey', '{apikey}'],  // we need to specify apikey here
    ]),
    $config
);

try {
    $rootFolderId = "{rootFolderId}";
    $folderAndFilesCount = $templatesInstance->templateFolderAndFileGetCount($rootFolderId);

    print_r($folderAndFilesCount);
} catch (Exception $e) {
    echo 'Exception when calling ApiKeysApi->apiKeysGetApiKeys: ', $e->getMessage(), PHP_EOL;
}
