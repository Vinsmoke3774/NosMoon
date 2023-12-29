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

using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Master.Library.Interface;
using OpenNos.SCS.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.SCS.Communication.ScsServices.Service;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenNos.Master.Server
{
    internal static class Program
    {
        #region Members

        private static readonly ManualResetEvent _run = new ManualResetEvent(true);

        private static bool _isDebug;

        private static EventHandler _exitHandler;

        #endregion

        #region Delegates

        public delegate bool EventHandler(CtrlType sig);

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
            try
            {
#if DEBUG
                _isDebug = true;
#endif
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                Console.Title = $"OpenNos Master Server{(_isDebug ? " Development Environment" : "")}";

                bool ignoreStartupMessages = false;
                bool ignoreTelemetry = false;
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "--nomsg":
                            ignoreStartupMessages = true;
                            break;

                        case "--notelemetry":
                            ignoreTelemetry = true;
                            break;
                    }
                }

                // initialize Logger
                Logger.InitializeLogger(new SerilogLogger());

                int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
                if (!ignoreStartupMessages)
                {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string text = $"MASTER SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                    int offset = (Console.WindowWidth / 2) + (text.Length / 2);
                    string separator = new string('=', Console.WindowWidth);
                    Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
                }

                // initialize DB
                if (!DataAccessHelper.Initialize())
                {
                    Console.ReadLine();
                    return;
                }

                Logger.Log.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

                try
                {
                    _exitHandler += ExitHandler;
                    NativeMethods.SetConsoleCtrlHandler(_exitHandler, true);
                    AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("General Error", ex);
                }

                try
                {
                    // configure Services and Service Host
                    string ipAddress = ConfigurationManager.AppSettings["MasterIP"];
                    IScsServiceApplication _server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(ipAddress, port));

                    _server.AddService<ICommunicationService, CommunicationService>(new CommunicationService());
                    _server.AddService<IConfigurationService, ConfigurationService>(new ConfigurationService());
                    _server.AddService<IMailService, MailService>(new MailService());
                    _server.AddService<IMallService, MallService>(new MallService());
                    _server.AddService<IAuthentificationService, AuthentificationService>(new AuthentificationService());
                    _server.ClientConnected += OnClientConnected;
                    _server.ClientDisconnected += OnClientDisconnected;

                    _server.Start();
                    Logger.Log.Info(Language.Instance.GetMessageFromKey("STARTED"));

                    if (!ignoreTelemetry)
                    {
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("General Error Server", ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error("General Error", ex);
                Console.ReadKey();
            }
        }

        private static void OnClientConnected(object sender, ServiceClientEventArgs e) => Logger.Log.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + e.Client.ClientId);

        private static void OnClientDisconnected(object sender, ServiceClientEventArgs e) => Logger.Log.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + e.Client.ClientId);

        private static bool ExitHandler(CtrlType sig)
        {
            return false;
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            Logger.Log.Error(null, (Exception)e.ExceptionObject);
            try
            {
                File.AppendAllText("C:\\MASTER_CRASHLOG.txt", e.ExceptionObject.ToString() + "\n");
            }
            catch { }
        }

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