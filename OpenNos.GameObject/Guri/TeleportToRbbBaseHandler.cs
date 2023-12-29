using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.TeleportToRbbBase)]
    public class TeleportToRbbBaseHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public TeleportToRbbBaseHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            long targetId = packet.User;

            if (targetId == 0)
            {
                return;
            }

            ClientSession target = ServerManager.Instance.GetSessionByCharacterId(targetId);

            if (target?.Character?.MapInstance == null)
            {
                return;
            }

            var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(target));

            if (target.Character.MapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance)
            {
                target.Character.PositionX = rbb.TeamEntity == RainbowTeamBattleType.Red ? ServerManager.RandomNumber<short>(30, 32) : ServerManager.RandomNumber<short>(83, 85);
                target.Character.PositionY = rbb.TeamEntity == RainbowTeamBattleType.Red ? ServerManager.RandomNumber<short>(73, 76) : ServerManager.RandomNumber<short>(2, 4);
                target.CurrentMapInstance.Broadcast(target.Character.GenerateTp());
                target.Character.NoAttack = false;
                target.Character.NoMove = false;
                target?.SendPacket(target.Character.GenerateCond());
                target.Character.IsFrozen = false;
            }
        }
    }
}
