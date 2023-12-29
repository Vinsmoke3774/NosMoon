using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.UserInterface
{
    [NRunHandler(NRunType.GetSavingsBook)]
    public class GetSavingBooksHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetSavingBooksHandler(ClientSession session) : base(session)
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
            if (packet.Type == 0 && packet.Value == 2)
            {
                int item = Session.Character.Inventory.CountItem(5836);
                if (item == 0)
                {
                    Item iteminfo = ServerManager.GetItem(5836);
                    ItemInstance inv = Session.Character.Inventory.AddNewToInventory(5836).FirstOrDefault();
                    Session.SendPacket(inv != null ? "info Cuarry Savings Book received" : UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                }
                else
                {
                    Session.SendPacket("info You already have a Savings Book.");
                }
            }
        }
    }
}
