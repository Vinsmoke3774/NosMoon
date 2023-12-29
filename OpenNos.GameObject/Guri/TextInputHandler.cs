using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Core.Extensions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Fku;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.TextInput)]
    public class TextInputHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public TextInputHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            var arena = ServerManager.Instance.PrivateArenaMaps.Find(s => s.Password == packet.Value);
            var arenaNpc = Session.CurrentMapInstance.Npcs.FirstOrDefault(s => s.Dialog == 100000 && s.NpcVNum == 920);
            const int speakerVNum = 2173;
            const int limitedSpeakerVNum = 10028;
            string[] filter = new string[] { "nig", "fuck", "bitch", "ass", "bastard", "cunt", "milf", "shit", "fag" };
            if (packet.Argument == 1)
            {
                if (packet.Value.Contains("^"))
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INVALID_NAME")));
                    return;
                }
                short[] listPetnameVNum = { 2157, 10023 };
                short vnumToUse = -1;
                foreach (short vnum in listPetnameVNum)
                {
                    if (Session.Character.Inventory.CountItem(vnum) > 0)
                    {
                        vnumToUse = vnum;
                    }
                }
                Mate mate = Session.Character.Mates.Find(s => s.MateTransportId == packet.Data);
                if (mate != null && Session.Character.Inventory.CountItem(vnumToUse) > 0)
                {
                    mate.Name = packet.Value.Truncate(16);
                    Session.CurrentMapInstance?.Broadcast(mate.GenerateOut(), ReceiverType.AllExceptMe);
                    Parallel.ForEach(Session.CurrentMapInstance.Sessions.Where(s => s.Character != null), s =>
                    {
                        if (ServerManager.Instance.ChannelId != 51 || Session.Character.Faction == s.Character.Faction)
                        {
                            s.SendPacket(mate.GenerateIn(false, ServerManager.Instance.ChannelId == 51));
                        }
                        else
                        {
                            s.SendPacket(mate.GenerateIn(true, ServerManager.Instance.ChannelId == 51, s.Account.Authority));
                        }
                    });

                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_NAME_PET")));
                    Session.SendPacket(Session.Character.GeneratePinit());
                    Session.SendPackets(Session.Character.GeneratePst());
                    Session.SendPackets(Session.Character.GenerateScP());
                    Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                }
            }

            // presentation message
            if (packet.Argument == 2)
            {
                int presentationVNum = Session.Character.Inventory.CountItem(1117) > 0
                    ? 1117
                    : (Session.Character.Inventory.CountItem(9013) > 0 ? 9013 : -1);
                if (presentationVNum != -1)
                {
                    string message = "";
                    string[] valuesplit = packet.Value?.Split(' ');
                    if (valuesplit == null)
                    {
                        return;
                    }

                    for (int i = 0; i < valuesplit.Length; i++)
                    {
                        if (filter.Contains(valuesplit[i])) message += "$@_éè&à$^ù" + "^";
                        else message += valuesplit[i] + "^";
                    }

                    message = message.Substring(0, message.Length - 1); // Remove the last ^
                    message = message.Trim();
                    if (message.Length > 60)
                    {
                        message = message.Substring(0, 60);
                    }

                    Session.Character.Biography = message;
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INTRODUCTION_SET"),
                            10));
                    Session.Character.Inventory.RemoveItemAmount(presentationVNum);
                }
            }

            // Speaker
            if (packet.Argument == 3 && (Session.Character.Inventory.CountItem(speakerVNum) > 0 || Session.Character.Inventory.CountItem(limitedSpeakerVNum) > 0) && (!Session.Character.CanCreatePrivateArena && arena == null))
            {
                string message = string.Empty;// $"<Speaker CH-{ServerManager.Instance.ChannelId}> [{Session.Character.Name}]:";
                var sayPacket = "";
                byte sayItemInventory = 0;
                short sayItemSlot = 0;
                var baseLength = message.Length;
                var valuesplit = packet.Value?.Split(' ');
                if (valuesplit == null)
                {
                    return;
                }

                if (packet.Data == 999 && (valuesplit.Length < 3 || !byte.TryParse(valuesplit[0], out sayItemInventory) || !short.TryParse(valuesplit[1], out sayItemSlot)))
                {
                    return;
                }

                for (var i = 0 + (packet.Data == 999 ? 2 : 0); i < valuesplit.Length; i++)
                {
                    if (filter.Contains(valuesplit[i])) message += "$@_éè&à$^ù" + " ";
                    else message += valuesplit[i] + " ";
                }

                if (message.Length > 120 + baseLength)
                {
                    message = message.Substring(0, 120 + baseLength);
                }

                if (Session.Character.LastSpeakerUse > DateTime.Now.AddSeconds(-10))
                {
                    Session.SendPacket(Session.Character.GenerateSay($"Unable to use Speaker for {10 - (DateTime.Now - Session.Character.LastSpeakerUse).Seconds} seconds", 10));
                    return;
                }

                message = message.Trim();

                if (packet.Data == 999)
                {
                    if (Session.Character.Inventory.LoadBySlotAndType(sayItemSlot, (InventoryType)sayItemInventory) is ItemInstance item)
                    {
                        sayPacket = $"{message.Replace(' ', '|')} {(item.Item.EquipmentSlot == EquipmentType.Sp ? item.GenerateSlInfo() : item.GenerateEInfo())}";
                    }
                }
                else
                {
                    sayPacket = message;
                }

                Session.Character.LastSpeakerUse = DateTime.Now;

                if (Session.Character.IsMuted())
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(
                            Language.Instance.GetMessageFromKey("SPEAKER_CANT_BE_USED"), 10));
                    return;
                }

                //ServerManager.Instance.ChatLogs.Add(new ChatLogDTO
                //{
                //    CharacterId = Session.Character.CharacterId,
                //    CharacterName = Session.Character.Name,
                //    DateTime = DateTime.Now,
                //    MessageType = "Speaker",
                //    Message = message
                //});

                if (Session.Character.Inventory.CountItem(limitedSpeakerVNum) > 0)
                {
                    Session.Character.Inventory.RemoveItemAmount(limitedSpeakerVNum);
                }
                else
                {
                    Session.Character.Inventory.RemoveItemAmount(speakerVNum);
                }

                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                {
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = sayPacket,
                    Type = MessageType.Speaker,
                    IsItemSpeaker = packet.Data == 999
                });
            }

            // Private arena password :)
            else if (packet.User == 0 && packet.Argument == 3 && (Session.Character.CanCreatePrivateArena || (arena != null && Session.Character.IsInRange(arenaNpc.MapX, arenaNpc.MapY, 2))))
            {
                bool alreadyCreatedArena = ServerManager.Instance.PrivateArenaMaps.Any(s => s.OwnerId == Session.Character.CharacterId);
                if (alreadyCreatedArena) return;

                const int arenaMapId = 2006; // TODO : Update it it's just a random value now
                if (arena == null)
                {
                    MapNpc signPost = new()
                    {
                        NpcVNum = 920,
                        MapX = Session.Character.PositionX,
                        MapY = Session.Character.PositionY,
                        MapId = Session.CurrentMapInstance.Map.MapId,
                        ShouldRespawn = false,
                        IsMoving = false,
                        MapNpcId = Session.CurrentMapInstance.GetNextNpcId(),
                        Owner = Session.Character.BattleEntity,
                        Position = 2,
                        Dialog = 100000,
                        Name = $"{Session.Character.Name}'s^[PrivateArena]"
                    };

                    signPost.AliveTime = int.MaxValue;
                    signPost.Initialize(Session.CurrentMapInstance);
                    Session.CurrentMapInstance.AddNPC(signPost);
                    Session.CurrentMapInstance.Broadcast(signPost.GenerateIn());
                    var mapInstance = ServerManager.GenerateMapInstance(arenaMapId, MapInstanceType.PrivateArenaInstance, new InstanceBag());
                    Portal portal = new()
                    {
                        SourceMapId = 2006,
                        SourceX = 37,
                        SourceY = 15,
                        DestinationMapId = 1,
                        DestinationX = 0,
                        DestinationY = 0,
                        Type = -1
                    };

                    mapInstance.CreatePortal(portal);
                    mapInstance.IsPVP = true;
                    Session.Character.LastMapId = Session.Character.MapId;
                    Session.Character.LastMapX = Session.Character.MapX;
                    Session.Character.LastMapY = Session.Character.MapY;
                    MapCell mapCell = mapInstance.Map.GetRandomPosition();
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, mapInstance.MapInstanceId, mapCell.X, mapCell.Y);
                    Session.Character.CanCreatePrivateArena = false;
                    ServerManager.Instance.PrivateArenaMaps.Add(new PrivateMapArena { OwnerId = Session.Character.CharacterId, Instance = mapInstance, Password = packet.Value });
                    return;
                }
                else if (arena != null && arenaNpc?.BattleEntity?.Character?.CharacterId == arena.OwnerId)
                {
                    Session.Character.LastMapId = Session.Character.MapId;
                    Session.Character.LastMapX = Session.Character.MapX;
                    Session.Character.LastMapY = Session.Character.MapY;
                    MapCell mapCell = arena.Instance.Map.GetRandomPosition();
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, arena.Instance.MapInstanceId, mapCell.X, mapCell.Y);
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo("Wrong password !"));
                }
            }

            // Bubble

            if (packet.Argument == 4)
            {
                int bubbleVNum = Session.Character.Inventory.CountItem(2174) > 0
                    ? 2174
                    : (Session.Character.Inventory.CountItem(10029) > 0 ? 10029 : -1);
                if (bubbleVNum != -1)
                {
                    string message = "";
                    string[] valuesplit = packet.Value?.Split(' ');
                    if (valuesplit == null)
                    {
                        return;
                    }

                    for (int i = 0; i < valuesplit.Length; i++)
                    {
                        if (filter.Contains(valuesplit[i])) message += "$@_éè&à$^ù" + "^";
                        else message += valuesplit[i] + "^";
                    }

                    message = message.Substring(0, message.Length - 1); // Remove the last ^
                    message = message.Trim();
                    if (message.Length > 60)
                    {
                        message = message.Substring(0, 60);
                    }

                    Session.Character.BubbleMessage = message;
                    Session.Character.BubbleMessageEnd = DateTime.Now.AddMinutes(30);
                    Session.SendPacket($"csp_r {Session.Character.BubbleMessage}");
                    Session.Character.Inventory.RemoveItemAmount(bubbleVNum);
                }
            }
        }
    }
}
