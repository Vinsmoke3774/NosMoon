using System;
using System.Linq;
using System.Reactive.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Npc
{
    public class PdtsePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PdtsePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// pdtse packet
        /// </summary>
        /// <param name="pdtsePacket"></param>
        public void Pdtse(PdtsePacket pdtsePacket)
        {
            try
            {
                Session.Character.LastPdtseRequests++;
                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }

                if (Session.Character.LastPdtseRequests > 20)
                {
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = Session.Account.AccountId,
                        Reason = "Auto Ban PdtseRequest Infinite Abuse PL",
                        Penalty = PenaltyType.Banned,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddYears(1),
                        AdminName = "NosMoon System"
                    };
                    Character.InsertOrUpdatePenalty(log);
                    Session?.Disconnect();
                    return;
                }
                Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(x =>
                {
                    if (Session?.Character?.LastPdtseRequests > 0)
                    {
                        Session.Character.LastPdtseRequests = 0;
                    }
                });
                short vNum = pdtsePacket.VNum;
                Recipe recipe = ServerManager.Instance.GetAllRecipes().Find(s => s.ItemVNum == vNum);
                if (Session.CurrentMapInstance.GetNpc(Session.Character.LastNpcMonsterId)?.Recipes.Find(s => s.ItemVNum == vNum) is Recipe recipeNpc)
                {
                    recipe = recipeNpc;
                }
                if (ServerManager.Instance.GetRecipesByItemVNum(Session.Character.LastItemVNum)?.Find(s => s.ItemVNum == vNum) is Recipe recipeScroll)
                {
                    recipe = recipeScroll;
                }
                if (pdtsePacket.Type == 1)
                {
                    if (recipe?.Amount > 0)
                    {
                        string recipePacket = $"m_list 3 {recipe.Amount}";
                        foreach (RecipeItemDTO ite in recipe.Items.Where(s =>
                            s.ItemVNum != Session.Character.LastItemVNum || Session.Character.LastItemVNum == 0))
                        {
                            if (ite.Amount > 0)
                            {
                                recipePacket += $" {ite.ItemVNum} {ite.Amount}";
                            }
                        }

                        recipePacket += " -1";
                        Session.SendPacket(recipePacket);
                    }
                }
                else if (recipe != null)
                {
                    // sequential
                    //pdtse 0 4955 0 0 1
                    // random
                    //pdtse 0 4955 0 0 2
                    if (recipe.Items.Count < 1 || recipe.Amount <= 0 || recipe.Items.Any(ite =>
                            Session.Character.Inventory.CountItem(ite.ItemVNum) < ite.Amount))
                    {
                        return;
                    }

                    if (Session.Character.LastItemVNum != 0)
                    {
                        if (!ServerManager.Instance.ItemHasRecipe(Session.Character.LastItemVNum))
                        {
                            return;
                        }
                        short npcVNum = 0;
                        switch (Session.Character.LastItemVNum)
                        {
                            case 1375:
                                npcVNum = 956;
                                break;

                            case 1376:
                                npcVNum = 957;
                                break;

                            case 1437:
                                npcVNum = 959;
                                break;
                        }
                        if (npcVNum != 0 && ((Session.Character.BattleEntity.IsCampfire(npcVNum) && Session.CurrentMapInstance.GetNpc(Session.Character.LastNpcMonsterId) == null) || Session.CurrentMapInstance.GetNpc(Session.Character.LastNpcMonsterId)?.NpcVNum != npcVNum))
                        {
                            return;
                        }

                        Session.Character.LastItemVNum = 0;
                    }
                    else if (!ServerManager.Instance.MapNpcHasRecipe(Session.Character.LastNpcMonsterId))
                    {
                        return;
                    }

                    switch (recipe.ItemVNum)
                    {
                        case 2802:
                            if (Session.Character.Inventory.CountItem(recipe.ItemVNum) >= 5)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_HAVE_MAX_AMOUNT"), 0));
                                return;
                            }
                            break;

                        case 5935:
                            if (Session.Character.Inventory.CountItem(recipe.ItemVNum) >= 3)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_HAVE_MAX_AMOUNT"), 0));
                                return;
                            }
                            break;

                        case 5936:
                        case 5937:
                        case 5938:
                        case 5939:
                            if (Session.Character.Inventory.CountItem(recipe.ItemVNum) >= 10)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_HAVE_MAX_AMOUNT"), 0));
                                return;
                            }
                            break;

                        case 5940:
                            if (Session.Character.Inventory.CountItem(recipe.ItemVNum) >= 4)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_HAVE_MAX_AMOUNT"), 0));
                                return;
                            }
                            break;

                        case 5942:
                        case 5943:
                        case 5944:
                        case 5945:
                        case 5946:
                            if (Session.Character.Inventory.CountItem(recipe.ItemVNum) >= 1)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_HAVE_MAX_AMOUNT"), 0));
                                return;
                            }
                            break;
                    }

                    Item item = ServerManager.GetItem(recipe.ItemVNum);
                    if (item == null)
                    {
                        return;
                    }

                    if(item.IsFamily)
                    {
                        var family = (FamilyDTO)Session.Character.Family;
                        short famRank = (short)Session.Character.FamilyCharacter.Authority;

                        if (famRank > item.FamilyRank)
                        {
                            Session?.SendPacket(UserInterfaceHelper.GenerateSay(Language.Instance.GetMessageFromKey("RANK_TOO_LOW"), 11));
                            Session?.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("RANK_TOO_LOW"), 0));
                            return;
                        }

                        if (item.FamilyLevel > family.FamilyLevel)
                        {
                            Session?.SendPacket(UserInterfaceHelper.GenerateSay(Language.Instance.GetMessageFromKey("FAMILY_LEVEL_TOO_LOW"), 11));
                            Session?.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAMILY_LEVEL_TOO_LOW"), 0));
                            return;
                        }


                    }

                    sbyte rare = 0;
                    if (item.EquipmentSlot == EquipmentType.Armor || item.EquipmentSlot == EquipmentType.MainWeapon || item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                    {
                        byte ra = (byte)ServerManager.RandomNumber();

                        for (int i = 0; i < ItemHelper.BuyCraftRareRate.Length; i++)
                        {
                            if (ra <= ItemHelper.BuyCraftRareRate[i])
                            {
                                rare = (sbyte)i;
                            }
                        }
                    }

                    if (item.VNum == 20055)
                    {
                        if (ServerManager.RandomNumber(0, 3) < 2)
                        {
                            rare = 7;
                        }
                        else
                        {
                            rare = 8;
                        }
                    }

                    // TODO : Return if it's a timespace craft

                    var recipeTS = DAOFactory.RecipeDAO.LoadByItemVNum(item.VNum);
                    var getRecipeTS = DAOFactory.RecipeItemDAO.LoadByRecipe(recipe.RecipeId)?.FirstOrDefault();
                    if (Session.Character.Inventory.CountItem(getRecipeTS.ItemVNum) < 1) return; // TODO : Improve this check using timespace's craft id

                    var energyField = Session.Character.EnergyFields.FirstOrDefault(s => s.MapId == Session.Character.MapId);
                    if (Session.Character.CanCreateTimeSpace && (energyField != null && Session.Character.IsInRange(energyField.MapX, energyField.MapY, 5)))
                    {
                        var script = ServerManager.Instance.TimeSpaces.FirstOrDefault(s => s.PositionX == item.VNum);
                        GameObject.ScriptedInstance instance = new()
                        {
                            CharacterId = Session.Character.CharacterId,
                            MapId = Session.Character.MapId,
                            PositionX = Session.Character.PositionX,
                            PositionY = Session.Character.PositionY,
                            Script = script.Script,
                            Type = ScriptedInstanceType.HiddenTsRaid
                        };

                        instance.LoadGlobals();
                        ServerManager.Instance.TimeSpaces.Add(instance); // Add the ts into the timespace list in order to be able to access it
                        Session.Character.Inventory.RemoveItemAmount(getRecipeTS.ItemVNum);
                        Session.CurrentMapInstance.Broadcast(Session, $"in 2 930 {instance.ScriptedInstanceId} {Session.Character.MapX} {Session.Character.MapY} 2 100 100 10002 0 0 -1 1 0 -1 @ 0 -1 0 0 0 0 0 0 0 0 647 {Session.Character.Name} 0");
                        Session.CurrentMapInstance.Broadcast(Session, $"eff_g  822 {instance.ScriptedInstanceId} {Session.Character.MapX} {Session.Character.MapY} 0");
                        Session.SendPacket("msgi 0 623 0 0 0 0 0");
                        Session.SendPacket("shop_end 0");
                        Session.SendPacket("pdtclose");
                        return;
                    }

                    ItemInstance oldShell = Session.Character.Inventory.LoadBySlotAndType(pdtsePacket?.EquipmentSlot, InventoryType.Equipment);
                    ItemInstance inv = Session.Character.Inventory.AddNewToInventory(recipe.ItemVNum, recipe.Amount, rare: rare).FirstOrDefault();
                    if (inv != null)
                    {
                        if (inv.Item.EquipmentSlot == EquipmentType.Armor
                            || inv.Item.EquipmentSlot == EquipmentType.MainWeapon
                            || inv.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                        {

                            #region keep shells 

                            if (inv.Item.IsHeroic && pdtsePacket.Option == 1)
                            {
                                if (inv == null)
                                {
                                    Session.SendPacket("shop_end 0");
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                    return;
                                }
                                foreach (ShellEffectDTO oldshells in oldShell.ShellEffects)
                                {

                                    inv.ShellEffects.Add(new ShellEffectDTO
                                    {
                                        Effect = oldshells.Effect,
                                        EffectLevel = oldshells.EffectLevel,
                                        Value = oldshells.Value,
                                    });
                                }
                                foreach (RecipeItemDTO ite in recipe.Items)
                                {
                                    if (oldShell != null && oldShell.Item.IsHeroic)
                                    {
                                        Session.Character.Inventory.RemoveItemFromInventory(oldShell.Id, ite.Amount);
                                    }
                                    Session.Character.Inventory.RemoveItemAmount(ite.ItemVNum, ite.Amount);

                                }
                                inv.BoundCharacterId = Session?.Character?.CharacterId;
                                inv.SetRarityPoint();
                                Session.SendPacket($"pdti 11 {inv.ItemVNum} {recipe.Amount} 29 {inv.Upgrade} {inv.Rare}");
                                Session.SendPacket(UserInterfaceHelper.GenerateGuri(19, 1, Session.Character.CharacterId, 1324));
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), inv.Item.Name, recipe.Amount), 0));
                                Session.Character.IncrementQuests(QuestType.Product, inv.ItemVNum, recipe.Amount);
                                return;

                            }
                            #endregion

                            #region random shell

                            if (inv.Item.IsHeroic && pdtsePacket.Option == 2)
                            {
                                if (inv == null)
                                {
                                    Session.SendPacket("shop_end 0");
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                    return;
                                }
                                foreach (RecipeItemDTO ite in recipe.Items)
                                {
                                    if (oldShell != null && oldShell.Item.IsHeroic)
                                    {
                                        Session.Character.Inventory.RemoveItemFromInventory(oldShell.Id, ite.Amount);
                                    }
                                    Session.Character.Inventory.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                                }
                                inv.GenerateHeroicShell(Session, RarifyProtection.None, true);
                                inv.BoundCharacterId = Session?.Character?.CharacterId;
                                inv.SetRarityPoint();
                                Session.SendPacket($"pdti 11 {inv.ItemVNum} {recipe.Amount} 29 {inv.Upgrade} {inv.Rare}");
                                Session.SendPacket(UserInterfaceHelper.GenerateGuri(19, 1, Session.Character.CharacterId, 1324));
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), inv.Item.Name, recipe.Amount), 0));
                                Session.Character.IncrementQuests(QuestType.Product, inv.ItemVNum, recipe.Amount);
                                return;
                            }
                            #endregion

                        }

                        foreach (RecipeItemDTO ite in recipe.Items)
                        {
                            if (oldShell != null && oldShell.Item.IsHeroic)
                            {
                                Session.Character.Inventory.RemoveItemFromInventory(oldShell.Id, ite.Amount);
                            }

                            Session.Character.Inventory.RemoveItemAmount(ite.ItemVNum, ite.Amount);
                            if (inv.Item.IsFamily)
                            {
                                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                                {
                                    DestinationCharacterId = Session.Character.Family.FamilyId,
                                    SourceCharacterId = Session.Character.CharacterId,
                                    SourceWorldId = ServerManager.Instance.WorldId,
                                    Message = UserInterfaceHelper.GenerateMsg($"{Session.Character.Name} just spent {ite.Amount} Family Coins to purchase {inv.Item.Name}!", 0),
                                    Type = MessageType.Family
                                });
                            }
                        }

                        // pdti {WindowType} {inv.ItemVNum} {recipe.Amount} {Unknown} {inv.Upgrade} {inv.Rare}
                        Session.SendPacket($"pdti 11 {inv.ItemVNum} {recipe.Amount} 29 {inv.Upgrade} {inv.Rare}");
                        Session.SendPacket(UserInterfaceHelper.GenerateGuri(19, 1, Session.Character.CharacterId, 1324));
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("CRAFTED_OBJECT"), inv.Item.Name, recipe.Amount), 0));
                        Session.Character.IncrementQuests(QuestType.Product, inv.ItemVNum, recipe.Amount);
                    }
                    else
                    {
                        Session.SendPacket("shop_end 0");
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));

                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }
    }
}
