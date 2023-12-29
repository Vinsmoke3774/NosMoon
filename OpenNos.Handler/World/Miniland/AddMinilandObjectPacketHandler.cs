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
    public class AddMinilandObjectPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public AddMinilandObjectPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// addobj packet
        /// </summary>
        /// <param name="addObjPacket"></param>
        public void MinilandAddObject(AddObjPacket addObjPacket)
        {
            ItemInstance minilandobject =
                Session.Character.Inventory.LoadBySlotAndType(addObjPacket.Slot, InventoryType.Miniland);
            if (minilandobject != null)
            {
                if (Session.Character.MinilandObjects.All(s => s.ItemInstanceId != minilandobject.Id))
                {
                    if (Session.Character.MinilandState == MinilandState.Lock)
                    {
                        MinilandObject minilandobj = new MinilandObject
                        {
                            CharacterId = Session.Character.CharacterId,
                            ItemInstance = minilandobject,
                            ItemInstanceId = minilandobject.Id,
                            MapX = addObjPacket.PositionX,
                            MapY = addObjPacket.PositionY,
                            Level1BoxAmount = 0,
                            Level2BoxAmount = 0,
                            Level3BoxAmount = 0,
                            Level4BoxAmount = 0,
                            Level5BoxAmount = 0
                        };

                        if (minilandobject.Item.ItemType == ItemType.House)
                        {
                            switch (minilandobject.Item.ItemSubType)
                            {
                                case 2:
                                    minilandobj.MapX = 31;
                                    minilandobj.MapY = 3;
                                    break;

                                case 0:
                                    minilandobj.MapX = 24;
                                    minilandobj.MapY = 7;
                                    break;

                                case 1:
                                    minilandobj.MapX = 21;
                                    minilandobj.MapY = 4;
                                    break;
                            }

                            MinilandObject min = Session.Character.MinilandObjects.Find(s =>
                                s.ItemInstance.Item.ItemType == ItemType.House && s.ItemInstance.Item.ItemSubType
                                == minilandobject.Item.ItemSubType);
                            if (min != null)
                            {
                                new MinilandRemovePacketHandler(Session).MinilandRemoveObject(new RmvobjPacket { Slot = min.ItemInstance.Slot });
                            }
                        }

                        Session.Character.MinilandObjects.Add(minilandobj);
                        Session.SendPacket(minilandobj.GenerateMinilandEffect(false));
                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
                        Session.SendPacket(minilandobj.GenerateMinilandObject(false));
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MINILAND_NEED_LOCK"),
                                0));
                    }
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(
                            Language.Instance.GetMessageFromKey("ALREADY_THIS_MINILANDOBJECT"), 0));
                }
            }
        }
    }
}
