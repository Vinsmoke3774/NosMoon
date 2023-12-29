using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Event.ACT7;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.TeleportActVII2)]
    public class TeleportActVII2Handler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TeleportActVII2Handler(ClientSession session) : base(session)
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
            return;
            if (Session.Character.QuestLogs.All(s => s.QuestId != 6500))
            {
                Session.SendPacket("info Cause of the arrival (Main Quest) wasn't accepted.");
                return;
            }

            if (Session.Character.Gold < 25000)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                return;
            }

            Session.GoldLess(25000);
            ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, ServerManager.Instance.Act7Ship.MapInstanceId, 5, 32);
            Act7Ship.Run(Session);
        }
    }
}
