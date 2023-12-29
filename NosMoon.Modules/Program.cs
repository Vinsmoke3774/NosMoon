using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NosMoon.Module.Bazaar;
using System.Threading.Tasks;

namespace NosMoon.Modules
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var web = BuildHost(args);
            var bazaarManager = (BazaarManager)web.Services.GetService(typeof(BazaarManager));
            await bazaarManager.Initialize();

            await web.RunAsync().ConfigureAwait(false);
        }

        public static IWebHost BuildHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, conf) =>
                {
                    conf.AddYamlFile("modules.yml", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
