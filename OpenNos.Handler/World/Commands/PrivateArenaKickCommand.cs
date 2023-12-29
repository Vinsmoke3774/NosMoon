using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System.Linq;

namespace OpenNos.Handler.World.Commands
{
    public class PrivateArenaKickCommand : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PrivateArenaKickCommand(ClientSession session) => Session = session;

        public void Execute(PrivateArenaKickCommandPacket packet)
        {
            var arena = ServerManager.Instance.PrivateArenaMaps.Find(s => s.Instance == Session.CurrentMapInstance);

            if (arena == null) return;
            if (string.IsNullOrEmpty(packet.Name))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateSay(packet.ReturnHelp(), 10));
                return;
            }

            var sessionToKick = arena.Instance.Sessions.First(s => s.Character.Name == packet.Name);
            if (sessionToKick == null) return;
            ServerManager.Instance.ChangeMap(sessionToKick.Character.CharacterId, sessionToKick.Character.LastMapId, sessionToKick.Character.LastMapX, sessionToKick.Character.LastMapY);
        }
    }
}
