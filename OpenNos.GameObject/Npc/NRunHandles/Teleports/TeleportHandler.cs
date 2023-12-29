using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.Teleport)]
    public class TeleportHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TeleportHandler(ClientSession session) : base(session)
        {

        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeTeleporter(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (packet.Type >= 0 && Session.Character.Gold >= 1000 * packet.Type)
            {
                Session.Character.Gold -= 1000 * packet.Type;
                Session.SendPacket(Session.Character.GenerateGold());
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Tp.MapId, Tp.MapX, Tp.MapY);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
            }
        }
    }
}
