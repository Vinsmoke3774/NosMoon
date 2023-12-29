using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Commands
{
    public class FixEventCommand : IPacketHandler
    {
        private ClientSession Session { get; }

        public FixEventCommand(ClientSession session) => Session = session;

        public void Execute(FixEventPacket packet)
        {
            if (ServerManager.Instance.EventInWaiting)
            {
                ServerManager.Instance.EventInWaiting = false;
            }

            ServerManager.Instance.StartedEvents.Remove(packet.EventType);
            Session.SendPacket(Session.Character.GenerateSay($"Event has been removed from the event pool.", 10));
            Session.SendPacket(Session.Character.GenerateSay($"It should now start properly and can be started with $Event {packet.EventType}.", 10));
        }
    }
}
