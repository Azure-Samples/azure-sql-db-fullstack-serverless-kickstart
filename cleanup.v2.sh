#!/bin/bash
set -euo pipefail

echo "Preparing sample for v2.0..."

# cleanup API binaries
echo "Cleaning ./api ..."
rm -rf ./api/bin
rm -rf ./api/obj
rm -rf ./api/Migrations

# cleanup CLIENT
echo "Cleaning ./client ..."
rm -rf ./client/node_modules
rm -rf ./client/dist

# cleanup DATABASE
echo "Cleaning ./database ..."
rm -rf ./database/declarative-deploy
rm -rf ./database/imperative-deploy
rm -rf ./database/deploy

# cleanup TEST
echo "Cleaning ./test ..."
rm -rf ./test

echo "Done"