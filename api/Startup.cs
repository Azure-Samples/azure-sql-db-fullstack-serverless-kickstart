using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Todo.Backend.EFCore;

[assembly: FunctionsStartup(typeof(Todo.Backend.Startup))]

namespace Todo.Backend
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {           
            string connectionString = Environment.GetEnvironmentVariable("AzureSQL"); 
            builder.Services.AddDbContext<TodoContext>(
                options => options.UseSqlServer(connectionString)
            );
        }
        
    }
}