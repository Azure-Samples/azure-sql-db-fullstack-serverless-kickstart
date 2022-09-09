using System;
using DbUp;
using DotNetEnv;
using Microsoft.Data.SqlClient;

namespace Todo.Database.Deploy
{
    class Program
    {
        static int Main(string[] args)
        {
            // This will load the content of .env and create related environment variables
            DotNetEnv.Env.Load();            

            // Connection string for deploying the database (high-privileged account as it needs to be able to CREATE/ALTER/DROP)
            var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            
            if (string.IsNullOrEmpty(connectionString)) {
                Console.WriteLine("ERROR: 'ConnectionString' enviroment variable not set or empty.");
                Console.WriteLine("You can create an .env file in this folder that sets the 'ConnectionString' environment variable; then run this app again.");                
                return 1;
            }
            
            var csb = new SqlConnectionStringBuilder(connectionString);
            Console.WriteLine($"Deploying database: {csb.InitialCatalog}");

            Console.WriteLine("Testing connection...");
            var conn = new SqlConnection(csb.ToString());
            conn.Open();
            conn.Close();

            Console.WriteLine("Starting deployment...");
            var dbup = DeployChanges.To
                .SqlDatabase(csb.ConnectionString)
                .WithScriptsFromFileSystem("./sql") 
                .JournalToSqlTable("dbo", "$__dbup_journal")                                               
                .LogToConsole()
                .Build();
         
            var result = dbup.PerformUpgrade();

            if (!result.Successful)
            {
                Console.WriteLine(result.Error);
                return -1;
            }

            Console.WriteLine("Success!");
            return 0;
        }
    }
}
