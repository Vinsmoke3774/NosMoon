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

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.ReturnFromFernonToCylloan)]
    public class ReturnFromFernonToCylloanHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ReturnFromFernonToCylloanHandler(ClientSession session) : base(session)
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
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 228, 146, 41);
        }
    }
}
