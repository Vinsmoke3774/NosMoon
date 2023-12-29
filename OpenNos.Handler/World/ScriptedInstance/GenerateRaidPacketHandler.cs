using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class GenerateRaidPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public GenerateRaidPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// mkraid packet
        /// </summary>
        /// <param name="mkRaidPacket"></param>
        public void GenerateRaid(MkRaidPacket mkRaidPacket)
        {
            if (Session.Character.Group?.Raid == null || !Session.Character.Group.IsLeader(Session))
            {
                return;
            }

            if (ServerManager.Instance.Configuration.RaidPortalLimitation == true)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_PORTAL"), 0));
                return;
            }


            if ((Session.Character.Group.SessionCount <= 0 && Session.Character.Authority < AuthorityType.TGM) || !Session.Character.Group.Sessions.All(s => s.CurrentMapInstance == Session.CurrentMapInstance))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("RAID_TEAM_NOT_READY"), 0));
                return;
            }

            if (Session.Character.Group.Raid.FirstMap == null)
            {
                Session.Character.Group.Raid.LoadScript(MapInstanceType.RaidInstance, Session.Character, false);
            }

            if (Session.Character.Group.Raid.FirstMap == null)
            {
                return;
            }

            Session.Character.Group.Raid.InstanceBag.Lock = true;

            Session.Character.Group.Raid.InstanceBag.Lives = (short)Session.Character.Group.SessionCount;

            foreach (ClientSession session in Session.Character.Group.Sessions.GetAllItems())
            {
                if (session == null)
                {
                    continue;
                }

                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, session.Character.Group.Raid.FirstMap.MapInstanceId, session.Character.Group.Raid.StartX, session.Character.Group.Raid.StartY);
                session.SendPacket(StaticPacketHelper.GenerateRaidBf(0));
                session.SendPacket(session.Character.Group.GeneraterRaidmbf(session));
                session.SendPacket(session.Character.GenerateRaid(5));
                session.SendPacket(session.Character.GenerateRaid(4));
                session.SendPacket(session.Character.GenerateRaid(3));
                if (session.Character.Group.Raid.DailyEntries > 0)
                {
                    var entries = session.Character.Group.Raid.DailyEntries - session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == session.Character.Group.Raid.Id && s.Timestamp.Date == DateTime.Today);
                    session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("INSTANCE_ENTRIES"), entries), 10));
                }
            }

            ServerManager.Instance.GroupList.Remove(Session.Character.Group);

            Logger.Log.LogUserEvent("RAID_START", Session.GenerateIdentity(), $"RaidId: {Session.Character.Group.GroupId}");
        }
    }
}
