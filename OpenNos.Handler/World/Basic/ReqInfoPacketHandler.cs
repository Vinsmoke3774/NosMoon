using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.Handler.World.Basic
{
    public class ReqInfoPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ReqInfoPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// req_info packet
        /// </summary>
        /// <param name="reqInfoPacket"></param>
        public void ReqInfo(ReqInfoPacket reqInfoPacket)
        {
            if (Session.Character == null)
            {
                return;
            }

            switch (reqInfoPacket.Type)
            {
                case 6:
                {
                    if (reqInfoPacket.MateVNum.HasValue)
                    {
                        Mate mate = Session.CurrentMapInstance?.Sessions?.FirstOrDefault(s => s?.Character?.Mates != null && s.Character.Mates.Any(o => o.MateTransportId == reqInfoPacket.MateVNum.Value))?
                            .Character.Mates.ToList().FirstOrDefault(m => m.MateTransportId == reqInfoPacket.MateVNum.Value);

                        if (mate != null)
                        {
                            Session.SendPacket(mate.GenerateEInfo());
                        }
                    }

                    break;
                }
                case 5:
                {
                    NpcMonster npc = ServerManager.GetNpcMonster((short)reqInfoPacket.TargetVNum);

                    if (Session.CurrentMapInstance?.GetMonsterById(Session.Character.LastNpcMonsterId)
                        is MapMonster monster && monster.Monster?.OriginalNpcMonsterVNum == reqInfoPacket.TargetVNum)
                    {
                        npc = ServerManager.GetNpcMonster(monster.Monster.NpcMonsterVNum);
                    }

                    if (npc != null)
                    {
                        Session.SendPacket(npc.GenerateEInfo());
                    }

                    break;
                }
                case 12:
                {
                    if (Session.Character.Inventory != null)
                    {
                        Session.SendPacket(Session.Character.Inventory.LoadBySlotAndType((short)reqInfoPacket.TargetVNum, InventoryType.Equipment)?.GenerateReqInfo());
                    }

                    break;
                }
                default:
                {
                    if (ServerManager.Instance.GetSessionByCharacterId(reqInfoPacket.TargetVNum)?.Character is Character character)
                    {
                        Session.SendPacket(character.GenerateReqInfo());
                    }

                    break;
                }
            }
        }
    }
}
