using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.Emoticon)]
    public class EmoticonHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public EmoticonHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {

            if (packet.Data.HasValue && packet.Type == 10 && packet.Data.Value == 1000 && !Session.Character.EmoticonsBlocked)
            {
                if (packet.User == Session.Character.CharacterId)
                {
                    Session.CurrentMapInstance?.Broadcast(Session, StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 5116), ReceiverType.AllNoEmoBlocked);
                }

                return;
            }
            
            
            if (!packet.Data.HasValue || packet.Type != 10 || packet.Data.Value < 973 || packet.Data.Value > 999 ||
                Session.Character.EmoticonsBlocked)
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (packet.User == Session.Character.CharacterId)
            {
                Session.CurrentMapInstance?.Broadcast(Session,
                    StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId,
                        packet.Data.Value + 4099), ReceiverType.AllNoEmoBlocked);
            }
            else if (int.TryParse(packet.User.ToString(), out int mateTransportId))
            {
                Mate mate = Session.Character.Mates.Find(s => s.MateTransportId == mateTransportId);
                if (mate != null)
                {
                    Session.CurrentMapInstance?.Broadcast(Session,
                        StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId,
                            packet.Data.Value + 4099), ReceiverType.AllNoEmoBlocked);
                }
            }
        }
    }
}
