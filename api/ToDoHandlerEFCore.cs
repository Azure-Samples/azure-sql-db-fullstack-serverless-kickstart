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

namespace Todo.Backend.EFCore
{
    [Table("todos")]
    public class Todo {

        [Column("id")]        
        public int Id { get; set; }

        [Column("todo", TypeName = "nvarchar(100)")]
        public string Title { get; set; }
        
        [Column("completed", TypeName = "tinyint")]
        public bool Completed { get; set; }
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

    public class ToDoHandler
    {        
        private TodoContext _todoContext;

        public ToDoHandler(TodoContext todoContext)
        {
            this._todoContext = todoContext;
        }
       
        [FunctionName("GetEF")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo-ef/{id:int?}")] HttpRequest req, 
            ILogger log, 
            int? id)
        {            
            IQueryable<Todo> todos = this._todoContext.Todos;

            if (id.HasValue) {
                todos = this._todoContext.Todos.Where(t => t.Id == id);                
            }         

            return new OkObjectResult(await todos.ToListAsync());
        }

        [FunctionName("PostEF")]
        public async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo-ef")] HttpRequest req, 
            ILogger log)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();            
            var todo = JsonConvert.DeserializeObject<Todo>(body);
            
            await this._todoContext.AddAsync(todo);
            await this._todoContext.SaveChangesAsync();                    
            
            return new OkObjectResult(todo);
        }

        [FunctionName("PatchEF")]
        public async Task<IActionResult> Patch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "todo-ef/{id}")] HttpRequest req, 
            ILogger log,
            int id)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();            
            var newTodo = JsonConvert.DeserializeObject<Todo>(body);
            
            var targetTodo = await this._todoContext.Todos.FindAsync(id);
            if (targetTodo == null)
                return new NotFoundResult();

            //targetTodo.Id = newTodo.Id;
            targetTodo.Title = newTodo.Title;
            targetTodo.Completed = newTodo.Completed;

            await this._todoContext.SaveChangesAsync();

            return new OkObjectResult(targetTodo);           
        }

        [FunctionName("DeleteEF")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo-ef/{id}")] HttpRequest req, 
            ILogger log,
            int id)
        {
            var todo = await this._todoContext.Todos.FindAsync(id);
            
            if (todo == null)
                return new NotFoundResult();

            this._todoContext.Todos.Remove(todo);
            await this._todoContext.SaveChangesAsync();

            return new OkObjectResult(todo);
        }
    }
}
