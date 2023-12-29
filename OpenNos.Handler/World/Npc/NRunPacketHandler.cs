using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.GameObject;
using System.Linq;

namespace OpenNos.Handler.World.Npc
{
    public class NRunPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public NRunPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// n_run packet
        /// </summary>
        /// <param name="packet"></param>
        public void NpcRunFunction(NRunPacket packet)
        {
            Session.Character.LastNRunId = packet.NpcId;
            Session.Character.LastItemVNum = 0;

            if (Session.Character.Hp <= 0)
            {
                return;
            }

            var index = Session.NrunHandlerMethods
                .FirstOrDefault(s => s.Key.Any(x => x.Equals(packet.Runner)));

            if (Session.NrunHandlerMethods.Keys.Any(s => s.Equals(index.Key)))
            {
                Session.NrunHandlerMethods[index.Key](packet);
                return;
            }

            Logger.Log.Warn($"NRun Handler not found: {packet.Runner}");
        }
    }
}
