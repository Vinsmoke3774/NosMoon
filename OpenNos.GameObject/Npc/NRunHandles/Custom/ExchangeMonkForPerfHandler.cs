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

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.ExchangeMonkVsPerf)]
    public class ExchangeMonkForPerfHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeMonkForPerfHandler(ClientSession session) : base(session)
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
            ItemInstance spitem = Session?.Character?.Inventory?.LoadBySlotAndType(0, InventoryType.Equipment);
            if (spitem?.Item == null || spitem.Item.VNum != 4497)
            {
                // No SP
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_SP_TO_TRADE"),
                    11));
                return;
            }

            if (spitem?.Item != null && spitem.Item.VNum == 4497)
            {
                Session.Character.GiftAdd(2519, (byte) ServerManager.RandomNumber(2, 5));
                Session.Character.Inventory.RemoveItemFromInventory(spitem.Id, 1);
            }
        }
    }
}
