using System.Collections.Generic;
using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.UserInterface
{
    [NRunHandler(NRunType.OpenProduction)]
    public class OpenProductionHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public OpenProductionHandler(ClientSession session) : base(session)
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
            Session.SendPacket("wopen 27 0");
            string recipelist = "m_list 2";
            if (Npc != null)
            {
                List<Recipe> tps = Npc.Recipes;
                recipelist = tps.Where(s => s.Amount > 0).Aggregate(recipelist, (current, s) => current + $" {s.ItemVNum}");
                recipelist += " -100";
                Session.SendPacket(recipelist);
            }
        }
    }
}
