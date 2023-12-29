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

using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public abstract class BroadcastableBase : IDisposable
    {
        #region Members

        /// <summary>
        /// List of all connected clients.
        /// </summary>
        private readonly ThreadSafeSortedList<long, ClientSession> _sessions;

        private bool _disposed;

        #endregion

        #region Instantiation

        protected BroadcastableBase()
        {
            LastUnregister = DateTime.Now.AddMinutes(-1);
            _sessions = new ThreadSafeSortedList<long, ClientSession>();
        }

        #endregion

        #region Properties

        public IEnumerable<Mate> Mates
        {
            get
            {
                List<Mate> mates = new List<Mate>();
                Sessions.ToList().ForEach(s => mates.AddRange(s.Character?.Mates.Where(m => m.IsTeamMember || m.IsTemporalMate)));
                return mates;
            }
        }

        public IEnumerable<ClientSession> Sessions => _sessions.Where(s => s.HasSelectedCharacter && !s.IsDisposing && s.IsConnected);

        protected DateTime LastUnregister { get; private set; }

        #endregion

        #region Methods

        public void Broadcast(string packet, ReceiverType receiver = ReceiverType.All) => Broadcast(null, packet, receiver);

        public void Broadcast(string packet, int xRangeCoordinate, int yRangeCoordinate) => Broadcast(new BroadcastPacket(null, packet, ReceiverType.AllInRange, xCoordinate: xRangeCoordinate, yCoordinate: yRangeCoordinate));

        public void Broadcast(PacketDefinition packet, ReceiverType receiver = ReceiverType.All) => Broadcast(null, packet, receiver);

        public void Broadcast(PacketDefinition packet, int xRangeCoordinate, int yRangeCoordinate) => Broadcast(new BroadcastPacket(null, PacketFactory.Serialize(packet), ReceiverType.AllInRange, xCoordinate: xRangeCoordinate, yCoordinate: yRangeCoordinate));

        public void Broadcast(ClientSession client, PacketDefinition packet, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1) => Broadcast(client, PacketFactory.Serialize(packet), receiver, characterName, characterId);

        public void Broadcast(BroadcastPacket packet)
        {
            try
            {
                SpreadBroadcastpacket(packet);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(null, ex);
            }
        }

        public void Broadcast(ClientSession client, string content, ReceiverType receiver = ReceiverType.All, string characterName = "", long characterId = -1)
        {
            try
            {
                if (client?.Character == null || !client.Character.InvisibleGm || content.StartsWith("out", StringComparison.CurrentCulture))
                {
                    SpreadBroadcastpacket(new BroadcastPacket(client, content, receiver, characterName, characterId));
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(null, ex);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public Character GetCharacterById(long characterId) => GetSessionByCharacterId(characterId)?.Character;

        public ClientSession GetSessionByCharacterId(long characterId) => _sessions.ContainsKey(characterId) ? _sessions[characterId] : null;

        public void RegisterSession(ClientSession session)
        {
            if (!session.HasSelectedCharacter)
            {
                return;
            }

            // Create a ChatClient and store it in a collection
            _sessions[session.Character.CharacterId] = session;
            if (session.HasCurrentMapInstance)
            {
                session.CurrentMapInstance.IsSleeping = false;
            }
        }

        public void UnregisterSession(long characterId)
        {
            // Get client from client list, if not in list do not continue
            ClientSession session = _sessions[characterId];
            if (session == null)
            {
                return;
            }

            // Remove client from online clients list
            _sessions.Remove(characterId);
            if (session.HasCurrentMapInstance && _sessions.Count == 0)
            {
                session.CurrentMapInstance.IsSleeping = true;
            }
            LastUnregister = DateTime.Now;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sessions.Dispose();
            }
        }

        private void SpreadBroadcastpacket(BroadcastPacket sentPacket)
        {
            if (Sessions == null || string.IsNullOrEmpty(sentPacket?.Packet))
            {
                return;
            }

            var sessions = new List<ClientSession>();

            switch (sentPacket.Receiver)
            {
                case ReceiverType.All: // send packet to everyone
                    if (sentPacket.Packet.StartsWith("out", StringComparison.CurrentCulture))
                    {
                        foreach (ClientSession session in Sessions.Where(s => s.Character == null || !s.Character.IsChangingMapInstance))
                        {
                            if (!session.HasSelectedCharacter)
                            {
                                continue;
                            }


                            if (sentPacket.Sender != null)
                            {
                                if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                            else
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    else
                    {
                        foreach (var session in Sessions)
                        {
                            if (session?.HasSelectedCharacter != true)
                            {
                                continue;
                            }

                            if (sentPacket.Sender != null)
                            {
                                if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId) || sentPacket.Packet.StartsWith("revive", StringComparison.CurrentCulture))
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                            else
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    break;

                case ReceiverType.AllExceptMe: // send to everyone except the sender
                    if (sentPacket.Packet.StartsWith("out", StringComparison.CurrentCulture))
                    {
                        foreach (var session in Sessions.Where(s => s != null && s?.SessionId != sentPacket.Sender?.SessionId && (s.Character == null || !s.Character.IsChangingMapInstance)))
                        {
                            if (!session.HasSelectedCharacter)
                            {
                                continue;
                            }

                            if (sentPacket.Sender != null)
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                            else
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    else
                    {
                        sessions = Sessions.Where(s => s?.SessionId != sentPacket.Sender?.SessionId).ToList();

                        foreach (var session in sessions)
                        {
                            if (session?.HasSelectedCharacter != true)
                            {
                                continue;
                            }

                            if (sentPacket.Sender != null)
                            {
                                if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                            else
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    break;

                case ReceiverType.AllExceptGroup:
                    if (sentPacket.Packet.StartsWith("out", StringComparison.CurrentCulture))
                    {
                        foreach (ClientSession session in Sessions.Where(s => s.SessionId != sentPacket.Sender.SessionId && (s.Character == null || !s.Character.IsChangingMapInstance)))
                        {
                            if (!session.HasSelectedCharacter)
                            {
                                continue;
                            }

                            if (sentPacket.Sender != null)
                            {
                                if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                            else
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    else
                    {
                        foreach (ClientSession session in Sessions.Where(s => s.SessionId != sentPacket.Sender.SessionId && (s.Character?.Group == null || (s.Character?.Group?.GroupId != sentPacket.Sender?.Character?.Group?.GroupId))))
                        {
                            if (session.HasSelectedCharacter && !sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    break;

                case ReceiverType.AllExceptMeAct4: // send to everyone except the sender(Act4)
                    sessions = Sessions.Where(s => s != null && s.HasSelectedCharacter && s.SessionId != sentPacket.Sender?.SessionId && (s.Character == null || !s.Character.IsChangingMapInstance)).ToList();

                    foreach (var session in sessions)
                    {
                        if (sentPacket.Sender != null)
                        {
                            if (sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                            {
                                continue;
                            }

                            if (session.Character.Faction == sentPacket.Sender.Character.Faction)
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                            else if (session.Account.Authority >= AuthorityType.TMOD /*|| session.Account.Authority == AuthorityType.Moderator*/)
                            {
                                string[] vals = sentPacket.Packet.Split(' ');
                                if (vals[0] == "say" && vals[3] == "13")
                                {
                                    vals[5] = $"[{sentPacket.Sender.Character.Faction}] {vals[5]}";
                                    sentPacket.Packet = string.Join(" ", vals);
                                }
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                        else
                        {
                            session.SendPacket(sentPacket.Packet);
                        }
                    }
                    break;

                case ReceiverType.AllInRange: // send to everyone which is in a range of 50x50
                    if (sentPacket.XCoordinate != 0 && sentPacket.YCoordinate != 0)
                    {
                        sessions = Sessions.Where(s => s != null && s.HasSelectedCharacter && s?.Character.IsInRange(sentPacket.XCoordinate, sentPacket.YCoordinate) == true && (s.Character == null || !s.Character.IsChangingMapInstance)).ToList();

                        foreach (var session in sessions)
                        {
                            if (sentPacket.Sender != null)
                            {
                                if (!sentPacket.Sender.Character.IsBlockedByCharacter(session.Character.CharacterId))
                                {
                                    session.SendPacket(sentPacket.Packet);
                                }
                            }
                            else
                            {
                                session.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    break;

                case ReceiverType.OnlySomeone:
                    if (sentPacket.SomeonesCharacterId > 0 || !string.IsNullOrEmpty(sentPacket.SomeonesCharacterName))
                    {
                        ClientSession targetSession = Sessions.SingleOrDefault(s => s.Character.CharacterId == sentPacket.SomeonesCharacterId || s.Character.Name == sentPacket.SomeonesCharacterName);
                        if (targetSession?.HasSelectedCharacter == true)
                        {
                            if (sentPacket.Sender != null)
                            {
                                if (!sentPacket.Sender.Character.IsBlockedByCharacter(targetSession.Character.CharacterId))
                                {
                                    targetSession.SendPacket(sentPacket.Packet);
                                }
                                else
                                {
                                    sentPacket.Sender.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                                }
                            }
                            else
                            {
                                targetSession.SendPacket(sentPacket.Packet);
                            }
                        }
                    }
                    break;

                case ReceiverType.AllNoEmoBlocked:
                    sessions = Sessions.Where(s => s?.Character.EmoticonsBlocked == false && s?.HasSelectedCharacter == true).ToList();

                    foreach (var session in sessions)
                    {
                        if (sentPacket.Sender?.Character.IsBlockedByCharacter(session.Character.CharacterId) == false)
                        {
                            session.SendPacket(sentPacket.Packet);
                        }
                    }
                    break;

                case ReceiverType.AllNoHeroBlocked:
                    sessions = Sessions.Where(s => s?.Character.HeroChatBlocked == false && s?.HasSelectedCharacter == true).ToList();

                    foreach (var session in sessions)
                    {
                        if (sentPacket.Sender?.Character.IsBlockedByCharacter(session.Character.CharacterId) == false)
                        {
                            session.SendPacket(sentPacket.Packet);
                        }
                    }
                    break;

                case ReceiverType.Group:
                    foreach (ClientSession session in Sessions.Where(s => s.Character?.Group != null && sentPacket.Sender?.Character?.Group != null && s.Character.Group.GroupId == sentPacket.Sender.Character.Group.GroupId))
                    {
                        session.SendPacket(sentPacket.Packet);
                    }
                    break;

                case ReceiverType.Unknown:
                    break;
            }
        }

        #endregion
    }
}