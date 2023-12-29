using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToGrenigasTemple)]
    public class GoToGrenigasTempleHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToGrenigasTempleHandler(ClientSession session) : base(session)
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
                if (Session.Character.Inventory.CountItem(5919) < 1)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 2536, 26, 31);
                Session.Character.Inventory.RemoveItemAmount(5919);
            }
        }
    }
}
