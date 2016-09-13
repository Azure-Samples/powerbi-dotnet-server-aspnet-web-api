# powerbi-dotnet-server-aspnet-web-api
Sample WebAPI application demonstrating use of [PowerBI-CSharp](https://github.com/Microsoft/PowerBI-CSharp) SDK for creating embed REST API.

View demo: powerbipaasapi.azurewebsites.net

## Running this sample

Pre-requisites:

You must have a workspace collection provisioned in azure.  For infromation about provisioning see: https://azure.microsoft.com/en-us/documentation/articles/power-bi-embedded-get-started/

1. Clone repository

	```
	git clone https://github.com/Azure-Samples/powerbi-dotnet-server-aspnet-web-api.git
	```

2. Set properties in Web.config from Azure Portal.

	```
	<add key="powerbi:WorkspaceCollectionName" value="[your workspace collection name]" />
	<add key="powerbi:WorkspaceCollectionAccessKey" value="[your workspace collection access key]" />
	<add key="powerbi:WorkspaceId" value="[workspace id containing your reports]" />
	```

3. Build and Run solution 

## Deploy this sample to Azure
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

## About the code
See: [PowerBI-CSharp](https://github.com/Microsoft/PowerBI-CSharp) for details about usage of NuGet packages to facilitate creating tokens for authentication.

## More information
We're interested in feedback.  Open a [new issue](https://github.com/Azure-Samples/powerbi-dotnet-server-aspnet-web-api/issues/new) if you have requests or find bugs.
