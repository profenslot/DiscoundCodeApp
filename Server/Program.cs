using Common.Services;
using Common.Services.Interfaces;
using Common.Storage;
using Common.Storage.Repositories;
using Common.Storage.Repositories.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Server.BackgroundServices;
using Server.GrpcServices;
using Server.Services;
using Server.Services.Interfaces;
using StackExchange.Redis;
using System.Net;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connection = new SqliteConnection(builder.Configuration.GetConnectionString("SqliteConnection"));
    connection.DefaultTimeout = 5000;
    options.EnableSensitiveDataLogging(false);
    options.UseSqlite(connection);
});

var redisConnectionString = builder.Configuration.GetConnectionString("RedisConnection");
if (string.IsNullOrEmpty(redisConnectionString))
{
    throw new InvalidOperationException("Redis connection string is missing");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(await ConnectionMultiplexer.ConnectAsync(redisConnectionString));
builder.Services.AddSingleton(Channel.CreateBounded<QueueModel>(new BoundedChannelOptions(10000)
{
    FullMode = BoundedChannelFullMode.Wait
}));

builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
builder.Services.AddScoped<IDiscountCodeService, DiscountCodeService>();
builder.Services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();

builder.Services.AddHostedService<QueueReaderService>();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 1000;
    options.Limits.MaxConcurrentUpgradedConnections = 1000;
    options.Listen(IPAddress.Any, 7032, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapGrpcService<DiscountCodeHandlerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();
