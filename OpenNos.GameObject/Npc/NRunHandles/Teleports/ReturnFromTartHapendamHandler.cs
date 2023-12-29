using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.ReturnFromTartHapendam)]
    public class ReturnFromTartHapendamHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ReturnFromTartHapendamHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 145, 50, 38);
        }
    }
}
