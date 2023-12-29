using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class RaidManagePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RaidManagePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// rdPacket packet
        /// </summary>
        /// <param name="rdPacket"></param>
        public void RaidManage(RdPacket rdPacket)
        {
            Group grp;
            switch (rdPacket.Type)
            {
                // Join Raid
                case 1:
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }

                    ClientSession target = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                    if (rdPacket.Parameter == null && target?.Character?.Group == null && Session.Character.Group != null && Session.Character.Group.IsLeader(Session) && Session.Character.Group?.Sessions.FirstOrDefault() == Session)
                    {
                        new JoinGroupPacketHandler(Session).GroupJoin(new PJoinPacket
                        {
                            RequestType = GroupRequestType.Invited,
                            CharacterId = rdPacket.CharacterId
                        });
                    }
                    else if (Session.Character.Group == null)
                    {
                        new JoinGroupPacketHandler(Session).GroupJoin(new PJoinPacket
                        {
                            RequestType = GroupRequestType.Accepted,
                            CharacterId = rdPacket.CharacterId
                        });
                    }

                    break;

                // Leave Raid
                case 2:
                    if (Session.Character.Group == null || Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }

                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("LEFT_RAID")),
                            0));
                    if (Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId,
                            Session.Character.MapX, Session.Character.MapY);
                    }

                    grp = Session.Character.Group;
                    Session.SendPacket(Session.Character.GenerateRaid(1, true));
                    Session.SendPacket(Session.Character.GenerateRaid(2, true));
                    if (grp != null)
                    {
                        grp.LeaveGroup(Session);
                        grp.Sessions.ForEach(s =>
                        {
                            s.SendPacket(grp.GenerateRdlst());
                            s.SendPacket(s.Character.GenerateRaid(0));
                        });
                    }
                    break;

                // Kick from Raid
                case 3:
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }

                    if (Session.Character.Group?.IsLeader(Session) == true)
                    {
                        ClientSession chartokick = ServerManager.Instance.GetSessionByCharacterId(rdPacket.CharacterId);
                        if (chartokick.Character?.Group == null)
                        {
                            return;
                        }

                        chartokick.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("KICK_RAID"), 0));
                        grp = chartokick.Character?.Group;
                        chartokick.SendPacket(chartokick.Character?.GenerateRaid(1, true));
                        chartokick.SendPacket(chartokick.Character?.GenerateRaid(2, true));
                        grp?.LeaveGroup(chartokick);
                        grp?.Sessions.ForEach(s =>
                        {
                            s.SendPacket(grp.GenerateRdlst());
                            s.SendPacket(s.Character.GenerateRaid(0));
                        });
                    }

                    break;

                // Disolve Raid
                case 4:
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        return;
                    }

                    if (Session.Character.Group?.IsLeader(Session) == true)
                    {
                        grp = Session.Character.Group;

                        ClientSession[] grpmembers = new ClientSession[40];
                        grp.Sessions.CopyTo(grpmembers);
                        foreach (ClientSession targetSession in grpmembers)
                        {
                            if (targetSession != null)
                            {
                                targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                                targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                                targetSession.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("RAID_DISOLVED"), 0));
                                grp.LeaveGroup(targetSession);
                            }
                        }

                        ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                        ServerManager.Instance.ThreadSafeGroupList.Remove(grp.GroupId);
                    }

                    break;
            }
        }
    }
}
