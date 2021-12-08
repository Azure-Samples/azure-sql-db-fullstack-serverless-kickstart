---
page_type: sample
languages:
  - csharp
  - tsql
  - sql
products:
  - azure
  - vs-code
  - azure-sql-database
  - azure-functions
  - azure-web-apps
  - azure-sql-database
  - azure-app-service-static
  - azure-app-service-web
  - dotnet
  - dotnet-core
  - azure-app-service-web
description: 'Full-Stack Kickstart with Azure Static WebApps, Azure Functions, .NET Core, Vue.Js and Azure SQL'
urlFragment: 'azure-sql-db-fullstack-serverless-kickstart'
---

<!--
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

# Serverless Full-Stack Kickstart 

![License](https://img.shields.io/badge/license-MIT-green.svg)

Learn how to implement a fully working, end-to-end, full-stack solution using Azure Static Web Apps, Azure Functions and Azure SQL Serverless. In this session we’ll see and build together the simple (but not too simple!) To-Do list reference app, using Vue.js, CI/CD and more! 

## Azure Serverless Conference 2021

This demo has been used in the Azure Serverless Conferece 2021. Make sure to check out the recording and get the slides here:

- Slides: https://www.slideshare.net/davidemauri/azure-serverless-fullstack-kickstart
- Recording: https://www.youtube.com/watch?v=TIh52zbi8Dk 

## Fork the repo

To run this sample in your subscription, make sure to fork the repository into your organization or account. 

## Repo branches

This repo has three branches that shows the development at different stages

- 1.0: No database used
- 2.0: ...
- 3.0: ...

## Folder Structure

- `/api`: the NodeJs Azure Function code used to provide the backend API, called by the Vue.Js client. 
- `/client`: the Vue.Js client. Original source code has been taken from official Vue.js sample and adapted to call a REST client instead of using local storage to save and retrieve todos
- `/database`: the database scripts and the database deployment tool

## Install the dependencies

Make sure you have [Node](https://nodejs.org/en/download/) as it is required by Azure Functions Core Tools and also by Azure Static Web Apps. The backend API will be using .NET Core, but Node is needed to have the local development experience running nicely.

Also install the [Azure Function Core Tools v3](https://www.npmjs.com/package/azure-functions-core-tools):

```sh
npm i -g azure-functions-core-tools@3 --unsafe-perm true
```

Also install the [Azure Static Web Apps CLI](https://github.com/azure/static-web-apps-cli):

```sh
npm install -g @azure/static-web-apps-cli`
```

## Create the Azure SQL database

If you don't have a Azure SQL server already, you can create one (no additional costs for a server) running the following [AZ CLI](https://docs.microsoft.com/en-us/cli/azure/) command (via [WSL](https://docs.microsoft.com/en-us/windows/wsl/), or Linux or [Azure Cloud Shell](https://azure.microsoft.com/en-us/features/cloud-shell/)):


```sh
az sql server create -n <server-name> -l <location> --admin-user <admin-user> --admin-password <admin-password> -g <resource-group>
 ```

Create a new Azure SQL database:

```sh
az sql db create -g <resource-group> -s <server-name> -n todo_dev --service-objective S0
```

Make sure you have the firewall configured to allow your machine to access Azure SQL:

```
az sql server firewall-rule create --resource-group <resource-group> --server <server-name> --name AllowMyClientIP_1 --start-ip-address <your_public_ip> --end-ip-address <your_public_ip>
```

you can get your public IP from here, for example: https://ifconfig.me/

## Deploy the database

Database is deployed using [DbUp](http://dbup.github.io/). Switch to the `./database/deploy` folder and create new `.env` file containg the connection string to the created Azure SQL database. You can get the connection string from the Azure Portal or by running:

```sh
az sql db show-connection-string --server <server-name> --client ado.net
```

which will return something like:

```
Server=tcp:dmmssqlsrv.database.windows.net,1433;Database=<databasename>;User ID=<username>;Password=<password>;Encrypt=true;Connection Timeout=30;
```

replace the placeholder with the correct value for your database, username and password and you're good to go. Make sure the database user specified in the connection string has enough permission to create objects (for example, make sure is a server administrator or in the db_owner database role) run the deployment application:

```
cd ./database/deploy
dotnet run
```

you will see something like: 

```
Deploying database: todo_dev
Testing connection...
Starting deployment...
Beginning database upgrade
Checking whether journal table exists..
Fetching list of already executed scripts.
Executing Database Server script '02-update-todo-table.sql'
Checking whether journal table exists..
Executing Database Server script '03-update-stored-procs.sql'
Executing Database Server script '04-move-to-long-user-id.sql'
Executing Database Server script '05-update-stored-procs-support-long-user-id.sql'
Upgrade successful
Success!
```

Database has been deployed successfully!

## Test solution locally

Before starting the solution locally, you have to configure the Azure Function that is used to provide the backed API. In the `./api` folder create a `local.settings.json` file starting from the provided template. All you have to do is update the connection string with the value correct for you solution. If have created the Azure SQL database as described above you'll have a database named `todo_dev`. Just make sure you add the correct server name in the `local.settings.json`. The database name, user login and password are already set in the template file to match those used in this repository and in the `./database/sql/01-create-objects.sql` file.

Start the Azure Static Web App emulator:

```sh
swa start ./client --api--location ./api    
```

and you'll be good to go.

once this text will appear:

```
Azure Static Web Apps emulator started at http://localhost:4280. Press CTRL+C to exit.
```

everything will be up and running. Go the the indicated URL and you'll see the ToDo App. Go an play with it, it will work perfectly, sending your todos to the created Azure SQL DB, by calling the REST API provided by the Azure Function. 

## Authorization and Authentication

The sample supports authentication and authorization. Just click on the login and you'll be brought to an [emulated GitHub authentication provider, as described here](https://github.com/Azure/static-web-apps-cli#local-authentication--authorization-emulation).

## Deploy the solution on Azure

Now that you know everything works fine, you can deploy the solution to Azure. You can take advantage of the script `./azure-deploy.sh` that will deploy the Azure Static Web app for you.

The first time the script will run will create an empty `.env` file in the sample root folder that you have to fill out. Aside from the usual Azure informations like the resource group, the location and the app name, you also have to provide a [GitHub Token](https://docs.microsoft.com/en-us/azure/static-web-apps/publish-azure-resource-manager?tabs=azure-cli#create-a-github-personal-access-token).

The GitHub Token is needed as Azure Static Web App will create a GitHub action in your repository in order to automate deployment of the solution to Azure. That is right: every time you'll push a code change to your code main code branch, the application will also be re-built and deployed in Azure.

### Adding the database to the CI/CD pipeline

The generated GitHub Action doesn't know that we are using a database to store to-do data, so we need to add the database deployment to the GitHub Action manually. No big deal, is a very small change. First of all you have to create three new [GitHub secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets):

- AZURE_CREDENTIALS
- AZURE_RESOURCE_GROUP
- AZURE_SQL_SERVER
- AZURE_SQL_CONNECTION_STRING

The `AZURE_RESOURCE_GROUP` is the resource group where your database will been created. The `AZURE_SQL_CONNECTION_STRING` is the connection string that can be used to deploy the database. You can use the same used to 

This is the code you need to add to the file you'll find in `./.github/workflow`.

```yaml
- name: Setup .NET Core
  uses: actions/setup-dotnet@v1
  with:
    dotnet-version: '5.0.x' 
- name: Azure Login
  uses: Azure/login@v1
  with:
    creds: ${{ secrets.AZURE_CREDENTIALS }}
- name: Create Database
  uses: Azure/arm-deploy@v1
  with:          
    resourceGroupName: ${{ secrets.AZURE_RESOURCE_GROUP }}
    region: CentralUS
    deploymentMode: Incremental   
    template: "./database/azure-sql-db-serverless.arm.json"
- name: Deploy Database
  working-directory: ./database/deploy
  env: 
    ConnectionString: ${{ secrets.AZURE_SQL_CONNECTION_STRING }}    
  run: dotnet build && dotnet run      
```


ref:to https://github.com/Azure/arm-deploy

