using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NosByte.Shared;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NosByte.Web.Bazaar
{
    public class Program
    {

        private static EventHandler _exitHandler;

        #region Delegates

        public delegate bool EventHandler(CtrlType sig);

        #endregion
        public static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Console.Title = "NosByte Cluster Server";

            log4net.Config.XmlConfigurator.Configure();
            Logger.InitializeLogger(new SerilogLogger());

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            string text = $"CLUSTER SERVER v{fileVersionInfo.ProductVersion}dev - by Elendan ";

            int offset = (Console.WindowWidth / 2) + (text.Length / 2);
            string separator = new string('=', Console.WindowWidth);
            Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);

            // initialize DB
            if (!DataAccessHelper.Initialize())
            {
                Console.ReadLine();
                return;
            }

            Logger.Log.Info("Config loaded.");

            try
            {
                _exitHandler += ExitHandler;
                NativeMethods.SetConsoleCtrlHandler(_exitHandler, true);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }

            IWebHost web = CreateWebHostBuilder(args).Build();

            ServerManager.Instance.LoadItems();

            Logger.Log.Info($"The cluster server has started !");

            await web.StartAsync();

            await web.WaitForShutdownAsync();
        }

        private static bool ExitHandler(CtrlType sig)
        {
            return false;
        }

        public static class NativeMethods
        {
            #region Methods

            [DllImport("Kernel32")]
            internal static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

            #endregion
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            IWebHostBuilder webhost = WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel(serverOptions =>
                {
                    serverOptions.Listen(new IPEndPoint(IPAddress.Parse(Environment.GetEnvironmentVariable("CLUSTER_SERVER_ADDRESS") ?? "62.141.38.247"), short.Parse(Environment.GetEnvironmentVariable("CLUSTER_SERVER_PORT") ?? "8282")));
                })
                .UseConfiguration(config)
                .UseStartup<Startup>();

            return webhost;
        }
    }
}
