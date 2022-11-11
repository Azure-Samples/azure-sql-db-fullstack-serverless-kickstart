#!/bin/bash
set -euo pipefail

echo "Preparing sample for v6.0..."

# cleanup API binaries
rm -rf ./api/bin
rm -rf ./api/obj

# cleanup CLIENT
rm -rf ./client/node_modules
rm -rf ./client/dist
rm ./client/index.html

# cleanup DATABASE
rm -rf ./database/deploy

# cleanup TEST
rm -rf ./test

echo "Done"