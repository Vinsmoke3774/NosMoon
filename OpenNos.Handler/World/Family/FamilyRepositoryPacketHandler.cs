using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Family
{
    public class FamilyRepositoryPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyRepositoryPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// f_repos packet
        /// </summary>
        /// <param name="fReposPacket"></param>
        public void FamilyRepos(FReposPacket fReposPacket)
        {
            if (fReposPacket == null)
            {
                return;
            }

            if (fReposPacket.Amount < 1)
            {
                return;
            }

            if (Session.Character.Family == null
                || !(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
                  || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familydeputy
                  || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                      && Session.Character.Family.MemberAuthorityType == FamilyAuthorityType.ALL)
                  || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper
                      && Session.Character.Family.ManagerAuthorityType == FamilyAuthorityType.ALL)))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            // check if the character is allowed to move the item
            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            if (fReposPacket.NewSlot > Session.Character.Family.WarehouseSize)
            {
                return;
            }

            ItemInstance sourceInventory =
                Session.Character.Family.Warehouse.LoadBySlotAndType(fReposPacket.OldSlot,
                    InventoryType.FamilyWareHouse);
            ItemInstance destinationInventory =
                Session.Character.Family.Warehouse.LoadBySlotAndType(fReposPacket.NewSlot,
                    InventoryType.FamilyWareHouse);

            if (sourceInventory != null && fReposPacket.Amount <= sourceInventory.Amount)
            {
                if (destinationInventory == null)
                {
                    destinationInventory = sourceInventory.DeepCopy();
                    sourceInventory.Amount -= fReposPacket.Amount;
                    destinationInventory.Amount = fReposPacket.Amount;
                    destinationInventory.Slot = fReposPacket.NewSlot;
                    if (sourceInventory.Amount > 0)
                    {
                        destinationInventory.Id = Guid.NewGuid();
                    }
                    else
                    {
                        sourceInventory = null;
                    }
                }
                else
                {
                    if (destinationInventory.ItemVNum == sourceInventory.ItemVNum
                        && (byte)sourceInventory.Item.Type != 0)
                    {
                        if (destinationInventory.Amount + fReposPacket.Amount > 999)
                        {
                            int saveItemCount = destinationInventory.Amount;
                            destinationInventory.Amount = 999;
                            sourceInventory.Amount = (short)(saveItemCount + sourceInventory.Amount - 999);
                        }
                        else
                        {
                            destinationInventory.Amount += fReposPacket.Amount;
                            sourceInventory.Amount -= fReposPacket.Amount;
                            if (sourceInventory.Amount == 0)
                            {
                                DAOFactory.ItemInstanceDAO.Delete(sourceInventory.Id);
                                sourceInventory = null;
                            }
                        }
                    }
                    else
                    {
                        destinationInventory.Slot = fReposPacket.OldSlot;
                        sourceInventory.Slot = fReposPacket.NewSlot;
                    }
                }
            }

            if (sourceInventory?.Amount > 0)
            {
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(sourceInventory);
            }

            if (destinationInventory?.Amount > 0)
            {
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(destinationInventory);
            }

            Session.Character.Family.SendPacket((destinationInventory != null)
                ? destinationInventory.GenerateFStash()
                : UserInterfaceHelper.Instance.GenerateFStashRemove(fReposPacket.NewSlot));
            Session.Character.Family.SendPacket((sourceInventory != null)
                ? sourceInventory.GenerateFStash()
                : UserInterfaceHelper.Instance.GenerateFStashRemove(fReposPacket.OldSlot));

            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
        }

        /// <summary>
        /// f_withdraw packet
        /// </summary>
        /// <param name="fWithdrawPacket"></param>
        public void FamilyWithdraw(FWithdrawPacket fWithdrawPacket)
        {
            if (fWithdrawPacket == null)
            {
                return;
            }

            if (Session.Character.Family == null
                || !(Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
                  || Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familydeputy
                  || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                      && Session.Character.Family.MemberAuthorityType == FamilyAuthorityType.ALL)
                  || (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Familykeeper
                      && Session.Character.Family.ManagerAuthorityType == FamilyAuthorityType.ALL)))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            ItemInstance previousInventory = Session.Character.Family.Warehouse.LoadBySlotAndType(fWithdrawPacket.Slot, InventoryType.FamilyWareHouse);

            if (fWithdrawPacket.Amount <= 0 || previousInventory == null || fWithdrawPacket.Amount > previousInventory.Amount)
            {
                return;
            }

            ItemInstance item2 = previousInventory.DeepCopy();
            item2.Id = Guid.NewGuid();
            item2.Amount = fWithdrawPacket.Amount;
            item2.CharacterId = Session.Character.CharacterId;

            previousInventory.Amount -= fWithdrawPacket.Amount;
            if (previousInventory.Amount <= 0)
            {
                previousInventory = null;
            }

            List<ItemInstance> newInv = Session.Character.Inventory.AddToInventory(item2, item2.Item.Type);
            Session.Character.Family.SendPacket(UserInterfaceHelper.Instance.GenerateFStashRemove(fWithdrawPacket.Slot));
            if (previousInventory != null)
            {
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(previousInventory);
            }
            else
            {
                FamilyCharacter fhead = Session.Character.Family.FamilyCharacters.Find(s => s.Authority == FamilyAuthority.Head);
                if (fhead == null)
                {
                    return;
                }

                DAOFactory.ItemInstanceDAO.DeleteFromSlotAndType(fhead.CharacterId, fWithdrawPacket.Slot, InventoryType.FamilyWareHouse);
            }

            Session.Character.Family.InsertFamilyLog(FamilyLogType.WareHouseRemoved, Session.Character.Name, message: $"{item2.ItemVNum}|{fWithdrawPacket.Amount}");
        }
    }
}
