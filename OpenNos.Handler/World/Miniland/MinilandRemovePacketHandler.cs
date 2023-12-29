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
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Miniland
{
    public class MinilandRemovePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public MinilandRemovePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// rmvobj packet
        /// </summary>
        /// <param name="packet"></param>
        public void MinilandRemoveObject(RmvobjPacket packet)
        {
            ItemInstance minilandobject =
                Session.Character.Inventory.LoadBySlotAndType(packet.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (Session.Character.MinilandState == MinilandState.Lock)
                {
                    MinilandObject minilandObject =
                        Session.Character.MinilandObjects.Find(s => s.ItemInstanceId == minilandobject.Id);
                    if (minilandObject != null)
                    {
                        Session.Character.MinilandObjects.Remove(minilandObject);
                        Session.SendPacket(minilandObject.GenerateMinilandEffect(true));
                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
                        Session.SendPacket(minilandObject.GenerateMinilandObject(true));
                    }
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"), 0));
                }
            }
        }
    }
}
