using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.BattlePass
{
    public class OpenBattlePassPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public OpenBattlePassPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// bp_open packet
        /// </summary>
        /// <param name="bpOpen"></param>
        public void BattlePassOpen(BpOpen bpOpen)
        {
            Session.SendPacket(Session.Character.GenerateBpm());
            Session.SendPacket(Session.Character.GenerateBpp());
            Session.SendPacket(UserInterfaceHelper.GenerateBpt());
            Session.SendPacket("bpo");
        }
    }
}
