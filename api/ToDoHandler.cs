using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Security.Claims;
using System.Text;
using Polly;
using System.ComponentModel;

namespace api
{
    public static class ToDoHandler
    {
        // Error list created from: 
        // - https://github.com/dotnet/efcore/blob/main/src/EFCore.SqlServer/Storage/Internal/SqlServerTransientExceptionDetector.cs
        // - https://docs.microsoft.com/en-us/dotnet/api/microsoft.data.sqlclient.sqlconfigurableretryfactory?view=sqlclient-dotnet-standard-4.1
        // - https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages
        // Manually added also
        // 0, 18456
        private static List<int> _transientErrors = new List<int> {
            233, 997, 921, 669, 617, 601, 121, 64, 20, 0, 53, 258,
            1203, 1204, 1205, 1222, 1221,
            1807,
            3966, 3960, 3935,
            4060, 4221, 4891,
            8651, 8645,
            9515,
            14355,
            10929, 10928, 10060, 10054, 10053, 10936, 10929, 10928, 10922, 10051, 10065,
            11001,
            17197,
            18456,
            20041,
            41839, 41325, 41305, 41302, 41301, 40143, 40613, 40501, 40540, 40197, 49918, 49919, 49920
        };

        private static bool CheckIfTransientError(Exception ex)
        {
            if (ex is SqlException sqlException)
            {
                foreach (SqlError err in sqlException.Errors)
                {
                    if (_transientErrors.Contains(err.Number)) return true;
                }

                return false;
            }

            return ex is TimeoutException;
        }

        private class ClientPrincipal
        {
            public string IdentityProvider { get; set; }
            public string UserId { get; set; }
            public string UserDetails { get; set; }
            public IEnumerable<string> UserRoles { get; set; }
        }

        private static ClientPrincipal ParsePrincipal(this HttpRequest req)
        {
            var principal = new ClientPrincipal();

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonConvert.DeserializeObject<ClientPrincipal>(json);
            }

            principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

            return principal;
        }

        private static async Task<JToken> ExecuteProcedure(string verb, JToken payload, JToken context, ILogger log)
        {            
            var polly = Policy
                .Handle<SqlException>(ex => CheckIfTransientError(ex))
                .Or<TimeoutException>()
                .OrInner<Win32Exception>(ex => CheckIfTransientError(ex))
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var details = string.Empty;
                        if (exception is SqlException) {
                            var sx = exception as SqlException;
                            details = $" [SQL Error: {sx.Number}]";
                        }
                        log.LogInformation($"WaitAndRetry called {retryCount} times, next retry in: {timeSpan}). Reason: {exception.GetType().Name}{details}: {exception.Message}");
                    });

            var runProcedure = async () => {
                using (var conn = new SqlConnection(Environment.GetEnvironmentVariable("AzureSQL")))
                {
                    DynamicParameters parameters = new DynamicParameters();
                    
                    if (payload != null) 
                        parameters.Add("payload", payload.ToString());
                    
                    if (context != null) 
                        parameters.Add("context", context.ToString()); 
                    else 
                        parameters.Add("context", "{}");

                    string stringResult = await conn.ExecuteScalarAsync<string>(
                        sql: $"web.{verb}_todo",
                        param: parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    if (!string.IsNullOrEmpty(stringResult)) 
                        return JToken.Parse(stringResult); 
                    else 
                        return null;
                }
            };

            return await polly.ExecuteAsync(async () => await runProcedure());
        }

        [FunctionName("Get")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id:int?}")] HttpRequest req,
            ILogger log,
            int? id)        
        {
            var context = JObject.FromObject(req.ParsePrincipal());

            var payload = id.HasValue ? new JObject { ["id"] = id.Value } : null;

            var result = await ExecuteProcedure("get", payload, context, log);

            if (result == null)
                return new NotFoundResult();

            return new OkObjectResult(result);
        }

        [FunctionName("Post")]
        public static async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            ILogger log)
        {
            var context = JObject.FromObject(req.ParsePrincipal());

            string body = await new StreamReader(req.Body).ReadToEndAsync();

            var payload = JObject.Parse(body);

            var result = await ExecuteProcedure("post", payload, context, log);

            return new OkObjectResult(result);
        }

        [FunctionName("Patch")]
        public static async Task<IActionResult> Patch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "todo/{id}")] HttpRequest req,
            ILogger log,
            int id)
        {
            var context = JObject.FromObject(req.ParsePrincipal());

            string body = await new StreamReader(req.Body).ReadToEndAsync();

            var payload = new JObject
            {
                ["id"] = id,
                ["todo"] = JObject.Parse(body)
            };

            JToken result = await ExecuteProcedure("patch", payload, context, log);

            if (result == null)
                return new NotFoundResult();

            return new OkObjectResult(result);
        }

        [FunctionName("Delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest req,
            ILogger log,
            int id)
        {
            var context = JObject.FromObject(req.ParsePrincipal());
            
            var payload = new JObject { ["id"] = id };

            var result = await ExecuteProcedure("delete", payload, context, log);

            if (result == null)
                return new NotFoundResult();

            return new OkObjectResult(result);
        }
    }
}
