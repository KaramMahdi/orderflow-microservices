using OrderFlow.HttpApi.Host.HealthChecks;
using Shared.AspNetCore.Constants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks()
    .AddCheck<SqlServerHealthCheck>(SqlServerHealthCheck.Name);

var app = builder.Build();

app.MapHealthChecks(EndpointPaths.Health);

await app.RunAsync();
