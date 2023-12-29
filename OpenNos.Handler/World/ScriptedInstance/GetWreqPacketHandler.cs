using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ServerPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class GetWreqPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public GetWreqPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// wreq packet
        /// </summary>
        /// <param name="packet"></param>
        public void GetWreq(WreqPacket packet)
        {
            short CharPositionX = Session.Character.PositionX;
            short CharPositionY = Session.Character.PositionY;
            foreach (GameObject.ScriptedInstance portal in Session.CurrentMapInstance.ScriptedInstances)
            {
                if (CharPositionY >= portal.PositionY - 1 && CharPositionY
                                                                        <= portal.PositionY + 1
                                                                        && CharPositionX
                                                                        >= portal.PositionX - 1
                                                                        && CharPositionX
                                                                        <= portal.PositionX + 1)
                {
                    switch (packet.Value)
                    {
                        case 0:
                            if (packet.Param != 1
                                && Session.Character.Group?.Sessions.Find(s =>
                                    s.CurrentMapInstance.InstanceBag?.Lock == false
                                    && s.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance
                                    && s.Character.CharacterId != Session.Character.CharacterId
                                    && s.Character.Timespace?.Id == portal.Id
                                    && !s.Character.Timespace.IsIndividual) is ClientSession TeamMemberInInstance)
                            {
                                if (portal.DailyEntries > 0)
                                {
                                    var entries = portal.DailyEntries - Session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == portal.Id && s.Timestamp.Date == DateTime.Today);
                                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("INSTANCE_ENTRIES"), entries), 10));
                                }
                                Session.SendPacket(UserInterfaceHelper.GenerateDialog(
                                    $"#wreq^3^{TeamMemberInInstance.Character.CharacterId} #wreq^0^1 {Language.Instance.GetMessageFromKey("ASK_JOIN_TEAM_TS")}"));
                            }
                            else
                            {
                                if (portal.DailyEntries > 0)
                                {
                                    var entries = portal.DailyEntries - Session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == portal.Id && s.Timestamp.Date == DateTime.Today);
                                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("INSTANCE_ENTRIES"), entries), 10));
                                }
                                Session.SendPacket(portal.GenerateRbr(Session.Character));
                            }

                            break;

                        case 1:
                            if (!packet.Param.HasValue)
                            {
                                Session.Character.EnterInstance(portal);
                            }
                            break;

                        case 3:
                            ClientSession clientSession =
                                Session.Character.Group?.Sessions.Find(s => s.Character.CharacterId == packet.Param);
                            if (clientSession != null && clientSession.CurrentMapInstance.InstanceBag?.Lock == false && clientSession.Character?.Timespace is GameObject.ScriptedInstance TeamTimeSpace && !TeamTimeSpace.IsIndividual)
                            {
                                if (portal.Id == TeamTimeSpace.Id)
                                {
                                    if (Session.Character.Level < TeamTimeSpace.LevelMinimum)
                                    {
                                        Session.SendPacket(
                                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_LOW_LVL"), 0));
                                        return;
                                    }
                                    if (Session.Character.Level > TeamTimeSpace.LevelMaximum)
                                    {
                                        Session.SendPacket(
                                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_HIGH_LVL"), 0));
                                        return;
                                    }

                                    var entries = TeamTimeSpace.DailyEntries - Session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == TeamTimeSpace.Id && s.Timestamp.Date == DateTime.Today);
                                    if (TeamTimeSpace.DailyEntries == 0 || entries > 0)
                                    {
                                        foreach (Gift gift in TeamTimeSpace.RequiredItems)
                                        {
                                            if (Session.Character.Inventory.CountItem(gift.VNum) < gift.Amount)
                                            {
                                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                                    string.Format(Language.Instance.GetMessageFromKey("NO_ITEM_REQUIRED"),
                                                        ServerManager.GetItem(gift.VNum).Name), 0));
                                                return;
                                            }

                                            Session.Character.Inventory.RemoveItemAmount(gift.VNum, gift.Amount);
                                        }
                                        Session?.SendPackets(TeamTimeSpace.GenerateMinimap());
                                        Session?.SendPacket(TeamTimeSpace.GenerateMainInfo());
                                        Session?.SendPacket(TeamTimeSpace.FirstMap.InstanceBag.GenerateScore());
                                        if (TeamTimeSpace.StartX != 0 || TeamTimeSpace.StartY != 0)
                                        {
                                            ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                                                clientSession.CurrentMapInstance.MapInstanceId, TeamTimeSpace.StartX, TeamTimeSpace.StartY);
                                        }
                                        else
                                        {
                                            ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, clientSession.CurrentMapInstance.MapInstanceId);
                                        }
                                        Session.Character.Timespace = TeamTimeSpace;
                                    }
                                    else
                                    {
                                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANCE_NO_MORE_ENTRIES"), 0));
                                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INSTANCE_NO_MORE_ENTRIES"), 10));
                                    }
                                }
                            }
                            else
                            {
                                GetWreq(new WreqPacket { Value = 0, Param = 1 });
                            }

                            // TODO: Implement
                            break;
                    }
                }
            }
        }
    }
}
