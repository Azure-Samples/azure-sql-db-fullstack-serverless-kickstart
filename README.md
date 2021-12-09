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

This demo has been used in the Azure Serverless Conference 2021. Make sure to check out the recording and get the slides here:

- Slides: https://www.slideshare.net/davidemauri/azure-serverless-fullstack-kickstart
- Recording: https://www.youtube.com/watch?v=TIh52zbi8Dk 

![Recording Screenshot](./docs/screenshot.jpg)

## Fork the repo

To run this sample in your subscription, make sure to fork the repository into your organization or account. 

## Repo branches

This repo has three branches that shows the development at different stages

- 1.0: First version, no database support
- 2.0: [This branch] Database support added
- 3.0: Authentication and Authorization 

### V2.0 Notes

TDB

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

Before starting the solution locally, you have to configure the Azure Function that is used to provide the backed API. In the `./api` folder create a `local.settings.json` file starting from the provided template. To run Azure Functions locally, you also need a local Azure Storage emulator. You can use [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio) that also has a VS Code extension.

Make sure Azurite is running and then start the Azure Static Web App emulator:

```sh
swa start ./client --api--location ./api    
```

and you'll be good to go.

once this text will appear:

```
Azure Static Web Apps emulator started at http://localhost:4280. Press CTRL+C to exit.
```

everything will be up and running. Go the the indicated URL and you'll see the ToDo App. Go an play with it, it will work perfectly, having the Vue.js frontend calling the REST API provided by the Azure Function and storing the to-do list in a List object. 

## Deploy the solution on Azure

Now that you know everything works fine, you can deploy the solution to Azure. You can take advantage of the script `./azure-deploy.sh` that will deploy the Azure Static Web app for you.

The first time the script will run will create an empty `.env` file in the sample root folder that you have to fill out. Aside from the usual Azure information like the resource group, the location and the app name, you also have to provide a [GitHub Token](https://docs.microsoft.com/en-us/azure/static-web-apps/publish-azure-resource-manager?tabs=azure-cli#create-a-github-personal-access-token).

The GitHub Token is needed as Azure Static Web App will create a GitHub action in your repository in order to automate deployment of the solution to Azure. That is right: every time you'll push a code change to your code main code branch, the application will also be re-built and deployed in Azure.

Run the `./azure-deploy.sh` script and the Azure Static Web app will be deployed in specified resource group. You can run the script using [WSL](https://docs.microsoft.com/en-us/windows/wsl/), or Linux or [Azure Cloud Shell](https://azure.microsoft.com/en-us/features/cloud-shell/).

## Run the solution on Azure

Once the deployment script has finished, you can go to the created Azure Static Web App in the Azure Portal and you can see it as been connected to the specified GitHub repository. Azure Static Web App has also created a new workflow in the GitHub repository that uses GitHub Actions to define the CI/CD pipeline that will build and publish the website every time a commit is pushed to the repo.

An example of the Azure Static Web App url you'll get is something like:

https://victorious-rock-01b2b501e.azurestaticapps.net/ 

The first time you'll visit the URL you might not see any to-do item, even if a couple are already inserted in the in-memory list as an example. This is due the fact that the Azure Function running behind the scenes can take several seconds to start up the first time.

## Next steps

Now that the solution is working nicely, it is time to add the database to the picture. Branch 2.0 will guide you in doing that.