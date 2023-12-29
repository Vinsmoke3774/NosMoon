using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.SetPlaceOfRevival)]
    public class SetPlaceOfRevivalHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public SetPlaceOfRevivalHandler(ClientSession session) : base(session)
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
            if (packet.Value == 2)
            {
                Session.SendPacket($"qna #n_run^15^1^1^{Npc.MapNpcId} {Language.Instance.GetMessageFromKey("ASK_CHANGE_SPAWNLOCATION")}");
            }
            else
            {
                switch (Npc.MapId)
                {
                    case 1:
                        Session.Character.SetRespawnPoint(1, 79, 116);
                        break;

                    case 20:
                        Session.Character.SetRespawnPoint(20, 9, 92);
                        break;

                    case 145:
                        Session.Character.SetRespawnPoint(145, 13, 110);
                        break;

                    case 170:
                        Session.Character.SetRespawnPoint(170, 79, 47);
                        break;

                    case 177:
                        Session.Character.SetRespawnPoint(177, 149, 74);
                        break;

                    case 189:
                        Session.Character.SetRespawnPoint(189, 58, 166);
                        break;
                }
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("RESPAWNLOCATION_CHANGED"), 0));
            }
        }
    }
}
