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

## Azure Serverless Conference Recording

This demo has been used in the Azure Serverless Conference 2021. Make sure to check out the recording and get the slides here:

- Slides: https://www.slideshare.net/davidemauri/azure-serverless-fullstack-kickstart
- Recording: https://www.youtube.com/watch?v=TIh52zbi8Dk 

![Recording Screenshot](./docs/screenshot.jpg)

## Fork the repo

To run this sample in your subscription, make sure to fork the repository into your organization or account. 

## Repo branches

This repo has different branches that shows the development at different stages. This is the branch 2.0.

- 1.0: First version, no database support
- 2.0: [This branch] Database support added

### V2.0 Notes

In this branch the backend REST API service is modified so that the to-do list can be saved an managed using an Azure SQL database. Communication with the database is done using JSON too, as Azure SQL support [JSON natively](https://docs.microsoft.com/en-us/sql/relational-databases/json/json-data-sql-server?view=sql-server-ver15). As an alternative, the same API using code [EF Core](https://learn.microsoft.com/en-us/ef/core/) is also provided.
A initial option to (imperatively) deploy the database is also used, either using manually applied database script, or via the EF Core migrations.

- [Folder Structure](./docs/run-and-deploy.md#folder-structure)
- [Install the dependencies](./docs/run-and-deploy.md#install-the-dependencies)
- [Create the Azure SQL database](./docs/run-and-deploy.md#create-the-azure-sql-database)
- [Deploy the database](./docs/run-and-deploy.md#deploy-the-database)
- [Test solution locally](./docs/run-and-deploy.md#test-solution-locally)
- [Deploy the solution on Azure](./docs/run-and-deploy.md#deploy-the-solution-on-azure)

### V1.0 Notes

In this branch the solution will have a full working front-end, sending REST request to the fully working backend REST API. The to-do list is saved in-memory using a List object. No authentication or authorization is supported.

## Next steps

Everything is working now, but data is not secured. In v3.0 authentication and authorization will be added to the solution to make it complete.
