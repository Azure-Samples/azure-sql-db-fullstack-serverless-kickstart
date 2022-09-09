#!/bin/bash

# Strict mode, fail on any error
set -euo pipefail

# Load values from .env file in the root folder
FILE="../.env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
    eval $(egrep "^[^#;]" $FILE | xargs -0 -n1 | sed 's/^/export /')
else
	echo "Enviroment file not detected."
	echo "Please make sure there is a .env file in the sample root folder and run the script again."
	exit 1
fi

# Local .env file with local settings for locust
FILE=".env"
if [[ -f $FILE ]]; then
	echo "loading from .env" | tee -a log.txt
    eval $(egrep "^[^#;]" $FILE | xargs -0 -n1 | sed 's/^/export /')
else
	cat << EOF > .env
HOST="https://jsonplaceholder.typicode.com"
TEST_CLIENTS=1
USERS_PER_CLIENT=10
SPAWN_RATE=1
EOF
	echo "Enviroment file not detected."
	echo "Please configure values for your environment in the created .env file"
	echo "and run the script again."
	echo "HOST: REST Endpoint to test"
	echo "TEST_CLIENTS: Number of locust client to create"
	echo "USERS_PER_CLIENT: Number of users that each locust client will simulate"
	echo "SPAWN_RATE: How many new users will be created per second per locust client"
	exit 1
fi

echo "starting"
cat << EOF > log.txt
EOF

echo "using resource group: $resourceGroup" | tee -a log.txt

AZURE_STORAGE_ACCOUNT=locust$RANDOM

echo "creating storage account: $AZURE_STORAGE_ACCOUNT" | tee -a log.txt
az storage account create -n $AZURE_STORAGE_ACCOUNT -g $resourceGroup --sku Standard_LRS \
	-o json >> log.txt	
	
echo "retrieving storage connection string" | tee -a log.txt
AZURE_STORAGE_CONNECTION_STRING=$(az storage account show-connection-string --name $AZURE_STORAGE_ACCOUNT -g $resourceGroup -o tsv)

echo 'creating file share' | tee -a log.txt
az storage share create -n locust --connection-string $AZURE_STORAGE_CONNECTION_STRING \
	-o json >> log.txt

echo 'uploading simulator scripts' | tee -a log.txt
az storage file upload-batch --destination locust --source locust --connection-string $AZURE_STORAGE_CONNECTION_STRING --pattern *.py \
    -o json >> log.txt

echo "deploying locust ($TEST_CLIENTS clients)..." | tee -a log.txt
LOCUST_MONITOR=$(az deployment group create -g $resourceGroup \
	--template-file locust-arm-template.json \
	--parameters \
		host=$HOST \
		storageAccountName=$AZURE_STORAGE_ACCOUNT \
		fileShareName=locust \
		numberOfInstances=$TEST_CLIENTS \
	--query properties.outputs.locustMonitor.value \
	-o tsv \
	)
sleep 10

echo "locust: endpoint: $LOCUST_MONITOR" | tee -a log.txt

echo "locust: starting ..." | tee -a log.txt
declare USER_COUNT=$(($USERS_PER_CLIENT*$TEST_CLIENTS))
declare SPAWN_RATE=$(($SPAWN_RATE*$TEST_CLIENTS))
echo "locust: users: $USER_COUNT, spawn rate: $SPAWN_RATE"
curl -fsL $LOCUST_MONITOR/swarm -X POST -F "user_count=$USER_COUNT" -F "spawn_rate=$SPAWN_RATE" >> log.txt

echo "locust: master available at: $LOCUST_MONITOR" | tee -a log.txt

echo "done" | tee -a log.txt