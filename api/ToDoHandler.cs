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

namespace api
{
    public static class ToDoHandler
    {
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

        private static async Task<JToken> ExecuteProcedure(string verb, JToken payload, JToken context)
        {
            JToken result = null;            

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

                if (!string.IsNullOrEmpty(stringResult)) result = JToken.Parse(stringResult);
            }

            return result;
        }

        [FunctionName("Get")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo/{id:int?}")] HttpRequest req,
            ILogger log,
            int? id)        
        {
            var context = JObject.FromObject(req.ParsePrincipal());

            var payload = id.HasValue ? new JObject { ["id"] = id.Value } : null;

            var result = await ExecuteProcedure("get", payload, context);

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

            var result = await ExecuteProcedure("post", payload, context);

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

            JToken result = await ExecuteProcedure("patch", payload, context);

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

            var result = await ExecuteProcedure("delete", payload, context);

            if (result == null)
                return new NotFoundResult();

            return new OkObjectResult(result);
        }
    }
}
