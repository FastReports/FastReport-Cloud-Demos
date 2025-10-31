# FastReport.Cloud C++ Demo

## Configuration

You should obtain apikey and write it to the source code as shown below.


```
int main(int argc, char *argv[])
{
    const char * api_key = "---- set API-KEY here -----";
```

## Building

1. Ensure that boost, cpprest, and crypto libraries are available on your system.
2. Run `make`

## Usage

Run `frcloud report.frx` where report.frx is a report template. This template will be uploaded to server, prepared, exported to PDF, 
and downloaded to a current folder.
Program exit code is 0 on success. Any other return value is error.

## SDK
This application uses FastReport Cloud C++ SDK:
- [FastReport Cloud Cpp SDK](https://github.com/FastReports/FastReport-Cloud-Cpp)

## Documentation
For implementation details using FastReport Cloud, see:
- [FastReport Cloud Official Documentation](https://www.fast-report.com/public_download/docs/Cloud/online/en/user/en-US/user/index.html)

## Support 

Contact us with any questions using [our website](https://www.fast-report.com/en/support/) or [GitHub issues](https://github.com/FastReports/FastReport-Cloud/issues). 

## Products
- [FastReport Cloud](https://www.fast-report.com/products/cloud)
- [FastReport Corporate Server](https://www.fast-report.com/products/corporate-server)
