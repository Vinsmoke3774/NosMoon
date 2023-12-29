/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF.Helpers;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Handler;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNos.World
{
    public static class Program
    {
        #region Members

        private static readonly ManualResetEvent _run = new ManualResetEvent(true);


        private static bool _ignoreTelemetry;
        private static bool _isDebug;
        private static int _port;

        #endregion

        private static EventHandler _exitHandler;

        #region Delegates

        public delegate bool EventHandler(CtrlType sig);

        #endregion

        #region Methods

        public static async Task Main(string[] args)
        {
#if DEBUG
            _isDebug = true;
#endif
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            Console.Title = $"OpenNos World Server{(_isDebug ? " Development Environment" : "")}";

            bool ignoreStartupMessages = false;
            _port = Convert.ToInt32(ConfigurationManager.AppSettings["WorldPort"]);
            int portArgIndex = Array.FindIndex(args, s => s == "--port");
            if (portArgIndex != -1
                && args.Length >= portArgIndex + 1
                && int.TryParse(args[portArgIndex + 1], out _port))
            {
                Console.WriteLine("Port override: " + _port);
            }
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "--nomsg":
                        ignoreStartupMessages = true;
                        break;

                    case "--notelemetry":
                        _ignoreTelemetry = true;
                        break;
                }
            }

            // initialize Logger
            Logger.InitializeLogger(new SerilogLogger());

            if (!ignoreStartupMessages)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string text = $"WORLD SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {_port} by OpenNos Team ";

                int offset = (Console.WindowWidth / 2) + (text.Length / 2);
                string separator = new('=', Console.WindowWidth);
                Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
            }

            // initialize api
            bool authenticated = false;
            string authKey = ConfigurationManager.AppSettings["MasterAuthKey"];

            do
            {
                if (CommunicationServiceClient.Instance.Authenticate(authKey))
                {
                    authenticated = true;
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("API_INITIALIZED"));
                }
            } while (authenticated == false);
            

            // initialize DB
            if (DataAccessHelper.Initialize())
            {
                // initialilize maps
                ServerManager.Instance.Initialize();
            }
            else
            {
                Console.ReadKey();
                return;
            }

            // TODO: initialize ClientLinkManager initialize PacketSerialization
            PacketFactory.Initialize<WalkPacket>();

            try
            {
                _exitHandler += ExitHandler;
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                NativeMethods.SetConsoleCtrlHandler(_exitHandler, true);
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
            }
            NetworkManager<WorldCryptography> networkManager = null;

            bool isLocal = bool.Parse(ConfigurationManager.AppSettings["IsLocal"]);

            string ipAddress = ConfigurationManager.AppSettings[(isLocal ? "LocalIp" : "IPAddress")];
            string publicIp = ConfigurationManager.AppSettings[(isLocal ? "LocalIp" : "PublicIP")];

        portloop:
            try
            {
                networkManager = new NetworkManager<WorldCryptography>(ipAddress, _port, typeof(CommandPacketHandler), typeof(LoginCryptography), true);
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode == 10048)
                {
                    _port++;
                    Logger.Log.Info("Port already in use! Incrementing...");
                    goto portloop;
                }
                Logger.Log.Error("General Error", ex);
                Environment.Exit(ex.ErrorCode);
            }

            ServerManager.Instance.ServerGroup = ConfigurationManager.AppSettings["ServerGroup"];
            const int sessionLimit = 150; // Needs workaround
            int? newChannelId = CommunicationServiceClient.Instance.RegisterWorldServer(new SerializableWorldServer(ServerManager.Instance.WorldId, publicIp, _port, sessionLimit, ServerManager.Instance.ServerGroup));
            if (newChannelId.HasValue)
            {
                ServerManager.Instance.ChannelId = newChannelId.Value;
                Console.Title += $" Channel: {ServerManager.Instance.ChannelId} | Pid: {Process.GetCurrentProcess().Id}";
                MailServiceClient.Instance.Authenticate(authKey, ServerManager.Instance.WorldId);
                ConfigurationServiceClient.Instance.Authenticate(authKey, ServerManager.Instance.WorldId);
                ServerManager.Instance.Configuration = ConfigurationServiceClient.Instance.GetConfigurationObject();
                ServerManager.Instance.MallApi = new GameObject.Helpers.MallAPIHelper(ServerManager.Instance.Configuration.MallBaseURL);
                ServerManager.Instance.SynchronizeSheduling();
                AntiSpamModule.Instance.RunBlacklistTask();
                Logger.Log.Info("Anti Spam Module initialized");
            }
            else
            {
                Logger.Log.Error("Could not retrieve ChannelId from Web API.", null);
                Console.ReadKey();
            }
        }

        private static bool ExitHandler(CtrlType sig)
        {
            ServerManager.Instance.InShutdown = true;
            CommunicationServiceClient.Instance.UnregisterWorldServer(ServerManager.Instance.WorldId);
            ServerManager.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            ServerManager.Instance.SaveAll();
            ServerManager.Instance.DisconnectAll();
            Thread.Sleep(5000);
            return false;
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            ServerManager.Instance.InShutdown = true;
            if (e == null)
            {
                return;
            }

            Logger.Log.Error(null, (Exception)e.ExceptionObject);
            try
            {
                File.AppendAllText("C:\\WORLD_CRASHLOG.txt", e.ExceptionObject.ToString() + "\n");
            }
            catch { }

            Logger.Log.Debug("Server crashed! Rebooting gracefully...");
            CommunicationServiceClient.Instance.UnregisterWorldServer(ServerManager.Instance.WorldId);
            ServerManager.Shout(string.Format(Language.Instance.GetMessageFromKey("SHUTDOWN_SEC"), 5));
            ServerManager.Instance.SaveAll();
            ServerManager.Instance.DisconnectAll();
            Process.Start("OpenNos.World.exe", $"--nomsg --port {_port}");
            Environment.Exit(1);
        }

        #endregion

        #region Classes

        public static class NativeMethods
        {
            #region Methods

            [DllImport("Kernel32")]
            internal static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

            #endregion
        }

        #endregion
    }
}