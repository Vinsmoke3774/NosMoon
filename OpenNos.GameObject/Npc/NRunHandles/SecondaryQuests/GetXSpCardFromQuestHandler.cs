using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.SecondaryQuests
{
    [NRunHandler(NRunType.GetXSPCard)]
    public class GetXSpCardFromQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetXSpCardFromQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            switch (packet.Type)
            {
                case 1: // Pajama
                    {
                        if (Session.Character.MapInstance.Npcs.Any(s => s.NpcVNum == 932))
                        {
                            /**
                             * TODO:
                             *
                             * Session.Character.GiftAdd(900, 1);
                             *
                             */

                            return;
                        }
                    }
                    break;

                case 2: // SP 1
                    {
                        if (Session.Character.MapInstance.Npcs.Any(s => s.NpcVNum == 933))
                        {
                            /**
                             * TODO:
                             *
                             * switch (Session.Character.Class)
                             * {
                             *     case ClassType.Swordsman:
                             *         Session.Character.GiftAdd(901, 1);
                             *         break;
                             *     case ClassType.Archer:
                             *         Session.Character.GiftAdd(903, 1);
                             *         break;
                             *     case ClassType.Magician:
                             *         Session.Character.GiftAdd(905, 1);
                             *         break;
                             * }
                             *
                             */

                            return;
                        }
                    }
                    break;

                case 3: // SP 2
                    {
                        if (Session.Character.MapInstance.Npcs.Any(s => s.NpcVNum == 934))
                        {
                            /**
                             * TODO:
                             *
                             * switch (Session.Character.Class)
                             * {
                             *     case ClassType.Swordsman:
                             *         Session.Character.GiftAdd(902, 1);
                             *         break;
                             *     case ClassType.Archer:
                             *         Session.Character.GiftAdd(904, 1);
                             *         break;
                             *     case ClassType.Magician:
                             *         Session.Character.GiftAdd(906, 1);
                             *         break;
                             * }
                             *
                             */

                            return;
                        }
                    }
                    break;
            }
        }
    }
}
