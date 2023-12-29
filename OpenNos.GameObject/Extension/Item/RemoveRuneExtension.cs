using NosTale.Configuration;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Extension.Item
{
    public static class RemoveRuneExtension
    {
        #region Methods

        public static void RemoveRune(this ItemInstance e, ClientSession s)
        {
            if (e.Item.EquipmentSlot != EquipmentType.MainWeapon)
            {
                // Not Main weapon
                s.SendShopEnd();
                return;
            }

            var get = GameConfiguration.RRemove;

            if (s.Character.Inventory.CountItem(get.ItemVnum) < 1)
            {
                // Not Enough Item
                s.SendShopEnd();
                return;
            }

            e.RuneEffects.Clear();
            e.RuneAmount = 0;
            DAOFactory.ShellEffectDAO.DeleteByEquipmentSerialId(e.EquipmentSerialId, true);
            var msg = $"The {e.Item.Name} rune has been removed";
            s.SendPacket(UserInterfaceHelper.GenerateMsg(msg, 0));
            s.SendPacket(UserInterfaceHelper.GenerateSay(msg, 11));
            s.Character.Inventory.RemoveItemAmount(get.ItemVnum);
            s.SendPacket(e.GenerateInventoryAdd());
            s.SendPacket(s.Character.GenerateEquipment());
            s.SendPacket(s.Character.GenerateEq());
            s.SendShopEnd();
        }

        #endregion
    }
}