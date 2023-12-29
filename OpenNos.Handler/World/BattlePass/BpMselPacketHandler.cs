using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.BattlePass
{
    public class BpMselPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public BpMselPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// bp_msel packet
        /// </summary>
        /// <param name="bpMsel"></param>
        public void BattlePassOpen(BpMsel bpMsel)
        {
            var missionReward = ServerManager.Instance.BattlePassQuests.Find(s => s.Id == bpMsel.Slot);

            if (missionReward == null) return;

            var canGetReward = Session.Character.BattlePassQuestLogs.Find(s => s.QuestId == bpMsel.Slot);

            if (canGetReward.AlreadyTaken) return;

            if (missionReward.MaxObjectiveValue != canGetReward.Advancement) return;

            Session.Character.BattlePassPoints += missionReward.Reward;
            Session.SendPacket(UserInterfaceHelper.GenerateSay($"You have received {missionReward.Reward} Battle Points", 10));
            Session.SendPacket(Session.Character.GenerateBpp());

            var index = Session.Character.BattlePassQuestLogs.IndexOf(canGetReward);
            Session.Character.BattlePassQuestLogs[index].AlreadyTaken = true;
            Session.SendPacket(Session.Character.GenerateBpm());
        }
    }
}