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
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.Miniland
{
    public class UseMinilandObjectPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UseMinilandObjectPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// useobj packet
        /// </summary>
        /// <param name="packet"></param>
        public void UseMinilandObject(UseobjPacket packet)
        {
            ClientSession client =
                ServerManager.Instance.Sessions.FirstOrDefault(s =>
                    s.Character?.Miniland == Session.Character.MapInstance);
            ItemInstance minilandObjectItem =
                client?.Character.Inventory.LoadBySlotAndType(packet.Slot, InventoryType.Miniland);
            if (minilandObjectItem != null)
            {

                MinilandObject minilandObject =
                    client.Character.MinilandObjects.Find(s => s.ItemInstanceId == minilandObjectItem.Id);
                if (minilandObject != null)
                {
                    if (!minilandObjectItem.Item.IsMinilandObject)
                    {
                        byte game = (byte)(minilandObject.ItemInstance.Item.EquipmentSlot == 0
                            ? 4 + (minilandObject.ItemInstance.ItemVNum % 10)
                            : (int)minilandObject.ItemInstance.Item.EquipmentSlot / 3);
                        const bool full = false;
                        Session.SendPacket(
                            $"mlo_info {(client == Session ? 1 : 0)} {minilandObjectItem.ItemVNum} {packet.Slot} {Session.Character.MinilandPoint} {(minilandObjectItem.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} 0 {SharedMinilandMethods.GetMinilandMaxPoint(game)[0]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[0] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[1]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[1] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[2]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[2] + 2} {SharedMinilandMethods.GetMinilandMaxPoint(game)[3]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[3] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[4]} {SharedMinilandMethods.GetMinilandMaxPoint(game)[4] + 1} {SharedMinilandMethods.GetMinilandMaxPoint(game)[5]}");
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateStashAll());
                    }
                }
            }
        }
    }
}
