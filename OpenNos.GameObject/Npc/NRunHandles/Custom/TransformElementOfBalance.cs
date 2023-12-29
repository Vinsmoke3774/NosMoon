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

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.TransformElementOfBalance)]
    public class TransformElementOfBalance : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TransformElementOfBalance(ClientSession session) : base(session)
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
            if (Session.Character.Inventory.CountItem(2803) < 25)
            {
                // Not enough Element of Balance
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEM"), 11));
                return;
            }

            if (Session.Character.Inventory.CountItem(2803) >= 25)
            {
                Session.Character.GiftAdd(2805, 1);
                Session.Character.Inventory.RemoveItemAmount(2803, 25);
                return;
            }
        }
    }
}
