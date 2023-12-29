using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToLandOfDeath)]
    public class GoToLandOfDeathHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToLandOfDeathHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ENTER"), 0));
                return;
            }

            if (Session.Character.Level < 55)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOD_REQUIRE_LEVEL"), 0));
                return;
            }

            if (Session.Character?.Family == null)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FAMILY"), 0));
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (Session.Character.Family?.LandOfDeath == null)
            {
                Session.Character.Family.LandOfDeath = ServerManager.GenerateMapInstance(150, MapInstanceType.LodInstance, new InstanceBag());
            }

            if (Session.Character?.Family?.LandOfDeath != null)
            {
                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.Character.Family.LandOfDeath.MapInstanceId, 153, 145);
            }
        }
    }
}
