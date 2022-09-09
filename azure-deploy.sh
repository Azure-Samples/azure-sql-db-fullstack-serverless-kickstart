#!/bin/bash
set -euo pipefail

# Load values from .env file or create it if it doesn't exists
FILE=".env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
  eval $(egrep "^[^#;]" $FILE | tr '\n' '\0' | xargs -0 -n1 | sed 's/^/export /')
else
	cat << EOF > .env
resourceGroup=""
appName=""
location=""

gitSource="https://github.com/Azure-Samples/azure-sql-db-fullstack-serverless-kickstart"
gitToken=""
EOF
	echo "Enviroment file not detected."
	echo "Please configure values for your environment in the created .env file"
	echo "and run the script again."
	exit 1
fi

echo "Creating Resource Group...";
az group create \
    -n $resourceGroup \
    -l $location

echo "Deploying Static Web App...";
az deployment group create \
  --name "swa-deploy-1.0" \
  --resource-group $resourceGroup \
  --template-file azure-deploy.arm.json \
  --parameters \
    name=$appName \
    location=$location \
    repositoryToken=$gitToken \
    repositoryUrl=$gitSource \
    branch="v1.0" \
    appLocation="./client" \
    apiLocation="./api"

echo "Getting Static Web App...";
dhn=`az staticwebapp show -g $resourceGroup -n $appName --query "defaultHostname"`
echo "Static Web App created at: $dhn";

echo "Done."