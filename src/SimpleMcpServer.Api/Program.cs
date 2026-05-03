using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using SimpleMcpServer.Api.Middleware;
using SimpleMcpServer.Application;
using SimpleMcpServer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseMiddleware<JsonRpcExceptionMiddleware>();
app.MapControllers();

app.Run();
