#!/bin/bash
set -euo pipefail

echo "Preparing sample for v2.0..."

# cleanup API binaries
echo "Cleaning ./api ..."
rm -rf ./api/bin
rm -rf ./api/obj

# cleanup CLIENT
echo "Cleaning ./client ..."
rm -rf ./client/node_modules
rm -rf ./client/dist

# cleanup DATABASE
echo "Cleaning ./database ..."
rm -rf ./database

# cleanup TEST
echo "Cleaning ./test ..."
rm -rf ./test

echo "Done"