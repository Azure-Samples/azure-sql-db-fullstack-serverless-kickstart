#!/bin/bash
set -euo pipefail

# Load values from .env file in the root folder
FILE=".env"
if [[ -f $FILE ]]; then
	echo "Loading from $FILE" 
    eval $(egrep "^[^#;]" $FILE | tr '\n' '\0' | xargs -0 -n1 | sed 's/^/export /')
else
	echo "Enviroment file not detected."
	echo "Please make sure there is a .env file in this folder that sets the 'ConnectionString' environment variable, and run the script again."
	exit 1
fi

echo "Building .dacpac..."
dotnet build /p:NetCoreBuild=true todo_v6

echo "Publishing .dacpac..."
sqlpackage /Action:Publish /SourceFile:./todo_v6/bin/Debug/todo_v6.dacpac /TargetConnectionString:"$ConnectionString"

echo "Done."