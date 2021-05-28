# FastReport.Cloud C++ demo

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
