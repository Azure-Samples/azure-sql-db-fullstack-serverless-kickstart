import * as fs from "fs/promises";
import * as cp from "child_process";
import * as util from "util";

const exec = util.promisify(cp.exec);

// Change these as needed.
const BRANCH = "v3.0";
const CLIENT_DIR = "./client";
const API_DIR = "./api";

const ENV_TEMPLATE = `resourceGroup=""
appName=""
location=""
# Connection string
azureSQL='Server=tcp:.database.windows.net,1433;Initial Catalog=todo_v3;Persist Security Info=False;User ID=webapp;Password=Super_Str0ng*P4ZZword!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
gitSource="https://github.com/Azure-Samples/azure-sql-db-fullstack-serverless-kickstart"
gitToken=""`;

async function getEnv() {
    try {
        const data = await fs.readFile(".env", "utf8");
        const env = data
            .replace(/\r\n/g, "\n")
            .split("\n")
            .filter((str) => !/^#/.test(str))
            .reduce((acc, val) => {
                const [ key, ...vals ] = val.split("=");
                const newVal = vals.join("=");
                acc[key] = newVal;
                return acc;
            }, {});
        return env;
    } catch (err) {
        if (err.code !== 'ENOENT') {
            throw err;
        }
        await fs.writeFile(".env", ENV_TEMPLATE);
        console.log("Enviroment file not detected.");
        console.log("Please configure values for your environment in the created .env file");
        console.log("and run the script again.");
        process.exit(1);
    }
}

async function main() {
    const env = await getEnv();

    console.log("Creating Resource Group...");
    await exec(`az group create -n ${env["resourceGroup"]} -l ${env["location"]}`);

    console.log("Deploying Static Web App...");
    await exec([
        "az deployment group create",
        "--name \"swa-deploy-3.0\"",
        `--resource-group ${env["resourceGroup"]}`,
        "--template-file azure-deploy.arm.json",
        "--parameters",
        `name=${env["appName"]}`,
        `location=${env["location"]}`,
        `repositoryToken=${env["gitToken"]}`,
        `repositoryUrl=${env["gitSource"]}`,
        `branch="${BRANCH}"`,
        `appLocation="${CLIENT_DIR}"`,
        `apiLocation="${API_DIR}"`,
        `azureSQL="$azureSQL"`
    ].join(" "));

    console.log("Getting Static Web App...");
    const { stdout } = await exec(
        `az staticwebapp show -g ${env["resourceGroup"]} -n ${env["appName"]} --query "defaultHostname"`
    );
    console.log(`Static Web App created at: ${stdout}`);

    process.exit(0);
}

main().catch((err) => {
    console.error(err);
    process.exit(1);
});
