using Microsoft.EntityFrameworkCore;
using Caridology_Department_System.Models;
using System;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddDbContext<DBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at root
    });
}

// Add these critical middleware components
app.UseHttpsRedirection();
app.UseRouting();  // This is crucial for endpoint routing
app.UseAuthorization();

// This maps your controller endpoints
app.MapControllers();

// Add a default route for testing
app.MapGet("/", () => "API is running!");

app.Run();
