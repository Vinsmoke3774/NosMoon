using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Family
{
    public class FamilyDepositPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyDepositPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// f_deposit packet
        /// </summary>
        /// <param name="fDepositPacket"></param>
        public void FamilyDeposit(FDepositPacket fDepositPacket)
        {
            if (fDepositPacket == null)
            {
                return;
            }

            if (Session.Character.Family == null
                || !(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
                  || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familydeputy
                  || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                      && Session.Character.Family.MemberAuthorityType != FamilyAuthorityType.NONE)
                  || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper
                      && Session.Character.Family.ManagerAuthorityType != FamilyAuthorityType.NONE)))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            ItemInstance item = Session.Character.Inventory.LoadBySlotAndType(fDepositPacket.Slot, fDepositPacket.Inventory);
            ItemInstance itemdest = Session.Character.Family.Warehouse.LoadBySlotAndType(fDepositPacket.NewSlot, InventoryType.FamilyWareHouse);

            if (itemdest != null)
            {
                return;
            }

            // check if the destination slot is out of range
            if (fDepositPacket.NewSlot > Session.Character.Family.WarehouseSize)
            {
                return;
            }

            // check if the character is allowed to move the item
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            if (fDepositPacket.Amount < 0)
            {
                return;
            }

            // actually move the item from source to destination
            Session.Character.Inventory.FDepositItem(fDepositPacket.Inventory, fDepositPacket.Slot, fDepositPacket.Amount, fDepositPacket.NewSlot, ref item, ref itemdest);
        }
    }
}
