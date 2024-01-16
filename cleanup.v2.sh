#!/bin/bash
set -euo pipefail

echo "Preparing sample for v1.0..."

# cleanup API binaries
rm -rf ./api/bin
rm -rf ./api/obj

# cleanup CLIENT
rm -rf ./client/node_modules
rm -rf ./client/dist
rm -rf ./client/src

# cleanup DATABASE
rm -rf ./database/declarative-deploy
rm -rf ./database/imperative-deploy
rm -rf ./database/deploy/bin
rm -rf ./database/deploy/obj

# cleanup TEST
rm -rf ./test

echo "Done"