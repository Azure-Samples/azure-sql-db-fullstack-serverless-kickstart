#!/bin/bash
set -euo pipefail

# Load values from .env file in the root folder
FILE=".env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
    export $(egrep "^[^#;]" $FILE | xargs -n1)
else
	echo "Enviroment file not detected."
	echo "Please make sure there is a .env file in the sample root folder and run the script again."
	exit 1
fi

echo "Building .dacpac..."
dotnet build /p:NetCoreBuild=true todo_v5

echo "Publishing .dacpac..."
sqlpackage /Action:Publish /SourceFile:./todo_v5/bin/Debug/todo_v5.dacpac /TargetConnectionString:$ConnectionString

echo "Done."