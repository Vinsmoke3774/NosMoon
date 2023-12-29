using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using System.Linq;

namespace OpenNos.Handler.World.Basic
{
    public class GuriPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public GuriPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// guri packet
        /// </summary>
        /// <param name="packet"></param>
        public void Guri(GuriPacket packet)
        {
            var index = Session.GuriHandlerMethods.FirstOrDefault(s => s.Key.Any(x => x.Equals((GuriType)packet.Type)));

            if (!Session.GuriHandlerMethods.Keys.Any(s => s.Equals(index.Key)))
            {
                Logger.Log.Warn($"Guri type not found: {packet.Type}");
                return;
            }

            Session.GuriHandlerMethods[index.Key](packet);
        }
    }
}
