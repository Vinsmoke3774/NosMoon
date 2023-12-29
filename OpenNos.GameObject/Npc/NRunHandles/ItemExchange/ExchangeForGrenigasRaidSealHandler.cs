using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.ItemExchange
{
    [NRunHandler(NRunType.ExchangeForGrenigasRaidSeal)]
    public class ExchangeForGrenigasRaidSealHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeForGrenigasRaidSealHandler(ClientSession session) : base(session)
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
            if (packet.Type == 0)
            {
                Session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("ASK_TRADE")}");
            }
            else
            {
                if (Session.Character.Inventory.CountItem(5917) < 1 || Session.Character.Inventory.CountItem(5918) < 1)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                Session.Character.GiftAdd(5922, 1);
                Session.Character.Inventory.RemoveItemAmount(5917);
                Session.Character.Inventory.RemoveItemAmount(5918);
            }
        }
    }
}
