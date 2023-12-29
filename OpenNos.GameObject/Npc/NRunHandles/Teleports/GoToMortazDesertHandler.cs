using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToShipMortazDesert)]
    public class GoToMortazDesertHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToMortazDesertHandler(ClientSession session) : base(session)
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
            if (30000 > Session.Character.Gold)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                return;
            }
            Session.Character.Gold -= 30000;
            Session.SendPacket(Session.Character.GenerateGold());
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 170, 127, 46);
        }
    }
}
