# FastReport Cloud API Web Demo

For this demo application to work, you need to configure ngrok.

1. Download the utility from the official website: https://dashboard.ngrok.com/get-started/setup

2. Run the following command in ngrok to expose port 5001 to the network - `ngrok http https://localhost:5001 -host-header="localhost:5001"`  

Now, you need to configure the report in the Resources folder.

1. Comment out the uploadAndExport.Initialize() line in the HomeController constructor (line 32)

2. Launch the demo application

3. While the application is running, edit the connection string in the report using the designer (Go to Data Sources >
   Right-click on Connection > Edit... >
   Replace the connection address with the ngrok URL shown in the ngrok console interface.)

4. If everything works correctly, save the report and close the application

5. Uncomment the previously commented line and relaunch the application

## Documentation

For implementation details using FastReport Cloud, see:
- [FastReport Cloud Official Documentation](https://www.fast-report.com/public_download/docs/Cloud/online/en/user/en-US/user/index.html)

## Support 

Contact us with any questions using [our website](https://www.fast-report.com/en/support/) or [GitHub issues](https://github.com/FastReports/FastReport-Cloud/issues). 

## Products
- [FastReport Cloud](https://www.fast-report.com/products/cloud)
- [FastReport Corporate Server](https://www.fast-report.com/products/corporate-server)

