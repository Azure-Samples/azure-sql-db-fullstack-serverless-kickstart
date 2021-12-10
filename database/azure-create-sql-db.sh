#!/bin/bash
set -euo pipefail

# Load values from .env file or create it if it doesn't exists
FILE="../.env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
    export $(egrep "^[^#;]" $FILE | xargs -n1)
else
	echo "Enviroment file not detected."
	echo "Please make sure there is a .env file in the sample root folder and run the script again."
	exit 1
fi

echo "Creating Resource Group...";
az group create \
    -n $resourceGroup \
    -l $location

echo "Deploying Azure SQL Database...";
azureSQLDB="todo_v3"
azureSQLServer=$(az deployment group create \
    --name "sql-db-deploy-2.0" \
    --resource-group $resourceGroup \
    --template-file azure-sql-db.arm.json \
    --parameters \
        databaseName=$azureSQLDB \
        location=$location \
    --query properties.outputs.databaseServer.value \
    -o tsv \
    )

echo "Azure SQL Database available at";
echo "Location: $location"
echo "Server: $azureSQLServer"
echo "Database: $azureSQLDB"
echo "Done."