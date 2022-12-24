#!/bin/bash
set -euo pipefail

# Load values from .env file in the root folder
FILE="../.env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
    eval $(egrep "^[^#;]" $FILE | tr '\n' '\0' | xargs -0 -n1 | sed 's/^/export /')
else
	echo "Enviroment file not detected."
	echo "Please make sure there is a .env file in the sample root folder and run the script again."
	exit 1
fi

azureSQLDB="todo_v6"
azureSQLSRV=`az sql server list -g $resourceGroup --query '[0].name' -o tsv`
echo "(Server: '$azureSQLSRV', Location: '$location', Resource Group: '$resourceGroup')"
azureSQLServerName=$(az deployment group create \
    --name "sql-db-deploy-6.0" \
    --resource-group $resourceGroup \
    --template-file azure-sql-db.arm.json \
    --parameters \
        databaseServer=$azureSQLSRV \
        databaseName=$azureSQLDB \
        location=$location \
    --query properties.outputs.databaseServer.value \
    -o tsv \
    )

echo "Azure SQL Database available at";
echo "- Location: $location"
echo "- Server: $azureSQLServerName"
echo "- Database: $azureSQLDB"
echo "Done."