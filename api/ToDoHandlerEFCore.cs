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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text;

namespace Todo.Backend.EFCore
{
    [Table("todos")]
    public class Todo {

        [JsonProperty("id")]
        [Column("id")]                
        public int Id { get; set; }

        [JsonProperty("title")]
        [Column("todo", TypeName = "nvarchar(100)")]
        public string Title { get; set; }
        
        [JsonProperty("completed")]
        [Column("completed", TypeName = "tinyint")]
        public bool Completed { get; set; }
        
        [Column("owner_id", TypeName = "nvarchar(128)")]        
        public string Owner { get; set; }

        public bool ShouldSerializeOwner() => false;

    }

    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<int>("global_sequence");

            modelBuilder.Entity<Todo>()
                .Property(o => o.Id)
                .HasDefaultValueSql("NEXT VALUE FOR global_sequence");                        

            modelBuilder.Entity<Todo>()
                .Property(o => o.Owner)
                .HasDefaultValue("anonymous");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
        
        public DbSet<Todo> Todos { get; set; }        
    }

    public class TodoContextFactory: IDesignTimeDbContextFactory<TodoContext>
    {
        public TodoContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TodoContext>();
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("AzureSQL"));

            return new TodoContext(optionsBuilder.Options);
        }
    }

    public class ClientPrincipal
    {
        public string IdentityProvider { get; set; }
        public string UserId { get; set; }
        public string UserDetails { get; set; }
        public IEnumerable<string> UserRoles { get; set; }
    }

    public static class Utils
    {
        public static ClientPrincipal ParsePrincipal(this HttpRequest req)
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
            principal.UserId = principal.UserId ?? "anonymous";

            return principal;
        }
    }

    public class ToDoHandler
    {                
        private TodoContext _todoContext;

        public ToDoHandler(TodoContext todoContext)
        {
            this._todoContext = todoContext;
        }               
       
        [FunctionName("GetEF")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ef/todo/{id:int?}")] HttpRequest req, 
            ILogger log, 
            int? id)
        {           
            var cp = req.ParsePrincipal();

            IQueryable<Todo> todos = this._todoContext.Todos.Where(t => t.Owner == cp.UserId);

            if (id.HasValue) {
                todos = this._todoContext.Todos.Where(t => t.Id == id);                
            }         

            return new OkObjectResult(await todos.ToListAsync());
        }

        [FunctionName("PostEF")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ef/todo")] HttpRequest req, 
            ILogger log)
        {
            var cp = req.ParsePrincipal();

            string body = await new StreamReader(req.Body).ReadToEndAsync();            
            var todo = JsonConvert.DeserializeObject<Todo>(body);
            todo.Owner = cp.UserId;
            
            await this._todoContext.AddAsync(todo);
            await this._todoContext.SaveChangesAsync();                    
            
            var result = new List<Todo>() { todo };
            return new OkObjectResult(result);
        }

        [FunctionName("PatchEF")]
        public async Task<IActionResult> Patch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "ef/todo/{id}")] HttpRequest req,             
            ILogger log,            
            int id)
        {
            var cp = req.ParsePrincipal();

            string body = await new StreamReader(req.Body).ReadToEndAsync();            
            var newTodo = JsonConvert.DeserializeObject<Todo>(body);            
                        
            var targetTodo = this._todoContext.Todos.Where(t => t.Owner == cp.UserId).FirstOrDefault(t => t.Id == id);
            if (targetTodo == null)
                return new NotFoundResult();
            
            targetTodo.Title = newTodo.Title ?? targetTodo.Title;
            targetTodo.Completed = newTodo.Completed;

            await this._todoContext.SaveChangesAsync();

            return new OkObjectResult(targetTodo);           
        }

        [FunctionName("DeleteEF")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "ef/todo/{id}")] HttpRequest req, 
            ILogger log,
            int id)
        {
            var cp = req.ParsePrincipal();
            
            var todo = this._todoContext.Todos.Where(t => t.Owner == cp.UserId).FirstOrDefault(t => t.Id == id);
            
            if (todo == null)
                return new NotFoundResult();

            this._todoContext.Todos.Remove(todo);
            await this._todoContext.SaveChangesAsync();

            return new OkObjectResult(todo);
        }
    }
}
