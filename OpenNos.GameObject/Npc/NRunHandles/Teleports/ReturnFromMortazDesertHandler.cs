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
    [NRunHandler(NRunType.ReturnFromMortazDesert)]
    public class ReturnFromMortazDesertHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ReturnFromMortazDesertHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeTeleporter(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Tp.MapId, Tp.MapX, Tp.MapY);
        }
    }
}
