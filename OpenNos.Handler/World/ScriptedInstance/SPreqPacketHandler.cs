using NosByte.Packets.ServerPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using System.Linq;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class SPreqPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SPreqPacketHandler(ClientSession session) => Session = session;

        public void LaunchTower(SreqPacket spreq)
        {
            var skytower = ServerManager.Instance.SkyTowers.FirstOrDefault(s => s.MapId == 2628 && s.Type == Domain.ScriptedInstanceType.SkyTower && s.Round == Session.Character.SkyTowerLevel);

            if (skytower != null)
            {
                Session.Character.EnterSkyTower(skytower);
            }
        }
    }
}
