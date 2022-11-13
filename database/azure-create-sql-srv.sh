#!/bin/bash
set -euo pipefail

# Load values from .env file
FILE="../.env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
    eval $(egrep "^[^#;]" $FILE | tr '\n' '\0' | xargs -0 -n1 | sed 's/^/export /')
else
	echo "Enviroment file not detected."
	echo "Please make sure there is a .env file in the sample root folder and run the script again."
	exit 1
fi

echo "Creating Resource Group...";
az group create \
    -n $resourceGroup \
    -l $location

echo "Deploying Azure SQL Logical Server...";
pwd1=`cat /dev/urandom | tr -dc A-Za-z0-9 | head -c 6 ; echo`
pwd2=`cat /dev/urandom | tr -dc A-Za-z0-9 | head -c 6 ; echo`
adminPwd="${pwd1}_${pwd2}"
adminName="db_admin"
azureSQLServer=$(az deployment group create \
    --name "sql-srv-deploy-3.0" \
    --resource-group $resourceGroup \
    --template-file azure-sql-srv.arm.json \
    --parameters \
        location=$location \
        databaseAdministratorLogin=$adminName \
		databaseAdministratorLoginPassword=$adminPwd \
    --query properties.outputs.databaseServer.value \
    -o tsv \
    )

echo "Azure SQL Database available at";
echo "- Location: $location"
echo "- Server: $azureSQLServer"
echo "- Admin Login: $adminName"
echo "- Admin Password: $adminPwd"
echo "Done."