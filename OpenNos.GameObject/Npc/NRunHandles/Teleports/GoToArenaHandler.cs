using System;
using System.Diagnostics;
using System.Reactive.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToArena)]
    public class GoToArenaHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToArenaHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (Session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                return;
            }

            if (Session.Character.Group != null && Session.Character.Group.GroupType >= GroupType.BigTeam && Session.Character.Group.GroupType <= GroupType.GiantTeam)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo($"You cannot enter in arena with a raid team !"));
                return;
            }

            if (packet.Value == 1)
            {
                Session.SendPacket($"qna #n_run^{packet.Runner}^{packet.Type}^2^{packet.NpcId} {string.Format(Language.Instance.GetMessageFromKey("ASK_ENETER_GOLD"), 500 * (1 + packet.Type))}");
            }
            else
            {
                double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
                if (!(timeSpanSinceLastPortal >= 4) || !Session.HasCurrentMapInstance || ServerManager.Instance.ChannelId == 51 || Session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.ArenaInstance.MapInstanceId || Session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                    return;
                }
                if (packet.Type >= 0 && Session.Character.Gold >= 500 * (1 + packet.Type))
                {
                    Session.Character.ChargeValue = 0;
                    Session.Character.CurrentDie = 0;
                    Session.Character.CurrentKill = 0;
                    Session.Character.CurrentTc = 0;
                    Session.Character.LastPortal = currentRunningSeconds;
                    Session.Character.Gold -= 500 * (1 + packet.Type);
                    Session.Character.DisableBuffs(BuffType.All);
                    foreach (var mateTeam in Session.Character.Mates?.Where(s => s.IsTeamMember && s.MateType == MateType.Partner))
                    {
                        if (mateTeam == null) continue;
                        mateTeam.RemoveTeamMember(true);
                    }
                    Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(s => Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_ARENABUFFS"), 0)));
                    Session.SendPacket(Session.Character.GenerateGold());
                    MapCell pos = packet.Type == 0 ? ServerManager.Instance.ArenaInstance.Map.GetRandomPosition() : ServerManager.Instance.FamilyArenaInstance.Map.GetRandomPosition();
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, packet.Type == 0 ? ServerManager.Instance.ArenaInstance.MapInstanceId : ServerManager.Instance.FamilyArenaInstance.MapInstanceId, pos.X, pos.Y);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                }
            }
        }
    }
}
