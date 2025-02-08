using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using PostgrestSharp.Abstract.Interfaces.Services;
using PostgrestSharp.Business.Services;
using PostgrestSharp.WebApi.Controllers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi


builder.Services.AddScoped<NpgsqlConnection>(  x =>
{
    var conn = new NpgsqlConnection(builder.Configuration.GetConnectionString("PortalDbContext"));
    conn.Open();
    return conn;
});


builder.Services.AddScoped<MemoryCacheEntryOptions>(_ => new MemoryCacheEntryOptions()
    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
    .SetAbsoluteExpiration(TimeSpan.FromHours(5)));

builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IJsonObjectHandlerService, JsonObjectHandlerService>();


builder.Services.AddLogging();

builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

var app = builder.Build();


app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();