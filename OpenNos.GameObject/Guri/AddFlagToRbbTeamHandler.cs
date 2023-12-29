using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.RainbowBattle;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.AddFlagToRbbTeam)]
    public class AddFlagToRbbTeamHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public AddFlagToRbbTeamHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            MapNpc npc = Session.CurrentMapInstance.Npcs.Find(s => s.MapNpcId == packet.User);
            if (npc == null)
            {
                //packet hacking
                return;
            }

            int dist = Map.GetDistance(
                new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY },
                new MapCell { X = npc.MapX, Y = npc.MapY });
            if (dist > 5)
            {
                return;
            }

            var RainbowTeam = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));

            if (RainbowTeam == null)
            {
                return;
            }

            if (RainbowBattleManager.AlreadyHaveFlag(RainbowTeam, (RainbowNpcType)packet.Argument, (int)packet.User))
            {
                return;
            }

            if (Session.Character.NoMove)
            {
                return;
            }

            if (Session.Character.HasBuff(85))
            {
                Session.Character.RemoveBuff(85);
            }

            if (Session.Character.HasBuff(559))
            {
                Session.Character.RemoveBuff(559);
            }

            if (Session.Character.HasBuff(560))
            {
                Session.Character.RemoveBuff(560);
            }

            RainbowBattleManager.AddFlag(Session, RainbowTeam, (RainbowNpcType)packet.Argument, (int)packet.User);
        }
    }
}
