using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.FriendshipWings)]
    public class WingsOfFriendshipHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public WingsOfFriendshipHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (packet.Type == 199 && packet.Argument == 2)
            {
                if (Session.Character.IsSeal)
                {
                    return;
                }
                short[] listWingOfFriendship = { 2160, 2312, 10048 };
                short vnumToUse = -1;
                foreach (short vnum in listWingOfFriendship)
                {
                    if (Session.Character.Inventory.CountItem(vnum) > 0)
                    {
                        vnumToUse = vnum;
                        break;
                    }
                }
                bool isCouple = Session.Character.IsCoupleOfCharacter(packet.User);
                if (vnumToUse != -1 || isCouple)
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterId(packet.User);
                    if (session != null && !session.Character.IsChangingMapInstance)
                    {
                        if (Session.Character.IsFriendOfCharacter(packet.User))
                        {
                            if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.BaseMapInstance)
                            {
                                if (Session.Character.MapInstance.MapInstanceType
                                    != MapInstanceType.BaseMapInstance
                                    || (ServerManager.Instance.ChannelId == 51
                                     && Session.Character.Faction != session.Character.Faction))
                                {
                                    Session.SendPacket(
                                        Session.Character.GenerateSay(
                                            Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                                    return;
                                }

                                short mapy = session.Character.PositionY;
                                short mapx = session.Character.PositionX;
                                short mapId = session.Character.MapInstance.Map.MapId;

                                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapId, mapx, mapy);
                                if (!isCouple)
                                {
                                    Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                                }
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("USER_ON_INSTANCEMAP"), 0));
                            }
                        }
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_WINGS"), 10));
                }
            }
            else if (packet.Type == 199 && packet.Argument == 1)
            {
                if (Session.Character.IsSeal)
                {
                    return;
                }
                short[] listWingOfFriendship = { 2160, 2312, 10048 };
                short vnumToUse = -1;
                foreach (short vnum in listWingOfFriendship)
                {
                    if (Session.Character.Inventory.CountItem(vnum) > 0)
                    {
                        vnumToUse = vnum;
                    }
                }
                bool isCouple = Session.Character.IsCoupleOfCharacter(packet.User);
                if (vnumToUse != -1 || isCouple)
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterId(packet.User);
                    if (session != null)
                    {
                        if (Session.Character.IsFriendOfCharacter(packet.User))
                        {
                            if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.BaseMapInstance)
                            {
                                if (Session.Character.MapInstance.MapInstanceType
                                    != MapInstanceType.BaseMapInstance
                                    || (ServerManager.Instance.ChannelId == 51
                                     && Session.Character.Faction != session.Character.Faction))
                                {
                                    Session.SendPacket(
                                        Session.Character.GenerateSay(
                                            Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                                    return;
                                }
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("USER_ON_INSTANCEMAP"), 0));
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                        return;
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_WINGS"), 10));
                    return;
                }
                if (!Session.Character.IsFriendOfCharacter(packet.User))
                {
                    Session.SendPacket(Language.Instance.GetMessageFromKey("CHARACTER_NOT_IN_FRIENDLIST"));
                    return;
                }

                Session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 7, $"#guri^199^2^{packet.User}"));
            }
        }
    }
}
