using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Mates
{
    public class PetMovePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PetMovePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// ptctl packet
        /// </summary>
        /// <param name="ptCtlPacket"></param>
        public void PetMove(PtCtlPacket ptCtlPacket)
        {
            if (ptCtlPacket.PacketEnd == null || ptCtlPacket.Amount < 1)
            {
                return;
            }
            string[] packetsplit = ptCtlPacket.PacketEnd.Split(' ');
            for (int i = 0; i < ptCtlPacket.Amount * 3; i += 3)
            {
                if (packetsplit.Length >= ptCtlPacket.Amount * 3 && int.TryParse(packetsplit[i], out int petId)
                                                                 && short.TryParse(packetsplit[i + 1],
                                                                     out short positionX)
                                                                 && short.TryParse(packetsplit[i + 2],
                                                                     out short positionY))
                {
                    Mate mate = Session.Character.Mates.Find(s => s.MateTransportId == petId);
                    if (mate != null && mate.IsAlive && !mate.HasBuff(BCardType.CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible) && mate.Owner.Session.HasCurrentMapInstance && !mate.Owner.IsChangingMapInstance
                        && Session.CurrentMapInstance?.Map?.IsBlockedZone(positionX, positionY) == false)
                    {
                        if (mate.Loyalty > 0)
                        {
                            mate.PositionX = positionX;
                            mate.PositionY = positionY;
                            if (Session.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                            {
                                mate.MapX = positionX;
                                mate.MapY = positionY;
                            }
                            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.Move(UserType.Npc, petId, positionX,
                                positionY, mate.Monster.Speed));

                            Session.CurrentMapInstance?.OnMoveOnMapEvents?.ForEach(e => EventHelper.Instance.RunEvent(e));
                            Session.CurrentMapInstance?.OnMoveOnMapEvents?.RemoveAll(s => s != null);
                        }
                        Session.SendPacket(mate.GenerateCond());
                    }
                }
            }
        }
    }
}
