
using Microsoft.AspNetCore.HttpLogging;
using SimpleMcpServer.Api.Mcp;
using SimpleMcpServer.Api.Mcp.Tools;
using SimpleMcpServer.Api.Models;
using SimpleMcpServer.Api.Providers;
using SimpleMcpServer.Api.Services;

namespace SimpleMcpServer.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<IDataProvider<FundamentalData>, FundamentalProvider>();
            builder.Services.AddScoped<IDataTool, FundamentalLookupTool>();
            builder.Services.AddScoped<ToolRegistry>();

            builder.Services.AddHttpLogging(logging =>
            {
                logging.LoggingFields =
                    HttpLoggingFields.RequestPath |
                    HttpLoggingFields.RequestBody |
                    HttpLoggingFields.ResponseBody;
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseHttpLogging();

            app.UseAuthorization();

            app.UseWebSockets();
            app.MapControllers();

            app.Run();
        }
    }
}
