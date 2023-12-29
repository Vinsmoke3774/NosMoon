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
using OpenNos.Handler.Login;
using OpenNos.Master.Library.Client;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenNos.Login
{
    public static class Program
    {
        #region Members

        private static bool _isDebug;

        private static int _port;

        private static EventHandler _exitHandler;

        #region Delegates

        public delegate bool EventHandler(CtrlType sig);

        #endregion

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
            checked
            {
                try
                {
#if DEBUG
                    _isDebug = true;
#endif
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                    Console.Title = $"OpenNos Login Server{(_isDebug ? " Development Environment" : "")}";

                    bool ignoreStartupMessages = false;
                    foreach (string arg in args)
                    {
                        ignoreStartupMessages |= arg == "--nomsg";
                    }

                    // initialize Logger
                    Logger.InitializeLogger(new SerilogLogger());

                    bool isLocal = bool.Parse(ConfigurationManager.AppSettings["IsLocal"]);
                    int port = Convert.ToInt32(ConfigurationManager.AppSettings[(isLocal ? "LocalPort" : "LoginPort")]);
                    int portArgIndex = Array.FindIndex(args, s => s == "--port");
                    if (portArgIndex != -1
                        && args.Length >= portArgIndex + 1
                        && int.TryParse(args[portArgIndex + 1], out port))
                    {
                        Console.WriteLine("Port override: " + port);
                    }
                    _port = port;
                    if (!ignoreStartupMessages)
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                        string text = $"LOGIN SERVER v{fileVersionInfo.ProductVersion}dev - PORT : {port} by OpenNos Team";
                        int offset = (Console.WindowWidth / 2) + (text.Length / 2);
                        string separator = new string('=', Console.WindowWidth);
                        Console.WriteLine(separator + string.Format("{0," + offset + "}\n", text) + separator);
                    }

                    // initialize api
                    if (CommunicationServiceClient.Instance.Authenticate(ConfigurationManager.AppSettings["MasterAuthKey"]))
                    {
                        Logger.Log.Info(Language.Instance.GetMessageFromKey("API_INITIALIZED"));
                    }

                    // initialize DB
                    if (!DataAccessHelper.Initialize())
                    {
                        Console.ReadKey();
                        return;
                    }

                    Logger.Log.Info(Language.Instance.GetMessageFromKey("CONFIG_LOADED"));

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

                    try
                    {
                        // initialize PacketSerialization
                        PacketFactory.Initialize<WalkPacket>();

                        NetworkManager<LoginCryptography> networkManager = new NetworkManager<LoginCryptography>(ConfigurationManager.AppSettings[(isLocal ? "LocalIp" : "IPAddress")], port, typeof(LoginPacketHandler), typeof(LoginCryptography), false);
                        AntiSpamModule.Instance.RunBlacklistTask();
                        Logger.Log.Info("Anti Spam Module initialized");
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
        }

        private static bool ExitHandler(CtrlType sig)
        {
            return false;
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Log.Error(null, (Exception)e.ExceptionObject);
            try
            {
            }
            catch (Exception ex)
            {
                Logger.Log.Error(null, ex);
            }

            Logger.Log.Debug("Login Server crashed! Rebooting gracefully...");
            Process.Start("OpenNos.Login.exe", $"--nomsg --port {_port}");
            Environment.Exit(1);
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