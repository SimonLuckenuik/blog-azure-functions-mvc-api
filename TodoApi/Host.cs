using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ExecutionContext = Microsoft.Azure.WebJobs.ExecutionContext;

namespace TodoApi
{
    public static class Host
    {
        private static readonly TestServer Server;
        private static readonly HttpClient ServerHttpClient;

        static Host()
        {
            var functionPath = Path.Combine(new FileInfo(typeof(Host).Assembly.Location).Directory.FullName, "..");
            Environment.SetEnvironmentVariable("HOST_FUNCTION_CONTENT_PATH", functionPath, EnvironmentVariableTarget.Process);
    
            Server = new TestServer(WebHost
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config
                        .SetBasePath(functionPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseContentRoot(functionPath));

            ServerHttpClient = Server.CreateClient();
        }

        [FunctionName("AllPaths")]
        public static async Task<HttpResponseMessage> RunAllPaths(
            CancellationToken ct,
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "patch", "options", Route = "{*x:regex(^(?!admin|debug|runtime).*$)}")]HttpRequestMessage req,
            ILogger log,
            ExecutionContext ctx)
        {
            return await ServerHttpClient.SendAsync(req, ct);
        }

        [FunctionName("Root")]
        public static async Task<HttpResponseMessage> Root(
            CancellationToken ct,
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "patch", "options", Route = "/")]HttpRequestMessage req,
            ILogger log,
            ExecutionContext ctx)
        {
            return await ServerHttpClient.SendAsync(req, ct);
        }
    }
}
