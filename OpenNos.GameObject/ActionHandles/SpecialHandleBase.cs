using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.ActionHandles
{
    public class SpecialHandlerBase
    {
        public SpecialHandlerBase(ClientSession session)
        {
            Session = session;
        }

        internal ClientSession Session { get; }

        internal MapNpc Npc { get; set; }

        internal TeleporterDTO Tp { get; set; }

        internal ClientSession TargetSession { get; set; }

        internal ItemInstance ItemInstance { get; set; }

        internal bool InitializeNpc(NRunPacket packet)
        {
            Npc = Session.CurrentMapInstance.Npcs.Find(s => s.MapNpcId == packet.NpcId);

            if (Npc == null)
            {
                return false;
            }

            return true;
        }

        internal bool InitializeTeleporter(NRunPacket packet)
        {
            Npc = Session.CurrentMapInstance.Npcs.Find(s => s.MapNpcId == packet.NpcId);

            Tp = Npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);

            if (Npc == null || Tp == null)
            {
                return false;
            }

            return true;
        }

        internal void TradeItems(short itemToTrade, short itemAmount, short itemToReceive, short itemToReceiveAmount)
        {
            if (Session.Character.Inventory.CountItem(itemToTrade) < itemAmount)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                return;
            }

            Session.Character.GiftAdd(itemToReceive, itemToReceiveAmount);
            Session.Character.Inventory.RemoveItemAmount(itemToTrade, itemAmount);
        }
    }

}
