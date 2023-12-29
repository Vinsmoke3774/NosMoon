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
using OpenNos.Core.Actions;
using OpenNos.Core.Extensions;
using OpenNos.Core.Handling;
using OpenNos.Core.Logger;
using OpenNos.Core.Networking.Communication.Scs.Communication.Messages;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Guri;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc.NRunHandles.Misc;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace OpenNos.GameObject
{
    public class ClientSession
    {
        #region Members

        private string _lastPacket;

        private static CryptographyBase _encryptor;

        private readonly INetworkClient _client;

        private readonly Random _random;

        private readonly ConcurrentQueue<byte[]> _receiveQueue;

        private readonly IList<string> _waitForPacketList = new List<string>();

        private Character _character;

        private IDictionary<string[], HandlerMethodReference> _handlerMethods;

        private int _lastPacketId;

        private bool _isWorldServer;

        // private byte countPacketReceived;

        private long _lastPacketReceive;

        private int? _waitForPacketsAmount;

        #endregion

        #region Instantiation

        public ClientSession(INetworkClient client)
        {
            _lastPacket = "";

            // set the time of last received packet
            _lastPacketReceive = DateTime.Now.Ticks;

            // lag mode
            _random = new Random((int)client.ClientId);

            // initialize network client
            _client = client;

            // absolutely new instantiated Client has no SessionId
            SessionId = 0;

            // register for NetworkClient events
            _client.MessageReceived += OnNetworkClientMessageReceived;

            // start observer for receiving packets
            _receiveQueue = new ConcurrentQueue<byte[]>();
            
            PacketHandlerInterval = Observable.Interval(TimeSpan.FromMilliseconds(10)).SafeSubscribe(x => HandlePackets());
        }

        #endregion

        #region Properties

        public IDisposable PacketHandlerInterval { get; set; }

        public Account Account { get; internal set; }

        public Character Character
        {
            get
            {
                if (_character == null || !HasSelectedCharacter)
                {
                    StackFrame method;

                    try
                    {
                        method = new StackTrace(true).GetFrame(1);
                    }
                    catch (Exception e)
                    {
                        method = null;
                        Logger.Log.Error(null, e);
                    }

                    Logger.Log.Error($"An uninitialized character cannot be accessed. (called from: {(method?.GetMethod().Name ?? string.Empty)} | File: {method?.GetFileName() ?? string.Empty} | Line: {method?.GetFileLineNumber()})", null);
                }

                return _character;
            }
            internal set => _character = value;
        }

        public string CleanIpAddress
        {
            get
            {
                string cleanIp = _client.IpAddress.Replace("tcp://", "");
                return cleanIp.Substring(0, cleanIp.LastIndexOf(":") > 0 ? cleanIp.LastIndexOf(":") : cleanIp.Length);
            }
            set { }
        }

        public long ClientId => _client.ClientId;

        public MapInstance CurrentMapInstance { get; set; }

        public IDictionary<string[], HandlerMethodReference> HandlerMethods
        {
            get => _handlerMethods ??= new Dictionary<string[], HandlerMethodReference>();
            private set => _handlerMethods = value;
        }

        public bool HasCurrentMapInstance => CurrentMapInstance != null;

        public bool HasSelectedCharacter { get; private set; }

        public bool HasSession => _client != null;

        public string IpAddress => _client.IpAddress;

        public bool IsAuthenticated { get; private set; }

        public bool IsConnected => _client.IsConnected;

        public bool IsDisposing
        {
            get => _client.IsDisposing;
            internal set => _client.IsDisposing = value;
        }

        public int SessionId { get; private set; }

        private IDictionary<NRunType[], Action<NRunPacket>> _nRunHandlerMethods;

        public IDictionary<NRunType[], Action<NRunPacket>> NrunHandlerMethods
        {
            get => _nRunHandlerMethods ??= new ConcurrentDictionary<NRunType[], Action<NRunPacket>>();
            set => _nRunHandlerMethods = value;
        }

        private IDictionary<GuriType[], Action<GuriPacket>> _guriHandlerMethods;

        public IDictionary<GuriType[], Action<GuriPacket>> GuriHandlerMethods
        {
            get => _guriHandlerMethods ??= new ConcurrentDictionary<GuriType[], Action<GuriPacket>>();
            set => _guriHandlerMethods = value;
        }

        #endregion

        #region Methods

        private void RegisterNRunHandlers(bool isWorldServer)
        {
            if (!isWorldServer)
            {
                return;
            }

            var type = typeof(ChangeClassHandler);
            foreach (var handler in type.Assembly.GetTypes().Where(s => !string.IsNullOrEmpty(s.Namespace) && s.Namespace.Contains("OpenNos.GameObject.Npc.NRunHandles"))) // Dirty but should work for now
            {
                if (!(handler.GetCustomAttribute(typeof(NRunHandlerAttribute)) is NRunHandlerAttribute attribute))
                {
                    continue;
                }

                var instance = (IGenericHandler<NRunPacket>)Activator.CreateInstance(handler, this);
                NrunHandlerMethods.Add(attribute.Runners, instance.ValidateData);
            }
        }

        private void RegisterGuriHandlers(bool isWorldServer)
        {
            if (!isWorldServer)
            {
                return;
            }

            var type = typeof(EmoticonHandler);

            foreach (var handler in type.Assembly.GetTypes().Where(s => !string.IsNullOrEmpty(s.Namespace) && s.Namespace.Equals(type.Namespace)))
            {
                if (handler.GetCustomAttribute(typeof(GuriHandlerAttribute)) is not GuriHandlerAttribute attribute)
                {
                    continue;
                }

                var instance = (IGenericHandler<GuriPacket>)Activator.CreateInstance(handler, this);
                GuriHandlerMethods.Add(attribute.GuriTypes, instance.ValidateData);
            }
        }


        public void ClearLowPriorityQueue() => _client.ClearLowPriorityQueueAsync();

        public void Destroy()
        {
            // unregister from WCF events
            CommunicationServiceClient.Instance.CharacterConnectedEvent -= OnOtherCharacterConnected;
            CommunicationServiceClient.Instance.CharacterDisconnectedEvent -= OnOtherCharacterDisconnected;
            

            // do everything necessary before removing client, DB save, Whatever
            if (HasSelectedCharacter)
            {
                Logger.Log.LogUserEvent("CHARACTER_LOGOUT", GenerateIdentity(), "");

                long characterId = Character.CharacterId;

                Character.Dispose();

                // disconnect client
                CommunicationServiceClient.Instance?.DisconnectCharacter(ServerManager.Instance.WorldId, characterId);

                // unregister from map if registered
                if (CurrentMapInstance != null)
                {
                    CurrentMapInstance.UnregisterSession(characterId);
                    CurrentMapInstance = null;
                }

                ServerManager.Instance.UnregisterSession(characterId);
            }

            if (Account != null)
            {
                CommunicationServiceClient.Instance.DisconnectAccount(Account.AccountId);
            }

            ClearReceiveQueue();
            DestroySessionObjects();
        }

        internal void DestroySessionObjects()
        {
            HandlerMethods.Clear();
            HandlerMethods = null;
            NrunHandlerMethods.Clear();
            NrunHandlerMethods = null;
            GuriHandlerMethods.Clear();
            GuriHandlerMethods = null;
            PacketHandlerInterval?.Dispose();
            _client.MessageReceived -= OnNetworkClientMessageReceived;
        }

        public void Disconnect()
        {
            Character?.SaveObs?.Dispose();
            _client.Disconnect();
            try
            {
                Logger.Log.Info("Calling Disconnect method.");
                DestroySessionObjects();
            }
            catch
            {
                Logger.Log.Error("An error occurred during disconnection. Objects could not be deleted properly.", null);
            }
            
        }

        public string GenerateIdentity()
        {
            if (Character != null)
            {
                return $"Character: {Character.Name}";
            }
            if (Account != null)
            {
                return $"Account: {Account.Name}";
            }
            return "Account doesn't exist";
        }

        public void Initialize(CryptographyBase encryptor, Type packetHandler, bool isWorldServer)
        {
            _encryptor = encryptor;
            _client.Initialize(encryptor);
            _isWorldServer = isWorldServer;


            // dynamically create packethandler references
            GenerateHandlerReferences(packetHandler, isWorldServer);
            RegisterNRunHandlers(isWorldServer);
            RegisterGuriHandlers(isWorldServer);
        }

        public void InitializeAccount(Account account, bool crossServer = false)
        {
            Account = account;
            if (crossServer)
            {
                CommunicationServiceClient.Instance.ConnectAccountCrossServer(ServerManager.Instance.WorldId, account.AccountId, SessionId);
            }
            else
            {
                CommunicationServiceClient.Instance.ConnectAccount(ServerManager.Instance.WorldId, account.AccountId, SessionId);
            }
            IsAuthenticated = true;
        }

        public void ReceivePacket(string packet, bool ignoreAuthority = false)
        {
            string header = packet.Split(' ')[0];
            TriggerHandler(header, $"{_lastPacketId} {packet}", false, ignoreAuthority);
            _lastPacketId++;
        }

        private void HardcodedShitButNowIFeelBetterAboutMyself(string packet, byte priority)
        {
            if (packet != null && packet.StartsWith("raidbf"))
            {
                var newPacket = packet;
                newPacket = newPacket.Replace("raidbf", "raidbf1");
                _client.SendPacket(newPacket, priority);
            }

            if (packet != null && packet.StartsWith("rdlst"))
            {
                if (packet.StartsWith("rdlstf"))
                {
                    var split = packet.Split(' ').ToList();
                    if (split.Count > 3)
                    {
                        split.RemoveAt(0);
                        split.RemoveAt(2);
                        var newPacket = split.Aggregate("rdlst1 ", (current, data) => current + $"{data} ");
                        _client.SendPacket(newPacket, priority);
                    }
                }
                else
                {
                    var newpacket = packet;
                    newpacket = newpacket.Replace("rdlst", "rdlst1");
                    _client.SendPacket(newpacket, priority);
                }
            }
        }

        public void SendPacket(string packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(packet, priority);

                HardcodedShitButNowIFeelBetterAboutMyself(packet, priority);

                if (packet != null && _character != null && HasSelectedCharacter && !packet.StartsWith("cond ") && !packet.StartsWith("mv ")) SendPacket(Character.GenerateCond());
            }
        }

        public void SendPacket(PacketDefinition packet, byte priority = 10)
        {
            if (!IsDisposing)
            {
                _client.SendPacket(PacketFactory.Serialize(packet), priority);
            }
        }

        public void SendPacketAfter(string packet, int milliseconds)
        {
            if (!IsDisposing)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(milliseconds)).SafeSubscribe(o => SendPacket(packet));
            }
        }

        public void SendPacketFormat(string packet, params object[] param)
        {
            if (!IsDisposing)
            {
                _client.SendPacketFormat(packet, param);
            }
        }

        public void SendPackets(IEnumerable<string> packets, byte priority = 10, bool cond = true)
        {
            if (!IsDisposing)
            {
                _client.SendPackets(packets, priority);

                if (_character != null && HasSelectedCharacter && cond)
                {
                    SendPacket(Character.GenerateCond());
                }
            }
        }

        public void SendPackets(IEnumerable<PacketDefinition> packets, byte priority = 10)
        {
            if (!IsDisposing)
            {
                packets.ToList().ForEach(s => _client.SendPacket(PacketFactory.Serialize(s), priority));
            }
        }

        public void SetCharacter(Character character)
        {
            Character = character;
            HasSelectedCharacter = true;

            Logger.Log.LogUserEvent("CHARACTER_LOGIN", GenerateIdentity(), "");

            // register CSC events
            CommunicationServiceClient.Instance.CharacterConnectedEvent += OnOtherCharacterConnected;
            CommunicationServiceClient.Instance.CharacterDisconnectedEvent += OnOtherCharacterDisconnected;

            // register for servermanager
            ServerManager.Instance.RegisterSession(this);
            ServerManager.Instance.CharacterScreenSessions.Remove(character.AccountId);
            Character.SetSession(this);
        }

        private void ClearReceiveQueue()
        {
            _receiveQueue.Clear();
        }

        private void GenerateHandlerReferences(Type type, bool isWorldServer)
        {
            IEnumerable<Type> handlerTypes = !isWorldServer ? type.Assembly.GetTypes().Where(t => t.Name.Equals("LoginPacketHandler")) // shitty but it works
                                                            : type.Assembly.GetTypes().Where(p =>
                                                            {
                                                                Type interfaceType = type.GetInterfaces().FirstOrDefault();
                                                                return interfaceType != null && !p.IsInterface && interfaceType.IsAssignableFrom(p);
                                                            });

            // iterate thru each type in the given assembly
            foreach (Type handlerType in handlerTypes)
            {
                IPacketHandler handler = (IPacketHandler)Activator.CreateInstance(handlerType, this);

                // include PacketDefinition
                foreach (MethodInfo methodInfo in handlerType.GetMethods().Where(x => x.GetParameters().FirstOrDefault()?.ParameterType.BaseType == typeof(PacketDefinition)))
                {
                    HandlerMethodReference methodReference = new(DelegateBuilder.BuildDelegate<Action<object, object>>(methodInfo), handler, methodInfo.GetParameters().FirstOrDefault()?.ParameterType);
                    HandlerMethods.Add(methodReference.Identification, methodReference);
                }
            }
        }

        /// <summary>
        /// Handle the packet received by the Client.
        /// </summary>
        private void HandlePackets()
        {
            if (ServerManager.Instance.InShutdown == true)
            {
                return;
            }

            while (_receiveQueue.TryDequeue(out byte[] packetData))
            {
                // determine first packet
                if (_encryptor.HasCustomParameter && SessionId == 0)
                {
                    string sessionPacket = _encryptor.DecryptCustomParameter(packetData);

                    string[] sessionParts = sessionPacket.Split(' ');

                    if (sessionParts.Length == 0)
                    {
                        return;
                    }

                    if (!int.TryParse(sessionParts[0], out int packetId))
                    {
                        Disconnect();
                    }

                    _lastPacketId = packetId;

                    // set the SessionId if Session Packet arrives
                    if (sessionParts.Length < 2)
                    {
                        return;
                    }

                    if (int.TryParse(sessionParts[1].Split('\\').FirstOrDefault(), out int sessid))
                    {
                        SessionId = sessid;
                        Logger.Log.Debug(string.Format(Language.Instance.GetMessageFromKey("CLIENT_ARRIVED"), SessionId));

                        if (!_waitForPacketsAmount.HasValue)
                        {
                            TriggerHandler("OpenNos.EntryPoint", "", false);
                        }
                    }

                    return;
                }

                string packetConcatenated = _encryptor.Decrypt(packetData, SessionId);

                foreach (string packet in packetConcatenated.Split(new[] { (char)0xFF }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string packetstring = packet.Replace('^', ' ');
                    string[] packetsplit = packetstring.Split(' ');

                    if (_encryptor.HasCustomParameter)
                    {
                        string nextRawPacketId = packetsplit[0];

                        if (!int.TryParse(nextRawPacketId, out int nextPacketId) && nextPacketId != _lastPacketId + 1)
                        {
                            Logger.Log.Error(string.Format(Language.Instance.GetMessageFromKey("CORRUPTED_KEEPALIVE"), _client.ClientId), null);
                            _client.Disconnect();
                            return;
                        }

                        if (nextPacketId == 0)
                        {
                            if (_lastPacketId == ushort.MaxValue)
                            {
                                _lastPacketId = nextPacketId;
                            }
                        }
                        else
                        {
                            _lastPacketId = nextPacketId;
                        }

                        if (_waitForPacketsAmount.HasValue)
                        {
                            _waitForPacketList.Add(packetstring);

                            string[] packetssplit = packetstring.Split(' ');

                            if (packetssplit.Length > 3 && packetsplit[1] == "DAC")
                            {
                                _waitForPacketList.Add("0 CrossServerAuthenticate");
                            }

                            if (_waitForPacketList.Count == _waitForPacketsAmount)
                            {
                                _waitForPacketsAmount = null;
                                string queuedPackets = string.Join(" ", _waitForPacketList.ToArray());
                                string header = queuedPackets.Split(' ', '^')[1];
                                TriggerHandler(header, queuedPackets, true);
                                _waitForPacketList.Clear();
                                return;
                            }
                        }
                        else if (packetsplit.Length > 1)
                        {
                            if (packetsplit[1].Length >= 1 && (packetsplit[1][0] == '/' || packetsplit[1][0] == ':' || packetsplit[1][0] == ';'))
                            {
                                packetsplit[1] = packetsplit[1][0].ToString();
                                packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                            }

                            if (packetsplit[1] != "0")
                            {
                                TriggerHandler(packetsplit[1].Replace("#", ""), packetstring, false);
                            }
                        }
                    }
                    else
                    {
                        string packetHeader = packetstring.Split(' ')[0];

                        // simple messaging
                        if (packetHeader[0] == '/' || packetHeader[0] == ':' || packetHeader[0] == ';')
                        {
                            packetHeader = packetHeader[0].ToString();
                            packetstring = packet.Insert(packet.IndexOf(' ') + 2, " ");
                        }

                        TriggerHandler(packetHeader.Replace("#", ""), packetstring, false);
                    }
                }
            }
        }

        /// <summary>
        /// This will be triggered when the underlying NetworkClient receives a packet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNetworkClientMessageReceived(object sender, MessageEventArgs e)
        {
            ScsRawDataMessage message = e.Message as ScsRawDataMessage;
            if (message == null)
            {
                return;
            }
            if (message.MessageData.Length > 0 && message.MessageData.Length > 2)
            {
                _receiveQueue.Enqueue(message.MessageData);
            }
            _lastPacketReceive = e.ReceivedTimestamp.Ticks;
        }

        private void OnOtherCharacterConnected(object sender, EventArgs e)
        {
            if (Character?.IsDisposed != false)
            {
                return;
            }

            Tuple<long, string> loggedInCharacter = (Tuple<long, string>)sender;

            if (Character.IsFriendOfCharacter(loggedInCharacter.Item1) && Character != null && Character.CharacterId != loggedInCharacter.Item1)
            {
                _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_IN"), loggedInCharacter.Item2), 10));
                _client.SendPacket(Character.GenerateFinfo(loggedInCharacter.Item1, true));
            }

            FamilyCharacter chara = Character.Family?.FamilyCharacters.Find(s => s.CharacterId == loggedInCharacter.Item1);

            if (chara != null && loggedInCharacter.Item1 != Character?.CharacterId)
            {
                _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_FAMILY_LOGGED_IN"), loggedInCharacter.Item2, Language.Instance.GetMessageFromKey(chara.Authority.ToString().ToUpper())), 10));
            }
        }

        private void OnOtherCharacterDisconnected(object sender, EventArgs e)
        {
            if (Character?.IsDisposed != false)
            {
                return;
            }

            Tuple<long, string> loggedOutCharacter = (Tuple<long, string>)sender;

            if (Character.IsFriendOfCharacter(loggedOutCharacter.Item1) && Character != null && Character.CharacterId != loggedOutCharacter.Item1)
            {
                _client.SendPacket(Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("CHARACTER_LOGGED_OUT"), loggedOutCharacter.Item2), 10));
                _client.SendPacket(Character.GenerateFinfo(loggedOutCharacter.Item1, false));
            }
        }

        private int _packetCount = 0;

        private void TriggerHandler(string packetHeader, string packet, bool force, bool ignoreAuthority = false)
        {
            if (ServerManager.Instance.InShutdown || string.IsNullOrWhiteSpace(packetHeader))
            {
                return;
            }

            try
            {
                if (!IsDisposing)
                {
                    string[] key = HandlerMethods.Keys.FirstOrDefault(s => s.Any(m => string.Equals(m, packetHeader, StringComparison.CurrentCultureIgnoreCase)));
                    HandlerMethodReference methodReference = key != null ? HandlerMethods[key] : null;
                    if (methodReference != null)
                    {
                        if (!force && methodReference.Amount > 1 && !_waitForPacketsAmount.HasValue)
                        {
                            // we need to wait for more
                            _waitForPacketsAmount = methodReference.Amount;
                            _waitForPacketList.Add(packet != "" ? packet : $"1 {packetHeader} ");
                            return;
                        }

                        try
                        {
                            if (HasSelectedCharacter && Character.IsLocked &&
                                (!string.IsNullOrEmpty(Account?.LockCode) || (Account?.TwoFactorEnabled == true)) &&
                                methodReference.ParentHandler.GetType().Name != "LockPacketHandler")
                            {
                                SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHAR_LOCKED"), 0));
                                return;
                            }

                            if (HasSelectedCharacter ||
                                methodReference.AnonymousAccess ||
                                methodReference.ParentHandler.GetType().Name == "EntryPointPacketHandler" ||
                                methodReference.ParentHandler.GetType().Name == "LoginPacketHandler")
                            {
                                // call actual handler method
                                if (methodReference.PacketDefinitionParameterType != null)
                                {
                                    //check for the correct authority
                                    if (!IsAuthenticated || Account?.Authority >= methodReference.Authority || ignoreAuthority)
                                    {
                                        PacketDefinition deserializedPacket = PacketFactory.Deserialize(packet,
                                            methodReference.PacketDefinitionParameterType, IsAuthenticated);

                                        if (HasSelectedCharacter && methodReference.Authority > AuthorityType.User)
                                        {
                                            if (Character?.Family != null && Character?.Authority > AuthorityType.GS)
                                            {
                                                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                                                {
                                                    DestinationCharacterId = Character?.Family?.FamilyId,
                                                    SourceCharacterId = Character.CharacterId,
                                                    SourceWorldId = ServerManager.Instance.WorldId,
                                                    Message = UserInterfaceHelper.GenerateSay($"[Command]: [{Character.Authority}]{Character.Name} used the command {packet.TrimUntil('$')} on Channel {ServerManager.Instance.ChannelId}.", 15),
                                                    Type = MessageType.Family,
                                                });
                                            }
                                        }

                                        if (deserializedPacket != null || methodReference.PassNonParseablePacket)
                                        {
                                            methodReference.HandlerMethod(methodReference.ParentHandler,
                                                deserializedPacket);
                                        }
                                        else
                                        {
                                            Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("CORRUPT_PACKET"),
                                                packetHeader, packet));
                                        }
                                    }
                                }
                                else
                                {
                                    methodReference.HandlerMethod(methodReference.ParentHandler, packet);
                                }
                            }
                        }
                        catch (DivideByZeroException ex)
                        {
                            // disconnect if something unexpected happens
                            Logger.Log.Error("Handler Error SessionId: " + SessionId, ex);
                            Disconnect();
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error("Handler Error SessionId: " + SessionId, e);
                            Disconnect();
                        }
                    }
                    else
                    {
                        if (!_isWorldServer)
                        {
                            if (_packetCount % 250 == 0)
                            {
                                Logger.Log.Warn("Current connections: " + _packetCount);
                            }

                            AntiSpamModule.Instance.AddToList(CleanIpAddress);
                            _packetCount++;
                        }
                        else
                        {
                            if (Account == null) // Just make sure the user isn't online when sending an unknown packet. This makes sure we don't ip ban players
                            {
                                if (_packetCount % 250 == 0)
                                {
                                    Logger.Log.Warn("Current connections: " + _packetCount);
                                }

                                AntiSpamModule.Instance.AddToList(CleanIpAddress);
                                _packetCount++;
                            }

                            Logger.Log.Warn($"{ string.Format(Language.Instance.GetMessageFromKey("HANDLER_NOT_FOUND"), packetHeader)} From IP: {_client.IpAddress}");
                        }
                    }
                }
                else
                {
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("CLIENTSESSION_DISPOSING"), packetHeader));
                }
            }
            catch
            {
                // break;
            }
        }

        #endregion
    }
}