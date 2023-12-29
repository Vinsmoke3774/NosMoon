using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class PreqPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PreqPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// PreqPacket packet
        /// </summary>
        /// <param name="packet"></param>
        public void Preq(PreqPacket packet)
        {
            if (Session.Character.IsSeal)
            {
                return;
            }

            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

            double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;

            if (timeSpanSinceLastPortal < 4 || !Session.HasCurrentMapInstance)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                return;
            }

            if (Session.Character.MapInstance?.MapInstanceType == MapInstanceType.ArenaInstance &&
                (Session.Character.LastDefence.AddSeconds(5) > DateTime.Now || Session.Character.LastSkillUse.AddSeconds(5) > DateTime.Now))
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE_IN_BATTLE"), 10));
                return;
            }

            if (!(Session.CurrentMapInstance.Portals.Concat(Session.Character.GetExtraPortal(Session.Character.Faction))
                .FirstOrDefault(s => Session.Character.PositionY >= s.SourceY - 1
                                     && Session.Character.PositionY <= s.SourceY + 1
                                     && Session.Character.PositionX >= s.SourceX - 1
                                     && Session.Character.PositionX <= s.SourceX + 1) is Portal portal))
            {
                return;
            }

            switch (Session.CurrentMapInstance.MapInstanceType)
            {
                case MapInstanceType.Act4Morcos:
                case MapInstanceType.Act4Calvina:
                case MapInstanceType.Act4Berios:
                case MapInstanceType.Act4Hatus:
                    {
                        var destinationMap = ServerManager.GetMapInstance(portal.DestinationMapInstanceId);

                        if (destinationMap == null)
                        {
                            break;
                        }

                        if (Session.Character.Family?.Act4Raid == null || Session.Character.Family.Act4RaidBossMap == null)
                        {
                            break;
                        }

                        if (portal.DestinationMapInstanceId == Session.Character.Family.Act4RaidBossMap.MapInstanceId)
                        {
                            foreach (var session in Session.Character.Family.Act4Raid.Sessions.Where(s => s.HasCurrentMapInstance))
                            {
                                session.Character.LastPortal = currentRunningSeconds;
                                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, portal.DestinationMapInstanceId, portal.DestinationX, portal.DestinationY);
                            }
                        }
                    }
                    break;

                case MapInstanceType.RaidInstance:
                    {
                        if (portal.Type == (short) PortalType.Closed)
                        {
                            break;
                        }

                        var destinationMap = ServerManager.GetMapInstance(portal.DestinationMapInstanceId);

                        if (destinationMap == null || Session.Character.Group?.Sessions == null)
                        {
                            break;
                        }

                        if (destinationMap.Monsters.Any(s => s.IsBoss))
                        {
                            foreach (var session in Session.Character.Group.Sessions.Where(s => s.HasCurrentMapInstance))
                            {
                                session.Character.LastPortal = currentRunningSeconds;
                                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, portal.DestinationMapInstanceId, portal.DestinationX, portal.DestinationY);
                            }
                        }
                    }
                    break;
            }

            switch (portal.Type)
            {
                case (sbyte) PortalType.MapPortal:
                case (sbyte) PortalType.TSNormal:
                case (sbyte) PortalType.Open:
                case (sbyte) PortalType.Miniland:
                case (sbyte) PortalType.TSEnd:
                case (sbyte) PortalType.Exit:
                case (sbyte) PortalType.Effect:
                case (sbyte) PortalType.ShopTeleport:
                    break;

                case (sbyte) PortalType.Raid:
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        break;
                    }

                    if (Session.Character.Group?.Raid != null)
                    {
                        if (Session.Character.Group.IsLeader(Session))
                        {
                            Session.SendPacket($"qna #mkraid^0^275 {Language.Instance.GetMessageFromKey("RAID_START_QUESTION")}");
                        }
                        else
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_TEAM_LEADER"), 10));
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NEED_TEAM"), 10));
                    }

                    return;

                case (sbyte) PortalType.BlueRaid:
                case (sbyte) PortalType.DarkRaid:
                    if (!packet.Parameter.HasValue)
                    {
                        Session.SendPacket($"qna #preq^1 {string.Format(Language.Instance.GetMessageFromKey("ACT4_RAID_ENTER"), Session.Character.Level * 5)}");
                        return;
                    }
                    else
                    {
                        if (packet.Parameter != 1)
                        {
                            return;
                        }

                        if ((int) Session.Character.Faction != portal.Type - 9 || Session.Character.Family?.Act4Raid == null)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                            return;
                        }

                        if (Session.Character.Level <= 49)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_LVL"), 10));
                            return;
                        }

                        if (Session.Character.Reputation <= 10000)
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_REP"), 10));
                            return;
                        }

                        Session.Character.GetReputation(Session.Character.Level * -5);

                        Session.Character.LastPortal = currentRunningSeconds;

                        switch (Session.Character.Family.Act4Raid.MapInstanceType)
                        {
                            case MapInstanceType.Act4Morcos:
                                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                                    Session.Character.Family.Act4Raid.MapInstanceId, 43, 179);
                                break;

                            case MapInstanceType.Act4Hatus:
                                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                                    Session.Character.Family.Act4Raid.MapInstanceId, 15, 9);
                                break;

                            case MapInstanceType.Act4Calvina:
                                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                                    Session.Character.Family.Act4Raid.MapInstanceId, 24, 6);
                                break;

                            case MapInstanceType.Act4Berios:
                                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                                    Session.Character.Family.Act4Raid.MapInstanceId, 20, 20);
                                break;
                        }
                    }

                    return;

                default:
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                    return;
            }

            if (Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance
                && Session?.Character?.Timespace != null && !Session.Character.Timespace.InstanceBag.Lock)
            {
                if (Session.Character.CharacterId == Session.Character.Timespace.InstanceBag.CreatorId)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateDialog($"#rstart^1 rstart {Language.Instance.GetMessageFromKey("FIRST_ROOM_START")}"));
                }

                return;
            }

            if (Session?.CurrentMapInstance?.MapInstanceType != MapInstanceType.BaseMapInstance &&
                portal.DestinationMapId == 134)
            {
                if (!packet.Parameter.HasValue)
                {
                    Session.SendPacket($"qna #preq^1 {Language.Instance.GetMessageFromKey("ACT4_RAID_EXIT")}");
                    return;
                }
            }


            portal.OnTraversalEvents.ForEach(e => EventHelper.Instance.RunEvent(e));

            if (portal?.OnTraversalEvents?.Where(s => s.EventActionType == EventActionType.TELEPORT).Count() != 0)
            {
                return;
            }

            if (portal.DestinationMapInstanceId == default)
            {
                return;
            }

            if (ServerManager.Instance.ChannelId == 51)
            {
                if ((Session.Character.Faction == FactionType.Angel && portal.DestinationMapId == 131)
                    || (Session.Character.Faction == FactionType.Demon && portal.DestinationMapId == 130))
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PORTAL_BLOCKED"), 10));
                    return;
                }

                if ((portal.DestinationMapId == 130 || portal.DestinationMapId == 131)
                    && timeSpanSinceLastPortal < 15)
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                    return;
                }

                if (portal.DestinationMapId == 153 &&  (Session.Character.LastDefence.AddSeconds(10) > DateTime.Now || Session.Character.LastSkillUse.AddSeconds(10) > DateTime.Now))
                {
                    Session?.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE_IN_BATTLE"), 10));
                    return;
                }
            }

            Session.SendPacket(Session.CurrentMapInstance.GenerateRsfn());

            if (ServerManager.GetMapInstance(portal.SourceMapInstanceId).MapInstanceType != MapInstanceType.BaseMapInstance 
                && ServerManager.GetMapInstance(portal.SourceMapInstanceId).MapInstanceType
                != MapInstanceType.CaligorInstance
                && ServerManager.GetMapInstance(portal.DestinationMapInstanceId).MapInstanceType
                == MapInstanceType.BaseMapInstance)
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId,
                    Session.Character.MapX, Session.Character.MapY);
            }
            else if (portal.DestinationMapInstanceId == Session.Character.Miniland.MapInstanceId)
            {
                ServerManager.Instance.JoinMiniland(Session, Session);
            }
            else if (portal.DestinationMapId == 20000)
            {
                ClientSession sess = ServerManager.Instance.Sessions.FirstOrDefault(s =>
                    s.Character.Miniland.MapInstanceId == portal.DestinationMapInstanceId);
                if (sess != null)
                {
                    ServerManager.Instance.JoinMiniland(Session, sess);
                }
            }
            else
            {
                if (ServerManager.Instance.ChannelId == 51)
                {
                    short destinationX = portal.DestinationX;
                    short destinationY = portal.DestinationY;

                    if (portal.DestinationMapInstanceId == CaligorRaid.CaligorMapInstance?.MapInstanceId
                    ) /* Caligor Raid Map */
                    {
                        if (!packet.Parameter.HasValue)
                        {
                            Session.SendPacket(
                                $"qna #preq^1 {Language.Instance.GetMessageFromKey("CALIGOR_RAID_ENTER")}");
                            return;
                        }
                    }
                    else if (portal.DestinationMapId == 153) /* Unknown Land */
                    {
                        if (Session.Character.MapInstance == CaligorRaid.CaligorMapInstance &&
                            !packet.Parameter.HasValue)
                        {
                            Session.SendPacket(
                                $"qna #preq^1 {Language.Instance.GetMessageFromKey("CALIGOR_RAID_EXIT")}");
                            return;
                        }
                        else if (Session.Character.MapInstance != CaligorRaid.CaligorMapInstance)
                        {
                            if (destinationX <= 0 && destinationY <= 0)
                            {
                                switch (Session.Character.Faction)
                                {
                                    case FactionType.Angel:
                                        destinationX = 50;
                                        destinationY = 172;
                                        break;

                                    case FactionType.Demon:
                                        destinationX = 130;
                                        destinationY = 172;
                                        break;
                                }
                            }
                        }
                    }

                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                        portal.DestinationMapInstanceId, destinationX, destinationY);
                }
                else
                {
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                        portal.DestinationMapInstanceId, portal.DestinationX, portal.DestinationY);
                }
            }

            Session.Character.LastPortal = currentRunningSeconds;
        }
    }
}
