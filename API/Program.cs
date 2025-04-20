using Application.Interfaces;
using Infrastructure.Repositories;
using Application;
using System.Reflection;
using MediatR;
using Application.Queries;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Redis config
builder.Services.AddStackExchangeRedisCache(options =>
{
    //options.Configuration = "redis:6379";
    options.Configuration = "localhost:6379";
    options.InstanceName = "RedisDemo_";
});


// PostgreSQL EF Core setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"))
);

// Dependency Injection
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
