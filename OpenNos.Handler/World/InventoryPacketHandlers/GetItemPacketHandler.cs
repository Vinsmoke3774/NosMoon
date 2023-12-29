using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Linq;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class GetItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public GetItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// get packet
        /// </summary>
        /// <param name="getPacket"></param>
        public void GetItem(GetPacket getPacket)
        {
            if (Session.Character.LastSkillUse.AddSeconds(1) > DateTime.Now
                || (Session.Character.IsVehicled
                 && Session.CurrentMapInstance?.MapInstanceType != MapInstanceType.EventGameInstance)
                || Session.Character.IsSeal
                || (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance && Session.CurrentMapInstance.InstanceBag.EndState != 0))
            {
                return;
            }

            if (getPacket.TransportId < 100000)
            {
                MapButton button = Session.CurrentMapInstance.Buttons.Find(s => s.MapButtonId == getPacket.TransportId);
                if (button != null)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(2000, 1, $"#git^{button.MapButtonId}"));
                }
            }
            else
            {
                if (!Session.CurrentMapInstance.DroppedList.ContainsKey(getPacket.TransportId))
                {
                    return;
                }

                MapItem mapItem = Session.CurrentMapInstance.DroppedList[getPacket.TransportId];

                if (mapItem != null)
                {
                    bool canpick = false;
                    switch (getPacket.PickerType)
                    {
                        case 1:
                            canpick = Session.Character.IsInRange(mapItem.PositionX, mapItem.PositionY, 8);
                            break;

                        case 2:
                            Mate mate = Session.Character.Mates.Find(s =>
                                s.MateTransportId == getPacket.PickerId && s.CanPickUp);
                            if (mate != null)
                            {
                                canpick = mate.IsInRange(mapItem.PositionX, mapItem.PositionY, 8);
                            }

                            break;
                    }

                    if (canpick && Session.HasCurrentMapInstance)
                    {
                        if (mapItem is MonsterMapItem item)
                        {
                            MonsterMapItem monsterMapItem = item;
                            if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance
                                && monsterMapItem.OwnerId.HasValue && monsterMapItem.OwnerId.Value != -1)
                            {
                                Group group = ServerManager.Instance.Groups.Find(g =>
                                    g.IsMemberOfGroup(monsterMapItem.OwnerId.Value)
                                    && g.IsMemberOfGroup(Session.Character.CharacterId));
                                if (item.CreatedDate.AddSeconds(30) > DateTime.Now
                                    && !(monsterMapItem.OwnerId == Session.Character.CharacterId
                                      || (group?.SharingMode == (byte)GroupSharingType.Everyone)))
                                {
                                    Session.SendPacket(
                                        Session.Character.GenerateSay(
                                            Language.Instance.GetMessageFromKey("NOT_YOUR_ITEM"), 10));
                                    return;
                                }
                            }

                            // initialize and rarify
                            item.Rarify(null);
                        }

                        if (mapItem.ItemVNum == 1097)
                        {
                            if (Session.CurrentMapInstance != null)
                            {
                                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
                                {
                                    Session.Character.Timespace.InstanceBag.Point += new Random().Next(3, 7);
                                    Session.SendPacket(Session.Character.Timespace.InstanceBag.GenerateScore());
                                }
                            }
                        }

                        if (mapItem.ItemVNum != 1046)
                        {
                            ItemInstance mapItemInstance = mapItem.GetItemInstance();

                            if (mapItemInstance?.Item == null)
                            {
                                return;
                            }

                            if (mapItemInstance.Item.ItemType == ItemType.Map || mapItem.IsQuest)
                            {
                                if (mapItem is MonsterMapItem)
                                {
                                    Session.Character.IncrementQuests(QuestType.Collect1, mapItem.ItemVNum);
                                    if (mapItem.IsQuest)
                                    {
                                        Session.Character.IncrementQuests(QuestType.Collect2, mapItem.ItemVNum);
                                        Session.Character.IncrementQuests(QuestType.Collect4, mapItem.ItemVNum);
                                    }
                                }
                                if (mapItemInstance.Item.Effect == 71)
                                {
                                    Session.Character.SpPoint += mapItem.GetItemInstance().Item.EffectValue;
                                    if (Session.Character.SpPoint > 10000)
                                    {
                                        Session.Character.SpPoint = 10000;
                                    }

                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"),
                                            mapItem.GetItemInstance().Item.EffectValue), 0));
                                    Session.SendPacket(Session.Character.GenerateSpPoint());
                                }
                                // Hardcode is best
                                if (mapItem.ItemVNum == 1086)
                                {
                                    ItemInstance inv = Session.Character.Inventory.AddToInventory(mapItemInstance).FirstOrDefault();
                                }

                                Session.CurrentMapInstance.DroppedList.TryRemove(getPacket.TransportId, out _);

                                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateGet(getPacket.PickerType, getPacket.PickerId, getPacket.TransportId));

                                if (getPacket.PickerType == 2)
                                {
                                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, getPacket.PickerId, 5004));
                                }
                            }
                            else
                            {
                                long characterDropperId = 0;
                                if (mapItemInstance.CharacterId > 0)
                                {
                                    characterDropperId = mapItemInstance.CharacterId;
                                }
                                short amount = mapItem.Amount;

                                if (amount <= 0) // Possible Dup ( If you spam X With another player )
                                {
                                    return;
                                }

                                ItemInstance inv = Session.Character.Inventory.AddToInventory(mapItemInstance)
                                    .FirstOrDefault();
                                if (inv != null)
                                {
                                    Session.CurrentMapInstance.DroppedList.TryRemove(getPacket.TransportId, out _);

                                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateGet(getPacket.PickerType, getPacket.PickerId, getPacket.TransportId));

                                    if (getPacket.PickerType == 2)
                                    {
                                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, getPacket.PickerId, 5004));
                                        Session.SendPacket(Session.Character.GenerateIcon(1, 1, inv.ItemVNum));
                                    }

                                    Session.SendPacket(Session.Character.GenerateSay(
                                        $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {inv.Item.Name} x {amount}",
                                        12));
                                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.LodInstance)
                                    {
                                        Session.CurrentMapInstance?.Broadcast(
                                            Session.Character.GenerateSay(
                                                $"{string.Format(Language.Instance.GetMessageFromKey("ITEM_ACQUIRED_LOD"), Session.Character.Name)}: {inv.Item.Name} x {mapItem.Amount}",
                                                10));
                                    }

                                    Logger.Log.LogUserEvent("CHARACTER_ITEM_GET", Session.GenerateIdentity(),
                                        $"[GetItem]IIId: {inv.Id} ItemVNum: {inv.ItemVNum} Amount: {amount}");
                                }
                                else
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                        else
                        {
                            // handle gold drop
                            long maxGold = ServerManager.Instance.Configuration.MaxGold;

                            double multiplier = 1 + (Session.Character.GetBuff(BCardType.CardType.Item, (byte)AdditionalTypes.Item.IncreaseEarnedGold)[0] / 100D);
                            //multiplier += (Session.Character.ShellEffectMain.FirstOrDefault(s => s.Effect == (byte)ShellWeaponEffectType.GainMoreGold)?.Value ?? 0) / 100D;

                            if (mapItem is MonsterMapItem droppedGold
                                && Session.Character.Gold + (droppedGold.GoldAmount * multiplier) <= maxGold)
                            {
                                if (getPacket.PickerType == 2)
                                {
                                    Session.SendPacket(Session.Character.GenerateIcon(1, 1, 1046));
                                }

                                Session.Character.Gold += (int)(droppedGold.GoldAmount * multiplier);

                                Logger.Log.LogUserEvent("CHARACTER_ITEM_GET", Session.GenerateIdentity(), $"[GetItem]Gold: {(int)(droppedGold.GoldAmount * multiplier)})");

                                Session.SendPacket(Session.Character.GenerateSay(
                                    $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {mapItem.GetItemInstance().Item.Name} x {droppedGold.GoldAmount}{(multiplier > 1 ? $" + {(int)(droppedGold.GoldAmount * multiplier) - droppedGold.GoldAmount}" : "")}",
                                    12));
                            }
                            else
                            {
                                Session.Character.Gold = maxGold;
                                Logger.Log.LogUserEvent("CHARACTER_ITEM_GET", Session.GenerateIdentity(), "[MaxGold]");
                                Session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"),
                                        0));
                            }

                            Session.SendPacket(Session.Character.GenerateGold());
                            Session.CurrentMapInstance.DroppedList.TryRemove(getPacket.TransportId, out _);

                            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateGet(getPacket.PickerType, getPacket.PickerId, getPacket.TransportId));

                            if (getPacket.PickerType == 2)
                            {
                                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, getPacket.PickerId, 5004));
                            }
                        }
                    }
                }
            }
        }
    }
}
