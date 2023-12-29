using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class DeleteItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DeleteItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// b_i packet
        /// </summary>
        /// <param name="bIPacket"></param>
        public void AskToDelete(BIPacket bIPacket)
        {
            if (bIPacket != null)
            {
                switch (bIPacket.Option)
                {
                    case null:
                        Session.SendPacket(UserInterfaceHelper.GenerateDialog(
                            $"#b_i^{(byte)bIPacket.InventoryType}^{bIPacket.Slot}^1 #b_i^0^0^5 {Language.Instance.GetMessageFromKey("ASK_TO_DELETE")}"));
                        break;

                    case 1:
                        Session.SendPacket(UserInterfaceHelper.GenerateDialog(
                            $"#b_i^{(byte)bIPacket.InventoryType}^{bIPacket.Slot}^2 #b_i^{(byte)bIPacket.InventoryType}^{bIPacket.Slot}^5 {Language.Instance.GetMessageFromKey("SURE_TO_DELETE")}"));
                        break;

                    case 2:
                        if (Session.Character.InExchangeOrTrade || bIPacket.InventoryType == InventoryType.Bazaar)
                        {
                            return;
                        }

                        ItemInstance delInstance =
                            Session.Character.Inventory.LoadBySlotAndType(bIPacket.Slot, bIPacket.InventoryType);
                        Session.Character.DeleteItem(bIPacket.InventoryType, bIPacket.Slot);

                        if (delInstance != null)
                        {
                            Logger.Log.LogUserEvent("ITEM_DELETE", Session.GenerateIdentity(),
                                $"[DeleteItem]IIId: {delInstance.Id} ItemVNum: {delInstance.ItemVNum} Amount: {delInstance.Amount} MapId: {Session.CurrentMapInstance?.Map.MapId} MapX: {Session.Character.PositionX} MapY: {Session.Character.PositionY}");
                        }

                        break;
                }
            }
        }
    }
}
