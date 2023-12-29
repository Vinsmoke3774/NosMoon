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
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.SpecialistInitializationPotion)]
    public class InitPotionHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public InitPotionHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            // SP points initialization
            int[] listPotionResetVNums = { 1366, 1427, 5115, 9040 };
            int vnumToUse = -1;
            foreach (int vnum in listPotionResetVNums)
            {
                if (Session.Character.Inventory.CountItem(vnum) > 0)
                {
                    vnumToUse = vnum;
                }
            }

            if (vnumToUse != -1)
            {
                if (Session.Character.UseSp)
                {
                    ItemInstance specialistInstance =
                        Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp,
                            InventoryType.Wear);
                    if (specialistInstance != null)
                    {
                        specialistInstance.SlDamage = 0;
                        specialistInstance.SlDefence = 0;
                        specialistInstance.SlElement = 0;
                        specialistInstance.SlHP = 0;

                        specialistInstance.DamageMinimum = 0;
                        specialistInstance.DamageMaximum = 0;
                        specialistInstance.HitRate = 0;
                        specialistInstance.CriticalLuckRate = 0;
                        specialistInstance.CriticalRate = 0;
                        specialistInstance.DefenceDodge = 0;
                        specialistInstance.DistanceDefenceDodge = 0;
                        specialistInstance.ElementRate = 0;
                        specialistInstance.DarkResistance = 0;
                        specialistInstance.LightResistance = 0;
                        specialistInstance.FireResistance = 0;
                        specialistInstance.WaterResistance = 0;
                        specialistInstance.CriticalDodge = 0;
                        specialistInstance.CloseDefence = 0;
                        specialistInstance.DistanceDefence = 0;
                        specialistInstance.MagicDefence = 0;
                        specialistInstance.HP = 0;
                        specialistInstance.MP = 0;

                        Session.Character.Inventory.RemoveItemAmount(vnumToUse);
                        Session.Character.Inventory.DeleteFromSlotAndType((byte)EquipmentType.Sp,
                            InventoryType.Wear);
                        Session.Character.Inventory.AddToInventoryWithSlotAndType(specialistInstance,
                            InventoryType.Wear, (byte)EquipmentType.Sp);
                        Session.SendPacket(Session.Character.GenerateCond());
                        Session.SendPacket(specialistInstance.GenerateSlInfo(Session));
                        Session.SendPacket(Session.Character.GenerateLev());
                        Session.SendPackets(Session.Character.GenerateStatChar());
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_RESET"),
                                0));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(
                            Language.Instance.GetMessageFromKey("TRANSFORMATION_NEEDED"), 10));
                }
            }
            else
            {
                Session.SendPacket(
                    Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POINTS"),
                        10));
            }
        }
    }
}
