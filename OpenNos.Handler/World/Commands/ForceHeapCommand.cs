using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Commands
{
    public class ForceHeapCommand : IPacketHandler
    {
        private ClientSession Session { get; }

        public ForceHeapCommand(ClientSession session) => Session = session;

        public void Execute(ForceHeapPacket packet)
        {
            LargeHeapCompactor.CompactHeap();
        }
    }
}
