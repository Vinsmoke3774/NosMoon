using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Npc
{
    public class SellShopPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SellShopPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// sell packet
        /// </summary>
        /// <param name="sellPacket"></param>
        public void SellShop(SellPacket sellPacket)
        {
            if (Session.Character.ExchangeInfo?.ExchangeList.Count > 0 || Session.Character.IsShopping)
            {
                return;
            }

            if (sellPacket.Amount.HasValue && sellPacket.Slot.HasValue && sellPacket.Amount.Value > 0)
            {
                InventoryType type = (InventoryType)sellPacket.Data;

                if (type == InventoryType.Bazaar)
                {
                    return;
                }

                short amount = sellPacket.Amount.Value, slot = sellPacket.Slot.Value;

                if (amount < 1)
                {
                    return;
                }

                ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot, type);

                if (inv == null || amount > inv.Amount)
                {
                    return;
                }

                if (Session.Character.MinilandObjects.Any(s => s.ItemInstanceId == inv.Id))
                {
                    return;
                }

                if (!inv.Item.IsSoldable)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateShopMemo(2, string.Format(Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"))));
                    return;
                }

                long price = inv.Item.SellToNpcPrice;

                if (price < 1)
                {
                    price = 0;
                }

                if (Session.Character.Gold + (price * amount) > ServerManager.Instance.Configuration.MaxGold)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                    return;
                }

                var titleEffect = Session.Character.EffectFromTitle.ToList().FirstOrDefault(s =>
                    s.Type == (byte)BCardType.CardType.BonusBCards && s.SubType ==
                    (byte)AdditionalTypes.BonusTitleBCards.IncreaseGoldEarned);

                var bonus = 0;

                if (titleEffect != null)
                {
                    bonus = titleEffect.FirstData;
                }

                double multiplier = (double)(1 + ((double)bonus / 100D));

                Session.Character.Gold += (int)((price * amount) * multiplier);
                Session.SendPacket(UserInterfaceHelper.GenerateShopMemo(1, string.Format(Language.Instance.GetMessageFromKey("SELL_ITEM_VALID"), inv.Item.Name, amount)));

                Session.Character.Inventory.RemoveItemFromInventory(inv.Id, amount);
                Session.SendPacket(Session.Character.GenerateGold());
            }
            else
            {
                short vnum = sellPacket.Data;

                CharacterSkill skill = Session.Character.Skills[vnum];

                if (skill == null || vnum == 200 + (20 * (byte)Session.Character.Class) || vnum == 201 + (20 * (byte)Session.Character.Class))
                {
                    return;
                }

                Session.Character.Gold -= skill.Skill.Price;
                Session.SendPacket(Session.Character.GenerateGold());

                foreach (CharacterSkill loadedSkill in Session.Character.Skills.GetAllItems())
                {
                    if (skill.Skill.SkillVNum == loadedSkill.Skill.UpgradeSkill)
                    {
                        Session.Character.Skills.Remove(loadedSkill.SkillVNum);
                    }
                }

                Session.Character.Skills.Remove(skill.SkillVNum);
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
                Session.SendPacket(Session.Character.GenerateLev());
            }
        }
    }
}
