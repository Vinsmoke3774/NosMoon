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
    [NRunHandler(NRunType.DowngradeHeroicEquipment)]
    public class DowngradeChampionEqHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public DowngradeChampionEqHandler(ClientSession session) : base(session)
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
            ItemInstance itemInstance = Session?.Character?.Inventory?.LoadBySlotAndType(0, InventoryType.Equipment);
            if (10000000 > Session.Character.Gold)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 11));
                return;
            }

            if (itemInstance?.Item != null && ((itemInstance.Item.IsHeroic)) && itemInstance.Rare >= 8)
            {
                Session.Character.Gold -= 10000000;
                Session.SendPacket(Session.Character.GenerateGold());
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PAY_RARE_DOWN"), 12));
                itemInstance.RarifyItem(Session, RarifyMode.HeroEquipmentDowngrade, RarifyProtection.None);
                Session.SendPacket(itemInstance.GenerateInventoryAdd());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_HEROIC"), 11));
                return;
            }
        }
    }
}
