import * as cp from "child_process";
import * as fs from "fs/promises";
import * as path from "path";
import * as url from 'url';
import * as util from "util";

// Change these as needed.
const ENV_FILE = "../.env";
const NAME = "sql-db-deploy-3.0";
const DATABASE = "todo_v3";
const TEMPLATE_FILE = "azure-sql-db.arm.json";

const exec = util.promisify(cp.exec);
const __dirname = path.dirname(url.fileURLToPath(import.meta.url));

async function getEnv(filename) {
    try {
        const data = await fs.readFile(filename, "utf8");
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
        console.log("Enviroment file not detected.");
        console.log("Please make sure there is a .env file in the sample root folder and run the script again.");
        process.exit(1);
    }
}

async function main() {
    const envFile = path.resolve(__dirname, ENV_FILE);
    const env = await getEnv(envFile);
    const resourceGroup = env["resourceGroup"] || process.env.resourceGroup;
    const location = env["location"] || process.env.location;
    const templateFile = path.resolve(__dirname, TEMPLATE_FILE);

    const createStatement = `az group create -n ${resourceGroup} -l ${location}`;
    const deployStatement = [
        "az deployment group create",
        `--name "${NAME}"`,
        `--resource-group ${resourceGroup}`,
        `--template-file ${templateFile}`,
        "--parameters",
        `databaseName=${DATABASE}`,
        `location=${location}`,
        "--query properties.outputs.databaseServer.value",
        "-o tsv"
    ].join(" ");

    console.log("Creating Resource Group...");
    await exec(createStatement);

    console.log("Deploying Azure SQL Database...");
    const { stdout } = await exec(deployStatement);

    const azureSQLServer = stdout.trim();

    console.log("Azure SQL Database available at");
    console.log(`Location: ${location}`);
    console.log(`Server: ${azureSQLServer}`);
    console.log(`Database: ${DATABASE}`);
    console.log("Done.");
}

main().catch((err) => {
    if (err.stderr) {
        console.error(`Error: ${err.stderr}`);
    } else {
        console.error(err);
    }
    process.exit(1);
});
