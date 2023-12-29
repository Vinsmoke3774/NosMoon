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
    [NRunHandler(NRunType.GoToSPStone)]
    public class GoToSpStoneHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToSpStoneHandler(ClientSession session) : base(session)
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
            int gemNpcVnum = 0;

            switch (Npc.NpcVNum)
            {
                case 935:
                    gemNpcVnum = 932;
                    break;

                case 936:
                    gemNpcVnum = 933;
                    break;

                case 937:
                    gemNpcVnum = 934;
                    break;

                case 952:
                    gemNpcVnum = 948;
                    break;

                case 953:
                    gemNpcVnum = 954;
                    break;
            }

            if (ServerManager.Instance.SpecialistGemMapInstances?.FirstOrDefault(s => s.Npcs.Any(n => n.NpcVNum == gemNpcVnum)) is MapInstance specialistGemMapInstance)
            {
                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, specialistGemMapInstance.MapInstanceId, 3, 3);
            }
        }
    }
}
