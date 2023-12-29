using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NosByte.Packets.ClientPackets;
using NosByte.Packets.ServerPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Modules.Bazaar.Queries;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.GameObject.Extension.CharacterExtensions
{
    public static class CharacterPacketExtensions
    {
        public static string GenerateBpm(this Character character)
        {
            string quests = "";
            foreach (var quest in ServerManager.Instance.BattlePassQuests)
            {
                TimeSpan timeSpan = TimeSpan.Zero;
                switch (quest.TaskType)
                {
                    case BattlePassQuestType.Daily:
                        timeSpan = new DateTime(quest.Start.Year, quest.Start.Month, quest.Start.Day).AddDays(1) - DateTime.Now;
                        break;

                    case BattlePassQuestType.Weekly:
                        timeSpan = new DateTime(quest.Start.Year, quest.Start.Month, quest.Start.Day).AddDays(7) - DateTime.Now;
                        break;

                    case BattlePassQuestType.Season:
                        timeSpan = ServerManager.Instance.Configuration.EndSeason - DateTime.Now;
                        break;
                }
                var characterAdvencement = character.BattlePassQuestLogs.Find(s => s.QuestId == quest.Id);
                var actualAdvencement = characterAdvencement == null ? 0 : characterAdvencement.Advancement;
                bool alreadyTaken = characterAdvencement == null ? false : characterAdvencement.AlreadyTaken;
                bool isExpired = character.IsBattlePassQuestExpired(quest.Id);
                quests += $"{quest.Id} {(byte)quest.MissionSubType} {(byte)quest.TaskType} {(alreadyTaken ? - 1 : actualAdvencement)} {quest.MaxObjectiveValue} {quest.Reward} {(isExpired ? -1 : Math.Round(timeSpan.TotalMinutes))} ";
            }
            return $"bpm 70 {(ServerManager.Instance.Configuration.BattlePassIconEnabled ? 1 : 0)} {ServerManager.Instance.Configuration.MaxBattlePassPoints} 21093011 21111111 {quests.TrimEnd(' ')}";
        }

        public static string GenerateBpp(this Character character)
        {
            string items = "";
            foreach (var palier in ServerManager.Instance.BattlePassPaliers)
            {
                var bpItems = ServerManager.Instance.BattlePassItems.Where(s => s.Palier == palier.Id);
                if (!bpItems.Any()) continue;
                if (bpItems.Count() > 2) continue; // We're not supposed to have more than 2 items there but we never know
                var freeItem = bpItems.FirstOrDefault(s => !s.IsPremium);
                var premiumItem = bpItems.FirstOrDefault(s => s.IsPremium);
                bool freeItemAlreadyTaken = character.BattlePassItemLogs.Any(s => !s.IsPremium && s.Palier == palier.Id);
                bool canGetFreeItem = palier.MaximumBattlePassPoint <= character.BattlePassPoints;
                bool premiumItemAlreadyTaken = character.BattlePassItemLogs.Any(s => s.IsPremium && s.Palier == palier.Id);
                bool canGetPremiumItem = canGetFreeItem && character.HavePremiumBattlePass;
                items += $"{palier.Id} {freeItem.ItemVNum} {freeItem.Amount} {premiumItem.ItemVNum} {premiumItem.Amount} {(canGetFreeItem ? freeItemAlreadyTaken ? 2 : 1 : 0)} {(canGetPremiumItem ? premiumItemAlreadyTaken ? 2 : 1 : 0)} {(premiumItem.IsSuperReward ? 1 : 0)} ";
            }
            return $"bpp {ServerManager.Instance.BattlePassPaliers.Count} {character.BattlePassPoints} {(character.HavePremiumBattlePass ? 1 : 0)} {items.TrimEnd(' ')}";
        }

        public static string GenerateHidn(this Character character, short x, short y)
        {
            var angle = MathsExtensions.CalculateAngle(x, y, character.PositionX, character.PositionY);
            return $"hidn 0 {angle.ToString().Replace(',', '.')} {character.PositionX} {character.PositionY}";
        }

        public static string GenerateAct(this Character character) => "act6";

        public static string GenerateAct6(this Character character) =>
            $"act6 1 0 {ServerManager.Instance.Act6Zenas.Percentage / 100} " +
            $"{Convert.ToByte(ServerManager.Instance.Act6Zenas.Mode)} " +
            $"{ServerManager.Instance.Act6Zenas.CurrentTime} " +
            $"{ServerManager.Instance.Act6Zenas.TotalTime} " +
            $"{ServerManager.Instance.Act6Erenia.Percentage / 100} " +
            $"{Convert.ToByte(ServerManager.Instance.Act6Erenia.Mode)} " +
            $"{ServerManager.Instance.Act6Erenia.CurrentTime} " +
            $"{ServerManager.Instance.Act6Erenia.TotalTime}";

        public static string GenerateAdditionalHpMp(this Character character)
        {
            return $"guri 4 {Math.Round(character.BattleEntity.AdditionalHp)} {Math.Round(character.BattleEntity.AdditionalMp)}";
        }

        public static string GenerateAscr(this Character character, AscrPacketType e)
        {
            if (e == AscrPacketType.Close)
            {
                return "ascr 0 0 0 0 0 0 0 0 -1";
            }
            long killGroup = 0;
            long dieGroup = 0;
            var topArena = "0 0 0";
            var packet = $"{character.CurrentKill} {character.CurrentDie} {character.CurrentTc} {character.ArenaKill} {character.ArenaDie} {character.ArenaTc}";
            if (e == AscrPacketType.Group)
            {
                if (character.Group == null)
                {
                    return $"ascr {packet} {killGroup} {dieGroup} {(long)e}";
                }

                if (character.Group.GroupType != GroupType.Group)
                {
                    return $"ascr {packet} {killGroup} {dieGroup} {(long)e}";
                }

                foreach (var ch in character.Group.Sessions.GetAllItems().Where(s => s != null && s.HasSelectedCharacter).Select(s => s.Character))
                {
                    dieGroup += ch.ArenaDie;
                    killGroup += ch.ArenaKill;
                }
            }
            else if (e == AscrPacketType.Family)
            {
                if (character.Family == null)
                {
                    return $"ascr {packet} {killGroup} {dieGroup} {(long)e}";
                }

                foreach (var charac in character.Family.FamilyCharacters)
                {
                    dieGroup += charac.Character.ArenaDie;
                    killGroup += charac.Character.ArenaKill;
                }
            }

            return $"ascr {packet} {killGroup} {dieGroup} {(long)e}";
        }

        public static string GenerateAt(this Character character) => $"at {character.CharacterId} {character.MapInstance.Map.GridMapId} {character.PositionX} {character.PositionY} {character.Direction} 0 {character.MapInstance?.InstanceMusic ?? 0} 2 -1";

        public static string GenerateBfePacket(this Character character, short effect, short time) => $"bf_e 1 {character.CharacterId} {effect} {time}";

        public static string GenerateBlinit(this Character character)
        {
            string result = "blinit";

            foreach (CharacterRelationDTO relation in character.CharacterRelations.Where(s => s.CharacterId == character.CharacterId && s.RelationType == CharacterRelationType.Blocked))
            {
                result += $" {relation.RelatedCharacterId}|{DAOFactory.CharacterDAO.LoadById(relation.RelatedCharacterId)?.Name}";
            }

            return result;
        }

        public static string GenerateBubbleMessagePacket(this Character character)
        {
            return $"csp {character.CharacterId} {character.BubbleMessage}";
        }

        public static string GenerateCInfo(this Character character)
        {
            var morph = character.UseSp && !character.IsVehicled ? character.UseSp || character.IsVehicled || character.IsMorphed ? character.Morph : 0 : character.UseSp || character.IsVehicled || character.IsMorphed ? character.Morph : 0;

            var rank = 918;

            if (character.Family != null)
            {
                switch (character.FamilyCharacter.Authority)
                {
                    case FamilyAuthority.Familydeputy:
                        rank = 916;
                        break;
                    case FamilyAuthority.Familykeeper:
                        rank = 917;
                        break;
                    case FamilyAuthority.Head:
                        rank = 915;
                        break;
                }
            }

            return $"c_info " +
                   $"{character.Name}" +
                $" - -1 " +
                   $"{(character.Family != null && character.FamilyCharacter != null && !character.Undercover ? $"{character.Family.FamilyId}.{rank} {character.Family.Name}" : "-1 -")}" +
                $" {character.CharacterId} {(character.Invisible && character.Authority >= AuthorityType.TMOD ? 6 : 0)} {(byte)character.Gender} {(byte)character.HairStyle} {(byte)character.HairColor} {(byte)character.Class} {(character.GetDignityIco() == 1 ? character.GetReputationIco() : -character.GetDignityIco())}" +
                $" {character.Compliment} {morph}" +
                $" {(character.Invisible ? 1 : 0)} {character.Family?.FamilyLevel ?? 0} {(character.UseSp ? character.MorphUpgrade : 0)} {character.ArenaWinner} 0 0";
        }

        public static string GenerateCMap(this Character character, bool enterOnMap = false)
        {
            return $"c_map 0 {character.MapInstance.Map.MapId} {(enterOnMap ? 1 : 0)}";
        }

        public static string GenerateCMode(this Character character)
        {
            ItemInstance item = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Wings, InventoryType.Wear);

            var morph = (character.UseSp && !character.IsVehicled ? character.UseSp || character.IsVehicled || character.IsMorphed ? character.Morph : 0 : character.UseSp || character.IsVehicled || character.IsMorphed ? character.Morph : 0);

            var morphUpgrade2 = character.MorphUpgrade2 == 17 ? 7 : character.MorphUpgrade2;

            return !character.IsSeal ? $"c_mode 1 " +
                                       $"{character.CharacterId} " +
                                       $"{(morph)} " +
                                       $"{(!character.IsLaurenaMorph() && character.UseSp ? character.MorphUpgrade : 0)} " +
                                       $"{(!character.IsLaurenaMorph() && character.UseSp ? morphUpgrade2 : 0)} " +
                                       $"{character.ArenaWinner} {character.Size} {item?.Item.Morph ?? 0}" : "";
        }

        public static string GenerateCond(this Character character) => $"cond 1 {character.CharacterId} {(!character.IsLaurenaMorph() && !character.CanAttack() ? 1 : 0)} {(!character.CanMove() ? 1 : 0)} {character.Speed}";

        public static string GenerateDG(this Character character)
        {
            byte raidType = 0;

            if (ServerManager.Instance.Act4RaidStart.AddMinutes(30) < DateTime.Now)
            {
                ServerManager.Instance.Act4RaidStart = DateTime.Now;
            }

            double seconds = (ServerManager.Instance.Act4RaidStart.AddMinutes(30) - DateTime.Now).TotalSeconds;

            switch (character.Family?.Act4Raid?.MapInstanceType)
            {
                case MapInstanceType.Act4Morcos:
                    raidType = 1;
                    break;

                case MapInstanceType.Act4Hatus:
                    raidType = 2;
                    break;

                case MapInstanceType.Act4Calvina:
                    raidType = 3;
                    break;

                case MapInstanceType.Act4Berios:
                    raidType = 4;
                    break;
            }

            // $"dg {raidType} {(seconds > 1800 ? 1 : 2)} {(int)seconds} 0" 1 - closed & 2 - opened
            return $"dg {raidType} 2 {(int)seconds} 0";
        }

        public static string GenerateDir(this Character character) => $"dir 1 {character.CharacterId} {character.Direction}";

        public static string GenerateDm(this Character character, int dmg) => character.BattleEntity.GenerateDm(dmg);

        public static EffectPacket GenerateEff(this Character character, int effectid)
        {
            return new EffectPacket
            {
                EffectType = UserType.Player,
                CallerId = character.CharacterId,
                EffectId = effectid
            };
        }

        public static string GenerateEq(this Character character)
        {
            int color = (byte)character.HairColor;

            ItemInstance head = character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);

            if (head?.Item.IsColored == true)
            {
                color = head.Design;
            }

            byte display;

            if (character.Invisible && character.Authority >= AuthorityType.TGS)
            {
                display = 6; // Is this invisible GM ? 
            }
            else
            {
                if (character.Undercover)
                {
                    display = (byte) AuthorityType.User;
                }
                else
                {
                    display = (byte)character.Authority;
                }

                if (display == 0 && character.Authority >= AuthorityType.GM)
                {
                    display = 2;
                }
            }

            return $"eq " +
                   $"{character.CharacterId} " +
                   $"{display}" +
                   $" {(byte)character.Gender} " +
                   $"{(byte)character.HairStyle} " +
                   $"{color} " +
                   $"{(byte)character.Class} " +
                   $"{character.GenerateEqListForPacket()} " +
                   $"{(!character.InvisibleGm ? character.GenerateEqRareUpgradeForPacket() : null)}";

            //return $"eq {CharacterId} {(Invisible ? 6 : 0)} {(byte)Gender} {(byte)HairStyle} {color} {(byte)Class} {GenerateEqListForPacket()} {(!InvisibleGm ? GenerateEqRareUpgradeForPacket() : null)}";
        }

        public static string GenerateEqListForPacket(this Character character)
        {
            string[] invarray = new string[17];

            if (character.Inventory != null)
            {
                for (short i = 0; i < invarray.Length; i++)
                {
                    ItemInstance item = character.Inventory.LoadBySlotAndType(i, InventoryType.Wear);

                    if (item != null)
                    {
                        if (item.FusionVnum != 0)
                        {
                            invarray[i] = item.FusionVnum.ToString();
                        }
                        else
                        {
                            invarray[i] = item.ItemVNum.ToString();
                        }
                    }
                    else
                    {
                        invarray[i] = "-1";
                    }
                }
            }

            return $"{(!character.HatInVisible ? invarray[(byte)EquipmentType.Hat] :"-1")}.{invarray[(byte)EquipmentType.Armor]}.{invarray[(byte)EquipmentType.MainWeapon]}.{invarray[(byte)EquipmentType.SecondaryWeapon]}.{invarray[(byte)EquipmentType.Mask]}.{invarray[(byte)EquipmentType.Fairy]}.{invarray[(byte)EquipmentType.CostumeSuit]}.{(!character.HatInVisible ? invarray[(byte)EquipmentType.CostumeHat] : "-1")}.{invarray[(byte)EquipmentType.WeaponSkin]}.{invarray[(byte)EquipmentType.Wings]}";

        }

        public static string GenerateEqRareUpgradeForPacket(this Character character)
        {
            sbyte weaponRare = 0;
            byte weaponUpgrade = 0;
            sbyte armorRare = 0;
            byte armorUpgrade = 0;

            if (character.Inventory != null)
            {
                for (short i = 0; i < 16; i++)
                {
                    ItemInstance wearable = character.Inventory.LoadBySlotAndType(i, InventoryType.Wear);

                    if (wearable != null)
                    {
                        switch (wearable.Item.EquipmentSlot)
                        {
                            case EquipmentType.MainWeapon:
                                weaponRare = wearable.Rare;
                                weaponUpgrade = wearable.Upgrade;
                                break;

                            case EquipmentType.Armor:
                                armorRare = wearable.Rare;
                                armorUpgrade = wearable.Upgrade;
                                break;
                        }
                    }
                }
            }

            return $"{weaponUpgrade}{weaponRare} {armorUpgrade}{armorRare}";
        }

        public static string GenerateEquipment(this Character character)
        {
            var level = character.Level;

            string eqlist = "";

            character.EquipmentBCards.Lock(() =>
            {
                character.EquipmentBCards.Clear();
                character.ShellEffectArmor.Clear();
                character.ShellEffectMain.Clear();
                character.RuneEffectMain.Clear();
                character.ShellEffectSecondary.Clear();

                if (character.Inventory != null)
                {
                    for (short i = 0; i < 17; i++)
                    {
                        ItemInstance item = character.Inventory.LoadBySlotAndType(i, InventoryType.Wear);
                        if (item != null)
                        {
                            if (item.Item.EquipmentSlot != EquipmentType.Sp)
                            {
                                item.Item.BCards.ForEach(x =>
                                {
                                    if (x.MaxLevel == 0 || (level >= x.MinLevel && level <= x.MaxLevel))
                                    {
                                        character.EquipmentBCards.Add(x);

                                        if (character.IsInArenaLobby)
                                        {
                                            character.Hp = (int)character?.HPLoad();
                                            character.Mp = (int)character?.MPLoad();
                                        }
                                    }
                                });

                                switch (item.Item.ItemType)
                                {
                                    case ItemType.Armor:
                                        foreach (ShellEffectDTO dto in item.ShellEffects)
                                        {
                                            character.ShellEffectArmor.Add(dto);
                                        }
                                        break;

                                    case ItemType.Weapon:
                                        switch (item.Item.EquipmentSlot)
                                        {
                                            case EquipmentType.MainWeapon:
                                                foreach (ShellEffectDTO dto in item.ShellEffects.Where(s => !s.IsRune))
                                                {
                                                    character.ShellEffectMain.Add(dto);
                                                }
                                                foreach (ShellEffectDTO dto in item.RuneEffects.Where(s => s.IsRune))
                                                {
                                                    character.RuneEffectMain.Add(dto);
                                                }
                                                break;

                                            case EquipmentType.SecondaryWeapon:
                                                foreach (ShellEffectDTO dto in item.ShellEffects)
                                                {
                                                    character.ShellEffectSecondary.Add(dto);
                                                }
                                                break;
                                        }
                                        break;
                                }
                            }

                            eqlist += $" {i}.{item.Item.VNum}.{item.Rare}.{(item.FusionVnum != 0 ? item.FusionVnum : item.Item.IsColored ? item.Design : item.Upgrade)}.0.{item.RuneAmount}";
                        }
                    }
                }
            });

            return $"equip {character.GenerateEqRareUpgradeForPacket()}{eqlist}";
        }

        public static string GenerateExts(this Character character)
        {
            var medalType = 0;
            var extendedBackpackSize = 0;

            var hasEreniaMedal = character.HasStaticBonus(StaticBonusType.EreniaMedal);
            var hasAdventurerMedal = character.HasStaticBonus(StaticBonusType.AdventurerMedal);

            if (hasEreniaMedal)
            {
                medalType = 1;
                extendedBackpackSize = 8;
            }

            if (hasAdventurerMedal)
            {
                medalType = 2;
                extendedBackpackSize = 4;
            }

            if (hasEreniaMedal && hasAdventurerMedal)
            {
                medalType = 3;
                extendedBackpackSize = 12;
            }

            if (character.HaveExtension())
            {
                extendedBackpackSize = 60 + (character.HasStaticBonus(StaticBonusType.EreniaMedal) ? 8 : 0) + (character.HasStaticBonus(StaticBonusType.AdventurerMedal) ? 4 : 0);
            }


            var backPackCapacity = 48 + (character.HasStaticBonus(StaticBonusType.BackPack) ? 1 : 0) * 12 + extendedBackpackSize;

            return "exts " +
                   $"{(medalType == 2 ? 3 : medalType)} " +
                   $"{(backPackCapacity > 120 ? 120 : backPackCapacity)} " +
                   $"{(backPackCapacity > 120 ? 120 : backPackCapacity)} " +
                   $"{(backPackCapacity > 120 ? 120 : backPackCapacity)}";

        }

        public static string GenerateFaction(this Character character) => $"fs {(byte)character.Faction}";

        public static string GenerateFamilyMember(this Character character)
        {
            string str = "gmbr 0";
            try
            {
                if (character.Family?.FamilyCharacters != null)
                {
                    foreach (FamilyCharacter TargetCharacter in character.Family?.FamilyCharacters)
                    {
                        bool isOnline = CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, TargetCharacter.CharacterId);
                        int hours = (int)DateTime.Now.Subtract(TargetCharacter.Character.LastSave).TotalHours;
                        str += $" {TargetCharacter.Character.CharacterId}|{character.Family.FamilyId}|{TargetCharacter.Character.Name}|{TargetCharacter.Character.Level}|{(byte)TargetCharacter.Character.Class}|{(byte)TargetCharacter.Authority}|{(byte)TargetCharacter.Rank}|{(isOnline ? 1 : 0)}|{TargetCharacter.Character.HeroLevel}|{hours}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(null, ex);
            }
            return str;
        }

        public static string GenerateFamilyMemberExp(this Character character)
        {
            string str = "gexp";
            try
            {
                if (character.Family?.FamilyCharacters != null)
                {
                    foreach (FamilyCharacter TargetCharacter in character.Family?.FamilyCharacters)
                    {
                        str += $" {TargetCharacter.CharacterId}|{TargetCharacter.Experience}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(null, ex);
            }
            return str;
        }

        public static string GenerateFamilyMemberMessage(this Character character)
        {
            string str = "gmsg";
            try
            {
                if (character.Family?.FamilyCharacters != null)
                {
                    foreach (FamilyCharacter TargetCharacter in character.Family?.FamilyCharacters)
                    {
                        str += $" {TargetCharacter.CharacterId}|{TargetCharacter.DailyMessage}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(null, ex);
            }
            return str;
        }

        /*
        public List<string> GenerateFamilyWarehouseHist()
        {
            if (Family != null)
            {
                List<string> packetList = new List<string>();
                string packet = "";
                int i = 0;
                int amount = -1;
                List<FamilyLogDTO> warehouseLogs = Family.FamilyLogs.Where(s => s.FamilyLogType == FamilyLogType.WareHouseAdded || s.FamilyLogType == FamilyLogType.WareHouseRemoved).OrderByDescending(s => s.Timestamp).Take(100).ToList();
                foreach (FamilyLogDTO log in warehouseLogs)
                {
                    packet += $" {(log.FamilyLogType == FamilyLogType.WareHouseAdded ? 0 : 1)}|{log.FamilyLogData}|{(int)(DateTime.Now - log.Timestamp).TotalHours}";
                    i++;
                    if (i == 50)
                    {
                        i = 0;
                        packetList.Add($"fslog_stc {amount}{packet}");
                        amount++;
                    }
                    else if (i == warehouseLogs.Count)
                    {
                        packetList.Add($"fslog_stc {amount}{packet}");
                    }
                }

                return packetList;
            }
            return new List<string>();
        }
        */

        public static string GenerateFc(this Character character)
        {
            return $"fc {(byte)character.Faction} " +
                   $"{ServerManager.Instance.Act4AngelStat.MinutesUntilReset} " +
                   $"{ServerManager.Instance.Act4AngelStat.Percentage / 100} " +
                   $"{ServerManager.Instance.Act4AngelStat.Mode}" +
                $" {ServerManager.Instance.Act4AngelStat.CurrentTime} " +
                $"{ServerManager.Instance.Act4AngelStat.TotalTime} " +
                $"{Convert.ToByte(ServerManager.Instance.Act4AngelStat.IsMorcos)}" +
                $" {Convert.ToByte(ServerManager.Instance.Act4AngelStat.IsHatus)} " +
                $"{Convert.ToByte(ServerManager.Instance.Act4AngelStat.IsCalvina)} " +
                $"{Convert.ToByte(ServerManager.Instance.Act4AngelStat.IsBerios)}" +
                $" 0 " +
                $"{ServerManager.Instance.Act4DemonStat.Percentage / 100} " +
                $"{ServerManager.Instance.Act4DemonStat.Mode} " +
                $"{ServerManager.Instance.Act4DemonStat.CurrentTime} " +
                $"{ServerManager.Instance.Act4DemonStat.TotalTime}" +
                $" {Convert.ToByte(ServerManager.Instance.Act4DemonStat.IsMorcos)} " +
                $"{Convert.ToByte(ServerManager.Instance.Act4DemonStat.IsHatus)} " +
                $"{Convert.ToByte(ServerManager.Instance.Act4DemonStat.IsCalvina)} " +
                $"{Convert.ToByte(ServerManager.Instance.Act4DemonStat.IsBerios)} " +
                $"0";

            //return $"fc {Faction} 0 69 0 0 0 1 1 1 1 0 34 0 0 0 1 1 1 1 0";
        }

        public static string GenerateFd(this Character character) => $"fd {character.Reputation} {character.GetReputationIco()} {(int)character.Dignity} {Math.Abs(character.GetDignityIco())}";

        public static string GenerateFinfo(this Character character, long? relatedCharacterLoggedId, bool isConnected)
        {
            string result = "finfo";
            foreach (CharacterRelationDTO relation in character.CharacterRelations.Where(c => c.RelationType == CharacterRelationType.Friend || c.RelationType == CharacterRelationType.Spouse))
            {
                if (relatedCharacterLoggedId.HasValue && (relatedCharacterLoggedId.Value == relation.RelatedCharacterId || relatedCharacterLoggedId.Value == relation.CharacterId))
                {
                    result += $" {relatedCharacterLoggedId}.{(isConnected ? 1 : 0)}";
                }
            }
            return result;
        }

        public static string GenerateFinit(this Character character)
        {
            string result = "finit";
            foreach (CharacterRelationDTO relation in character.CharacterRelations.ToList().Where(c => c.RelationType == CharacterRelationType.Friend || c.RelationType == CharacterRelationType.Spouse))
            {
                long id = relation.RelatedCharacterId == character.CharacterId ? relation.CharacterId : relation.RelatedCharacterId;
                if (DAOFactory.CharacterDAO.LoadById(id) is CharacterDTO target)
                {
                    bool isOnline = CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, id);
                    result += $" {id}|{(short)relation.RelationType}|{(isOnline ? 1 : 0)}|{target.Name}";
                }
            }
            return result;
        }

        /*
        public string GenerateFStashAll()
        {
            string stash = $"f_stash_all {Family.WarehouseSize}";
            foreach (ItemInstance item in Family.Warehouse.GetAllItems())
            {
                stash += $" {item.GenerateStashPacket()}";
            }
            return stash;
        }
        */

        public static string GenerateFtPtPacket(this Character character) => $"ftpt {character.UltimatePoints} 3000";

        public static string GenerateGb(this Character character, byte type) => $"gb {type} {character.Session.Account.GoldBank / 1000} {character.Gold} 0 0";

        public static string GenerateGender(this Character character) => $"p_sex {(byte)character.Gender}";

        public static string GenerateGExp(this Character character)
        {
            string str = "gexp";
            foreach (FamilyCharacter familyCharacter in character.Family.FamilyCharacters)
            {
                str += $" {familyCharacter.CharacterId}|{familyCharacter.Experience}";
            }
            return str;
        }

        public static string GenerateGidx(this Character character)
        {
            var rank = 918;

            if (character.Family != null)
            {
                switch (character.FamilyCharacter.Authority)
                {
                    case FamilyAuthority.Familydeputy:
                        rank = 916;
                        break;
                    case FamilyAuthority.Familykeeper:
                        rank = 917;
                        break;
                    case FamilyAuthority.Head:
                        rank = 915;
                        break;
                }
            }

            return character.Family != null && character.FamilyCharacter != null ?
                $"gidx 1 {character.CharacterId} " +
                $"{character.Family.FamilyId}.{rank} " +
                $"{character.Family.Name} " +
                $"{character.Family.FamilyLevel} " +
                $"0|0|{character.Family.FamilyFaction}" :
                $"gidx 1 {character.CharacterId} -1 - 0 0|0|0";
        }
        public static string GenerateGInfo(this Character character)
        {
            if (character.Family != null)
            {
                try
                {
                    FamilyCharacter familyCharacter = character.Family.FamilyCharacters.Find(s => s.Authority == FamilyAuthority.Head);
                    if (familyCharacter != null)
                    {
                        return $"ginfo " +
                               $"{character.Family.Name} " +
                               $"{familyCharacter.Character.Name} " +
                               $"{(byte)character.Family.FamilyHeadGender} " +
                               $"{character.Family.FamilyLevel} " +
                               $"{character.Family.FamilyExperience} " +
                               $"{CharacterHelper.LoadFamilyXPData(character.Family.FamilyLevel)} " +
                               $"{character.Family.FamilyCharacters.Count} " +
                               $"{character.Family.MaxSize} " +
                               $"{(byte)character.FamilyCharacter.Authority} " +
                               $"{(character.Family.ManagerCanInvite ? 1 : 0)} " +
                               $"{(character.Family.ManagerCanNotice ? 1 : 0)} " +
                               $"{(character.Family.ManagerCanShout ? 1 : 0)} " +
                               $"{(character.Family.ManagerCanGetHistory ? 1 : 0)} " +
                               $"{(byte)character.Family.ManagerAuthorityType} " +
                               $"{(character.Family.MemberCanGetHistory ? 1 : 0)} " +
                               $"{(byte)character.Family.MemberAuthorityType} " +
                               $"{character.Family.FamilyMessage.Replace(' ', '^')}";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }
            return "";
        }

        // the function GenerateGold() was like this before public string GenerateGold() => $"gold
        // {Gold} {Session.Account.GoldBank / 1000}";

        public static string GenerateGold(this Character character)
        {
            if (character.Gold < 0 || character.Gold > ServerManager.Instance.Configuration.MaxGold)
            {
                character.Session.Account.GoldBank = 0;
                character.Gold = 0;
                foreach (var team in ServerManager.Instance.Sessions.Where(s => s.Account.Authority >= AuthorityType.GM))
                {
                    if (team.HasSelectedCharacter)
                    {
                        team.SendPacket(team.Character.GenerateSay($"User {character.Name} probably tried to hack gold, didn't work though.", 12));
                    }
                }
            }

            return $"gold {character.Gold} {character.Session.Account.GoldBank / 1000}";
        }

        public static string GenerateIcon(this Character character, int type, int value, short itemVNum) => $"icon {type} {character.CharacterId} {value} {itemVNum}";

        public static string GenerateIdentity(this Character character) => $"Character: {character.Name}";

        public static string GenerateIn(this Character character, bool foe = false, AuthorityType receiverAuthority = AuthorityType.User, int broadcastEffect = 0)
        {
            string name = character.Name;

            if (receiverAuthority >= AuthorityType.TMOD)
            {
                foe = false;

                if (ServerManager.Instance.ChannelId == 51)
                {
                    name = $"[{character.Faction}]{name}";
                }
            }

            if (foe && receiverAuthority < AuthorityType.TMOD)
            {
                name = ServerManager.Instance.ChannelId == 51 ? "!ยง$%&/()=?*+~#" : "Participant";
            }

            int faction = 0;

            if (ServerManager.Instance.ChannelId == 51)
            {
                faction = (byte)character.Faction + 2;
            }

            int color = character.HairStyle == HairStyleType.Hair8 ? 0 : (byte)character.HairColor;

            ItemInstance fairy = null;

            if (character.Inventory != null)
            {
                ItemInstance headWearable = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);

                if (headWearable?.Item.IsColored == true)
                {
                    color = headWearable.Design;
                }

                fairy = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            }

            long tit = 0;
            if (character.Title.Find(s => s.Stat.Equals(3)) != null)
            {
                tit = character.Title.Find(s => s.Stat.Equals(3)).TitleVnum;
            }
            if (character.Title.Find(s => s.Stat.Equals(7)) != null)
            {
                tit = character.Title.Find(s => s.Stat.Equals(7)).TitleVnum;
            }

            var morph = character.UseSp && !character.IsVehicled ? character.UseSp || character.IsVehicled || character.IsMorphed ? character.Morph : 0 : character.UseSp || character.IsVehicled || character.IsMorphed ? character.Morph : 0;

            var morphUpgrade2 = character.MorphUpgrade2 == 17 ? 7 : character.MorphUpgrade2;

            byte display;

            if (character.Undercover)
            {
                display = (byte)AuthorityType.User;
            }
            else
            {
                display = (byte)character.Authority;
            }

            if (character.MapInstance.MapInstanceType == MapInstanceType.BattleRoyaleInstance)
            {
                display = (byte)AuthorityType.User;
            }

            if (display == 0 && character.Authority >= AuthorityType.GM)
            {
                display = 2;
            }

            var displayCompliment = character.Compliment;

            if (character.MapInstance.MapInstanceType == MapInstanceType.BattleRoyaleInstance)
            {
                displayCompliment = 0;
            }

            var rank = 918;

            if (character.Family != null)
            {
                switch (character.FamilyCharacter.Authority)
                {
                    case FamilyAuthority.Familydeputy:
                        rank = 916;
                        break;
                    case FamilyAuthority.Familykeeper:
                        rank = 917;
                        break;
                    case FamilyAuthority.Head:
                        rank = 915;
                        break;
                }
            }

            return $"in 1 " +
                   $"{(character.MapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance ? CompetitiveRank.GetRankFromRp(character.RP).ToString() : name)} " +
                $"- {character.CharacterId} " +
                   $"{character.PositionX} " +
                   $"{character.PositionY} " +
                   $"{character.Direction} " +
                   $"{display} " +
                $"{(byte)character.Gender} " +
                   $"{(byte)character.HairStyle} " +
                   $"{color} " +
                   $"{(byte)character.Class} " +
                   $"{character.GenerateEqListForPacket()} " +
                   $"{Math.Ceiling(character.Hp / character.HPLoad() * 100)} " +
                   $"{Math.Ceiling(character.Mp / character.MPLoad() * 100)} " +
                $"{(character.IsSitting ? 1 : 0)} " +
                   $"{(character.Group?.GroupType == GroupType.Group ? (character.Group?.GroupId ?? -1) : -1)} " +
                   $"{(fairy != null && !character.Undercover ? 4 : 0)} " +
                $"{fairy?.Item.Element ?? 0} " +
                   $"0 " +
                   $"{fairy?.Item.Morph ?? 0} " +
                   $"{broadcastEffect} " +
                   $"{morph} " +
                   $"{character.GenerateEqRareUpgradeForPacket()} " +
                $"{(!character.Undercover ? (foe ? -1 : $"{character.Family?.FamilyId}.{rank}" ?? "-1") : -1)} " +
                   $"{(!character.Undercover ? (foe ? name : character.Family?.Name ?? "-") : "-")} " +
                   $"{(character.GetDignityIco() == 1 ? character.GetReputationIco(foe) : -character.GetDignityIco())} " +
                $"{(character.Invisible ? 1 : 0)} " +
                   $"{(character.UseSp ? character.MorphUpgrade : 0)} " +
                   $"{faction} " +
                   $"{(character.UseSp ? morphUpgrade2 : 0)} " +
                   $"{character.Level} " +
                   $"{character.Family?.FamilyLevel ?? 0} " +
                   $"0|0|{(foe ? 0 : character.Family?.FamilyFaction ?? 0)} " +
                   $"{character.ArenaWinner} " +
                   $"{displayCompliment} " +
                   $"{character.Size} " +
                   $"{character.HeroLevel} " +
                   $"{tit}";
        }

        public static string GenerateInvisible(this Character character) => $"cl {character.CharacterId} {(character.Invisible ? 1 : 0)} {(character.InvisibleGm ? 1 : 0)}";

        public static string GenerateLev(this Character character)
        {
            ItemInstance specialist = null;
            if (character.Inventory != null)
            {
                specialist = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            return $"lev {character.Level} {(int)(character.Level < 100 ? character.LevelXp : character.LevelXp / 100)} {(!character.UseSp || specialist == null ? character.JobLevel : specialist.SpLevel)} {(!character.UseSp || specialist == null ? character.JobLevelXp : specialist.XP)} {(int)(character.Level < 100 ? character.XpLoad() : character.XpLoad() / 100)} {(!character.UseSp || specialist == null ? character.JobXPLoad() : character.SpXpLoad())} {character.Reputation} {character.GetCP()} {(int)(character.HeroLevel < 100 ? character.HeroXp : character.HeroXp / 100)} {character.HeroLevel} {(int)(character.HeroLevel < 100 ? character.HeroXPLoad() : character.HeroXPLoad() / 100)} 0";
        }

        public static string GenerateLevelUp(this Character character)
        {
            Logger.Log.LogUserEvent("LEVELUP", character.Session.GenerateIdentity(), $"Level: {character.Level} JobLevel: {character.JobLevel} SPLevel: {character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear)?.SpLevel} HeroLevel: {character.HeroLevel} MapId: {character.Session.CurrentMapInstance?.Map.MapId} MapX: {character.PositionX} MapY: {character.PositionY}");
            return $"levelup {character.CharacterId}";
        }

        public static string GenerateMinilandObjectForFriends(this Character character)
        {
            string mlobjstring = "mltobj";
            int i = 0;
            foreach (MinilandObject mp in character.MinilandObjects)
            {
                mlobjstring += $" {mp.ItemInstance.ItemVNum}.{i}.{mp.MapX}.{mp.MapY}";
                i++;
            }
            return mlobjstring;
        }

        public static string GenerateMinilandPoint(this Character character) => $"mlpt {character.MinilandPoint} 100";

        public static string GenerateMinimapPosition(this Character character) => character.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance
                                                                                  || character.MapInstance.MapInstanceType == MapInstanceType.RaidInstance
            ? $"rsfp {character.MapInstance.MapIndexX} {character.MapInstance.MapIndexY}" : "rsfp 0 -1";

        public static string GenerateMlinfo(this Character character) => $"mlinfo 3800 {character.MinilandPoint} 100 {character.GeneralLogs.CountLinq(s => s.LogData == nameof(character.Miniland) && s.Timestamp.Day == DateTime.Now.Day)} {character.GeneralLogs.CountLinq(s => s.LogData == nameof(character.Miniland))} 10 {(byte)character.MinilandState} {Language.Instance.GetMessageFromKey("WELCOME_MUSIC_INFO")} {character.MinilandMessage.Replace(' ', '^')}";

        public static string GenerateMlinfobr(this Character character) => $"mlinfobr 3800 {character.Name} {character.GeneralLogs.CountLinq(s => s.LogData == nameof(character.Miniland) && s.Timestamp.Day == DateTime.Now.Day)} {character.GeneralLogs.CountLinq(s => s.LogData == nameof(character.Miniland))} 25 {character.MinilandMessage.Replace(' ', '^')}";

        public static string GenerateMloMg(this Character character, MinilandObject mlobj, MinigamePacket packet) => $"mlo_mg {packet.MinigameVNum} {character.MinilandPoint} 0 0 {mlobj.ItemInstance.DurabilityPoint} {mlobj.ItemInstance.Item.MinilandObjectPoint}";

        public static string GenerateNpcDialog(this Character character, int value) => $"npc_req 1 {character.CharacterId} {value}";

        public static bool HasStaticBonus(this Character character, StaticBonusType bonus) => character.StaticBonusList.Any(s => s.StaticBonusType == bonus);

        public static string GenerateNpcDialogEnd(this Character character) => $"npc_req -1 -1";

        public static string GeneratePairy(this Character character)
        {
            ItemInstance fairy = null;
            if (character.Inventory != null)
            {
                fairy = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            }
            character.ElementRate = 0;
            character.Element = 0;
            bool shouldChangeMorph = false;

            if (fairy != null)
            {
                bool hasEreniaMedal = character.HasStaticBonus(StaticBonusType.EreniaMedal);
                //exclude magical fairy
                shouldChangeMorph = character.IsUsingFairyBooster && (fairy.Item.Morph > 4 && fairy.Item.Morph != 9 && fairy.Item.Morph != 14);
                character.ElementRate += fairy.ElementRate + fairy.Item.ElementRate + (character.IsUsingFairyBooster ? 30 : 0) + (character.HasBuff(8017) ? 20 : 0 ) + (hasEreniaMedal ? 10 : 0) + character.GetStuffBuff(BCardType.CardType.PixieCostumeWings, (byte)AdditionalTypes.PixieCostumeWings.IncreaseFairyElement)[0];
                character.Element = fairy.Item.Element;
            }

            return fairy != null
                ? $"pairy 1 {character.CharacterId} 4 {fairy.Item.Element} {fairy.ElementRate + fairy.Item.ElementRate} {fairy.Item.Morph + (shouldChangeMorph ? 5 : 0)}"
                : $"pairy 1 {character.CharacterId} 0 0 0 0";
        }

        public static string GenerateParcel(this Character character, MailDTO mail) => mail.AttachmentVNum != null ? $"parcel 1 1 {character.MailList.First(s => s.Value.MailId == mail.MailId).Key} {(mail.Title == "NOSMALL" ? 1 : 4)} 0 {mail.Date.ToString("yyMMddHHmm")} {mail.Title} {mail.AttachmentVNum} {mail.AttachmentAmount} {(byte)ServerManager.GetItem((short)mail.AttachmentVNum).Type}" : "";

        public static string GeneratePetskill(this Character character, int VNum = -1) => $"petski {VNum}";

        public static string GeneratePidx(this Character character, bool isLeaveGroup = false)
        {
            if (!isLeaveGroup && character.Group != null)
            {
                string result = $"pidx {character.Group.GroupId}";
                foreach (ClientSession session in character.Group.Sessions.GetAllItems().Where(s => s.Character.CharacterId != character.CharacterId))
                {
                    if (session.Character != null)
                    {
                        result += $" {(character.Group.IsMemberOfGroup(character.CharacterId) ? 1 : 0)}.{session.Character.CharacterId} ";
                    }
                }
                foreach (ClientSession session in character.Group.Sessions.GetAllItems().Where(s => s.Character.CharacterId == character.CharacterId))
                {
                    if (session.Character != null)
                    {
                        result += $" {(character.Group.IsMemberOfGroup(character.CharacterId) ? 1 : 0)}.{session.Character.CharacterId} ";
                    }
                }
                return result;
            }
            return $"pidx -1 1.{character.CharacterId}";
        }

        public static string GeneratePinit(this Character character)
        {
            Group grp = ServerManager.Instance?.Groups?.Find(s => s.IsMemberOfGroup(character.CharacterId) && s?.GroupType == GroupType.Group);

            List<Mate> mates = character.Mates.ToList();

            int count = 0;

            string str = "";

            if (mates != null)
            {
                foreach (Mate mate in mates.Where(s => s.IsTeamMember).OrderByDescending(s => s.MateType))
                {
                    if ((byte)mate.MateType == 1)
                    {
                        count++;
                    }

                    str += $" 2|{mate.MateTransportId}|{(short)mate.MateType}|{mate.Level}|{(mate.IsUsingSp ? mate.Sp.GetName() : mate.Name.Replace(' ', '^'))}|-1|{(mate.IsUsingSp && mate.Sp != null ? mate.Sp.Instance.Item.Morph : mate.Monster.NpcMonsterVNum)}|0";
                }
            }

            if (grp != null)
            {
                foreach (ClientSession groupSessionForId in grp.Sessions.GetAllItems().Where(s => s.Character.CharacterId != character.CharacterId))
                {
                    count++;
                    str += $" 1|{groupSessionForId.Character.CharacterId}|{count}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|0|{(byte)groupSessionForId.Character.Gender}|{(byte)groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp || groupSessionForId.Character.IsVehicled || groupSessionForId.Character.IsMorphed ? groupSessionForId.Character.Morph : 0)}|{groupSessionForId.Character.HeroLevel}";
                }
                foreach (ClientSession groupSessionForId in grp.Sessions.GetAllItems().Where(s => s.Character.CharacterId == character.CharacterId))
                {
                    count++;
                    str += $" 1|{groupSessionForId.Character.CharacterId}|{count}|{groupSessionForId.Character.Level}|{groupSessionForId.Character.Name}|0|{(byte)groupSessionForId.Character.Gender}|{(byte)groupSessionForId.Character.Class}|{(groupSessionForId.Character.UseSp || groupSessionForId.Character.IsVehicled || groupSessionForId.Character.IsMorphed ? groupSessionForId.Character.Morph : 0)}|{groupSessionForId.Character.HeroLevel}";
                }
            }

            return $"pinit {(grp != null ? count : mates.Count(s => s.IsTeamMember))} {str}";
        }

        public static string GeneratePlayerFlag(this Character character, long pflag) => $"pflag 1 {character.CharacterId} {pflag}";

        public static string GeneratePost(this Character character, MailDTO mail, byte type)
        {
            if (mail != null)
            {
                return $"post 1 {type} {(character.MailList?.FirstOrDefault(s => s.Value?.MailId == mail?.MailId))?.Key} 0 {(mail.IsOpened ? 1 : 0)} {mail.Date.ToString("yyMMddHHmm")} {(type == 2 ? DAOFactory.CharacterDAO.LoadById(mail.ReceiverId).Name : DAOFactory.CharacterDAO.LoadById(mail.SenderId).Name)} {mail.Title}";
            }
            return "";
        }

        public static string GeneratePostMessage(this Character character, MailDTO mailDTO, byte type)
        {
            CharacterDTO sender = DAOFactory.CharacterDAO.LoadById(mailDTO.SenderId);

            return $"post 5 {type} {character.MailList.First(s => s.Value == mailDTO).Key} 0 0 {(byte)mailDTO.SenderClass} {(byte)mailDTO.SenderGender} {mailDTO.SenderMorphId} {(byte)mailDTO.SenderHairStyle} {(byte)mailDTO.SenderHairColor} {mailDTO.EqPacket} {sender.Name} {mailDTO.Title} {mailDTO.Message}";
        }

        public static List<string> GeneratePst(this Character character) => character.Mates.Where(s => s.IsTeamMember).OrderByDescending(s => s.MateType).Select(mate => $"pst 2 {mate.MateTransportId} {(mate.MateType == MateType.Partner ? "0" : "1")} {(int)(mate.Hp / mate.MaxHp * 100)} {(int)(mate.Mp / mate.MaxMp * 100)} {mate.Hp} {mate.Mp} 0 0 0 {mate.Buff.GetAllItems().Aggregate("", (current, buff) => current + $" {buff.Card.CardId}")}").ToList();

        public static string GeneratePStashAll(this Character character)
        {
            string stash = $"pstash_all {(character.HasStaticBonus(StaticBonusType.PetBackPack) ? 50 : 0)}";
            return character.Inventory.Values.Where(s => s.Type == InventoryType.PetWarehouse).Aggregate(stash, (current, item) => current + $" {item.GenerateStashPacket()}");
        }

        public static string GenerateQuestsPacket(this Character character, long newQuestId = -1)
        {
            short a = 0;
            short b = 6;
            character.Quests.ToList().ForEach(qst =>
            {
                qst.QuestNumber = qst.IsMainQuest
                    ? (short)5
                    : (!qst.IsMainQuest && !qst.Quest.IsDaily || qst.Quest.QuestId >= 5000 ? b++ : a++);
            });
            return $"qstlist {character.Quests.Aggregate("", (current, quest) => current + $" {quest.GetInfoPacket(quest.QuestId == newQuestId)}")}";
        }

        public static IEnumerable<string> GenerateQuicklist(this Character character)
        {
            string[] pktQs = { "qslot 0", "qslot 1" };

            var morph = character.UseSp && character.SpInstance != null ? character.SpInstance.Item.Morph : 0;
            if (character.Class == ClassType.MartialArtist && character.Morph == (byte)BrawlerMorphType.Dragon || character.Morph == (byte)BrawlerMorphType.Normal)
            {
                morph = 30;
            }

            switch (character.Class)
            {
                case ClassType.MartialArtist when character.Morph == 31 && character.UseSp && character.SpInstance?.SpLevel >= 20 && character.HasBuff(BCardType.CardType.LotusSkills, (byte)AdditionalTypes.LotusSkills.ChangeLotusSkills):
                    character.GenerateQuickListSp2Am(ref pktQs);
                    break;

                case ClassType.MartialArtist when character.Morph == 33 && character.UseSp && character.SpInstance?.SpLevel >= 20 && character.HasBuff(BCardType.CardType.WolfMaster, (byte)AdditionalTypes.WolfMaster.CanExecuteUltimateSkills):
                    character.GenerateQuickListSp3Am(ref pktQs);
                    break;

                default:
                    {
                        for (var i = 0; i < 30; i++)
                        {
                            for (var j = 0; j < 2; j++)
                            {
                                QuicklistEntryDTO qi = character.QuicklistEntries?.Find(n => n != null && n.Q1 == j && n.Q2 == i && n.Morph == morph);
                                pktQs[j] += $" {qi?.Type ?? 7}.{qi?.Slot ?? 7}.{qi?.Pos.ToString() ?? "-1"}";
                            }
                        }

                        break;
                    }
            }

            return pktQs;
        }

        public static string GenerateRaid(this Character character, int Type, bool exit = false)
        {
            string result = "";
            switch (Type)
            {
                case 0:
                    result = "raid 0";
                    character.Group?.Sessions?.ForEach(s => result += $" {s.Character?.CharacterId}");
                    break;

                case 2:
                    result = $"raid 2 {(exit ? "-1" : $"{character.Group?.Sessions?.FirstOrDefault().Character.CharacterId}")}";
                    break;

                case 1:
                    result = $"raid 1 {(exit ? 0 : 1)}";
                    break;

                case 3:
                    result = "raid 3";
                    character.Group?.Sessions?.ForEach(s => result += $" {s.Character?.CharacterId}.{Math.Ceiling(s.Character.Hp / s.Character.HPLoad() * 100)}.{Math.Ceiling(s.Character.Mp / s.Character.MPLoad() * 100)}");
                    break;

                case 4:
                    result = "raid 4";
                    break;

                case 5:
                    result = "raid 5 1";
                    break;
            }
            return result;
        }

        public static string GenerateRc(this Character character, int characterHealth) => character.BattleEntity.GenerateRc(characterHealth);

        public static string GenerateRCSList(this Character character, CSListPacket packet)
        {
            return BazaarHttpClient.Instance.GenerateRcsList(new GetRcsListQuery { Model = new RcsPacketModel {
                CharacterId = character.CharacterId,
                Packet = packet
            }});
        }

        public static string GenerateReqInfo(this Character character)
        {
            ItemInstance fairy = null;
            ItemInstance armor = null;
            ItemInstance weapon2 = null;
            ItemInstance weapon = null;

            if (character.Inventory != null)
            {
                fairy = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
                armor = character.Inventory.LoadBySlotAndType((byte)EquipmentType.Armor, InventoryType.Wear);
                weapon2 = character.Inventory.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                weapon = character.Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            }

            bool isPvpPrimary = false;
            bool isPvpSecondary = false;
            bool isPvpArmor = false;

            if (weapon?.Item.Name.Contains(": ") == true)
            {
                isPvpPrimary = true;
            }

            isPvpSecondary |= weapon2?.Item.Name.Contains(": ") == true;
            isPvpArmor |= armor?.Item.Name.Contains(": ") == true;

            return $"tc_info {character.Level} {(character.MapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance ? "ENEMY" : character.Name)} {fairy?.Item.Element ?? 0} {character.ElementRate} {(byte)character.Class} {(byte)character.Gender} {(character.Family != null ? $"{character.Family.FamilyId} {character.Family.Name}({Language.Instance.GetMessageFromKey(character.FamilyCharacter.Authority.ToString().ToUpper())})" : "-1 -")} {character.GetReputationIco()} {character.GetDignityIco()} {(weapon != null ? 1 : 0)} {weapon?.Rare ?? 0} {weapon?.Upgrade ?? 0} {(weapon2 != null ? 1 : 0)} {weapon2?.Rare ?? 0} {weapon2?.Upgrade ?? 0} {(armor != null ? 1 : 0)} {armor?.Rare ?? 0} {armor?.Upgrade ?? 0} {character.Act4Kill} {character.Act4Dead} {character.Reputation} 0 0 0 {(character.UseSp ? character.Morph : 0)} {character.TalentWin} {character.TalentLose} {character.TalentSurrender} 0 {character.MasterPoints} {character.Compliment} {character.Act4Points} {(isPvpPrimary ? 1 : 0)} {(isPvpSecondary ? 1 : 0)} {(isPvpArmor ? 1 : 0)} {character.HeroLevel} {(string.IsNullOrEmpty(character.Biography) ? Language.Instance.GetMessageFromKey("NO_PREZ_MESSAGE") : character.Biography)}";
        }

        public static string GenerateRest(this Character character) => $"rest 1 {character.CharacterId} {(character.IsSitting ? 1 : 0)}";

        public static string GenerateRevive(this Character character)
        {
            int lives = character.MapInstance.InstanceBag.Lives - character.MapInstance.InstanceBag.DeadList.Count + 1;
            if (character.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                lives = character.MapInstance.InstanceBag.Lives - character.MapInstance.InstanceBag.DeadList.Count(s => s == character.CharacterId) + 1;
            }
            return $"revive 1 {character.CharacterId} {(lives > 0 ? lives : 0)}";
        }

        public static string GenerateSay(this Character character, string message, int type, bool ignoreNickname = false)
        {
            if (string.IsNullOrEmpty(message) || character == null)
            {
                return string.Empty;
            }

            return $"say {(ignoreNickname ? 2 : 1)} {character.CharacterId} {type} {message}";
        }

        public static string GenerateSayItem(this Character character, string message, int type, byte itemInventory, short itemSlot, bool ignoreNickname = false)
        {
            if (character.Inventory.LoadBySlotAndType(itemSlot, (InventoryType)itemInventory) is ItemInstance item)
            {
                return $"sayitem {(ignoreNickname ? 2 : 1)} {character.CharacterId} {type} {message.Replace(' ', '|')} {(item.Item.EquipmentSlot == EquipmentType.Sp ? item.GenerateSlInfo() : item.GenerateEInfo())}";
            }
            return "";
        }

        public static string GenerateScal(this Character character) => $"char_sc 1 {character.CharacterId} {character.Size}";

        public static List<string> GenerateScN(this Character character)
        {
            List<string> list = new List<string>();
            byte i = 0;
            character.Mates.Where(s => s.MateType == MateType.Partner).ToList().ForEach(s =>
            {
                s.PetId = i;
                s.LoadInventory();
                list.Add(s.GenerateScPacket());
                i++;
            });
            return list;
        }

        public static List<string> GenerateScP(this Character character, byte page = 0)
        {
            List<string> list = new List<string>();

            byte i = 0;

            character.Mates.Where(s => s.MateType == MateType.Pet).Skip(page * 10).Take(10).ToList().ForEach(s =>
            {
                s.PetId = (byte)(page * 10 + i);
                list.Add(s.GenerateScPacket());
                i++;
            });

            return list;
        }

        public static string GenerateScpStc(this Character character) => $"sc_p_stc {(character.MaxMateCount - 10) / 10} {character.MaxPartnerCount - 3}";

        public static string GenerateShop(this Character character, string shopname) => $"shop 1 {character.CharacterId} 1 3 0 {shopname}";

        public static string GenerateShopEnd(this Character character) => $"shop 1 {character.CharacterId} 0 0";

        public static string GenerateSki(this Character character)
        {
            string ski = "ski";

            List<CharacterSkill> skills = character.GetSkills().OrderBy(s => s.Skill.CastId).OrderBy(s => s.SkillVNum < 200).ToList();

            if (skills.Count >= 2)
            {
                if (character.UseSp)
                {
                    ski += $" {skills[0].SkillVNum} {skills[0].SkillVNum}";
                }
                else
                {
                    ski += $" {skills[0].SkillVNum} {skills[1].SkillVNum}";
                }

                ski = skills.Aggregate(ski, (packet, characterSKill) => $"{packet} {(characterSKill.IsTattoo ? $"{characterSKill.SkillVNum}|{characterSKill.TattooLevel}" : $"{characterSKill.SkillVNum}")}");
            }

            return ski;
        }

        public static string GenerateSpk(this Character character, object message, int type) => $"spk 1 {character.CharacterId} {type} {character.Name} {message}";

        public static string GenerateSpPoint(this Character character) => $"sp {character.SpAdditionPoint} 1000000 {character.SpPoint} 10000";

        public static void GenerateQuickListSp2Am(this Character character, ref string[] pktQs)
        {
            var morph = character.Morph;
            if (character.Class == ClassType.MartialArtist && character.Morph == (byte)BrawlerMorphType.Dragon || character.Morph == (byte)BrawlerMorphType.Normal)
            {
                morph = 30;
            }

            for (var i = 0; i < 30; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    QuicklistEntryDTO qi = character.QuicklistEntries.Find(n => n.Q1 == j && n.Q2 == i && n.Morph == (character.UseSp ? morph : 0));
                    var pos = qi?.Pos;
                    if (pos >= 6 && pos <= 9)
                    {
                        pos += 5;
                    }
                    pktQs[j] += $" {qi?.Type ?? 7}.{qi?.Slot ?? 7}.{pos.ToString() ?? "-1"}";
                }
            }
        }

        private static void GenerateQuickListSp3Am(this Character character, ref string[] pktQs)
        {
            var morph = character.Morph;
            if (character.Class == ClassType.MartialArtist && character.Morph == (byte)BrawlerMorphType.Dragon || character.Morph == (byte)BrawlerMorphType.Normal)
            {
                morph = 30;
            }

            for (var i = 0; i < 30; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    QuicklistEntryDTO qi = character.QuicklistEntries.Find(n => n.Q1 == j && n.Q2 == i && n.Morph == (character.UseSp ? morph : 0));
                    short? pos = qi?.Pos;
                    if (pos.HasValue && pos == 3 && character.UltimatePoints >= 2000 || pos == 4 && character.UltimatePoints >= 1000 || pos == 5 && character.UltimatePoints >= 3000)
                    {
                        pos += 8;
                    }

                    if (pos.HasValue && pos == 10 && character.UltimatePoints >= 3000)
                    {
                        pos += 4;
                    }

                    pktQs[j] += $" {qi?.Type ?? 7}.{qi?.Slot ?? 7}.{pos.ToString() ?? "-1"}";
                }
            }
        }

        public static string GenerateStashAll(this Character character)
        {
            string stash = $"stash_all {character.WareHouseSize}";
            foreach (ItemInstance item in character.Inventory.Values.Where(s => s.Type == InventoryType.Warehouse))
            {
                stash += $" {item.GenerateStashPacket()}";
            }
            return stash;
        }

        public static string GenerateStat(this Character character)
        {
            double option =
                (character.WhisperBlocked ? Math.Pow(2, (int)CharacterOption.WhisperBlocked - 1) : 0)
                + (character.FamilyRequestBlocked ? Math.Pow(2, (int)CharacterOption.FamilyRequestBlocked - 1) : 0)
                + (!character.MouseAimLock ? Math.Pow(2, (int)CharacterOption.MouseAimLock - 1) : 0)
                + (character.MinilandInviteBlocked ? Math.Pow(2, (int)CharacterOption.MinilandInviteBlocked - 1) : 0)
                + (character.ExchangeBlocked ? Math.Pow(2, (int)CharacterOption.ExchangeBlocked - 1) : 0)
                + (character.FriendRequestBlocked ? Math.Pow(2, (int)CharacterOption.FriendRequestBlocked - 1) : 0)
                + (character.EmoticonsBlocked ? Math.Pow(2, (int)CharacterOption.EmoticonsBlocked - 1) : 0)
                + (character.HpBlocked ? Math.Pow(2, (int)CharacterOption.HpBlocked - 1) : 0)
                + (character.BuffBlocked ? Math.Pow(2, (int)CharacterOption.BuffBlocked - 1) : 0)
                + (character.HatInVisible ? Math.Pow(2, (int)CharacterOption.HatInVisible - 1) : 0)
                + (character.LockInterface ? Math.Pow(2, (int)CharacterOption.LockInterface - 1) : 0)
                + (character.GroupRequestBlocked ? Math.Pow(2, (int)CharacterOption.GroupRequestBlocked - 1) : 0)
                + (character.HeroChatBlocked ? Math.Pow(2, (int)CharacterOption.HeroChatBlocked - 1) : 0)
                + (character.QuickGetUp ? Math.Pow(2, (int)CharacterOption.QuickGetUp - 1) : 0)
                + (!character.IsPetAutoRelive ? 64 : 0)
                + (!character.IsPartnerAutoRelive ? 128 : 0);
            return $"stat {character.Hp} {character.HPLoad()} {character.Mp} {character.MPLoad()} 0 {option}";
        }

        public static List<string> GenerateStatChar(this Character character)
        {
            int weaponUpgrade = 0;
            int secondaryUpgrade = 0;
            int armorUpgrade = 0;
            character.MinHit = CharacterHelper.MinHit(character.Class, character.Level);
            character.MaxHit = CharacterHelper.MaxHit(character.Class, character.Level);
            character.HitRate = CharacterHelper.HitRate(character.Class, character.Level);
            character.HitCriticalChance = CharacterHelper.HitCriticalRate(character.Class, character.Level);
            character.HitCriticalRate = CharacterHelper.HitCritical(character.Class, character.Level);
            character.SecondWeaponMinHit = CharacterHelper.MinDistance(character.Class, character.Level);
            character.SecondWeaponMaxHit = CharacterHelper.MaxDistance(character.Class, character.Level);
            character.SecondWeaponHitRate = CharacterHelper.DistanceRate(character.Class, character.Level);
            character.SecondWeaponCriticalChance = CharacterHelper.DistCriticalRate(character.Class, character.Level);
            character.SecondWeaponCriticalRate = CharacterHelper.DistCritical(character.Class, character.Level);
            character.FireResistance = 0;
            character.LightResistance = 0;
            character.WaterResistance = 0;
            character.DarkResistance = 0;
            character.Defence = CharacterHelper.Defence(character.Class, character.Level);
            character.DefenceRate = CharacterHelper.DefenceRate(character.Class, character.Level);
            character.ElementRate = 0;
            character.ElementRateSP = 0;
            character.DistanceDefence = CharacterHelper.DistanceDefence(character.Class, character.Level);
            character.DistanceDefenceRate = CharacterHelper.DistanceDefenceRate(character.Class, character.Level);
            character.MagicalDefence = CharacterHelper.MagicalDefence(character.Class, character.Level);

            ItemInstance mainWeapon = character.Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon, InventoryType.Wear);
            ItemInstance secondaryWeapon = character.Inventory.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
            List<ShellEffectDTO> effects = new List<ShellEffectDTO>();
            if (mainWeapon?.ShellEffects != null)
            {
                effects.AddRange(mainWeapon.ShellEffects);
            }
            if (secondaryWeapon?.ShellEffects != null)
            {
                effects.AddRange(secondaryWeapon.ShellEffects);
            }

            int GetShellWeaponEffectValue(ShellWeaponEffectType effectType)
            {
                return effects?.Where(s => s.Effect == (byte)effectType)?.OrderByDescending(s => s.Value)?.FirstOrDefault()?.Value ?? 0;
            }

            if (character.UseSp)
            {
                // handle specialist
                ItemInstance specialist = character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                if (specialist != null)
                {
                    character.MinHit += specialist.DamageMinimum + (specialist.SpDamage * 10);
                    character.MaxHit += specialist.DamageMaximum + (specialist.SpDamage * 10);
                    character.SecondWeaponMinHit += specialist.DamageMinimum + (specialist.SpDamage * 10);
                    character.SecondWeaponMaxHit += specialist.DamageMaximum + (specialist.SpDamage * 10);
                    character.HitCriticalChance += specialist.CriticalLuckRate;
                    character.HitCriticalRate += specialist.CriticalRate;
                    character.SecondWeaponCriticalChance += specialist.CriticalLuckRate;
                    character.SecondWeaponCriticalRate += specialist.CriticalRate;
                    character.HitRate += specialist.HitRate;
                    character.SecondWeaponHitRate += specialist.HitRate;
                    character.DefenceRate += specialist.DefenceDodge;
                    character.DistanceDefenceRate += specialist.DistanceDefenceDodge;
                    character.FireResistance += specialist.Item.FireResistance + specialist.SpFire;
                    character.WaterResistance += specialist.Item.WaterResistance + specialist.SpWater;
                    character.LightResistance += specialist.Item.LightResistance + specialist.SpLight;
                    character.DarkResistance += specialist.Item.DarkResistance + specialist.SpDark;
                    character.ElementRateSP += specialist.ElementRate + specialist.SpElement;
                    character.Defence += specialist.CloseDefence + (specialist.SpDefence * 10);
                    character.DistanceDefence += specialist.DistanceDefence + (specialist.SpDefence * 10);
                    character.MagicalDefence += specialist.MagicDefence + (specialist.SpDefence * 10);

                    int point = CharacterHelper.SlPoint(specialist.SlDamage, 0) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLDamage) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                    if (point > 100) { point = 100; };

                    int p = 0;
                    if (point <= 10)
                    {
                        p = point * 5;
                    }
                    else if (point <= 20)
                    {
                        p = 50 + ((point - 10) * 6);
                    }
                    else if (point <= 30)
                    {
                        p = 110 + ((point - 20) * 7);
                    }
                    else if (point <= 40)
                    {
                        p = 180 + ((point - 30) * 8);
                    }
                    else if (point <= 50)
                    {
                        p = 260 + ((point - 40) * 9);
                    }
                    else if (point <= 60)
                    {
                        p = 350 + ((point - 50) * 10);
                    }
                    else if (point <= 70)
                    {
                        p = 450 + ((point - 60) * 11);
                    }
                    else if (point <= 80)
                    {
                        p = 560 + ((point - 70) * 13);
                    }
                    else if (point <= 90)
                    {
                        p = 690 + ((point - 80) * 14);
                    }
                    else if (point <= 94)
                    {
                        p = 830 + ((point - 90) * 15);
                    }
                    else if (point <= 95)
                    {
                        p = 890 + 16;
                    }
                    else if (point <= 97)
                    {
                        p = 906 + ((point - 95) * 17);
                    }
                    else if (point > 97)
                    {
                        p = 940 + ((point - 97) * 20);
                    }

                    character.MaxHit += p;
                    character.MinHit += p;
                    character.SecondWeaponMaxHit += p;
                    character.SecondWeaponMinHit += p;

                    point = CharacterHelper.SlPoint(specialist.SlDefence, 1) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLDefence) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                    if (point > 100) { point = 100; };

                    p = 0;
                    if (point <= 10)
                    {
                        p = point;
                    }
                    else if (point <= 20)
                    {
                        p = 10 + ((point - 10) * 4);
                    }
                    else if (point <= 30)
                    {
                        p = 30 + ((point - 20) * 6);
                    }
                    else if (point <= 40)
                    {
                        p = 60 + ((point - 30) * 8);
                    }
                    else if (point <= 50)
                    {
                        p = 100 + ((point - 40) * 10);
                    }
                    else if (point <= 60)
                    {
                        p = 150 + ((point - 50) * 12);
                    }
                    else if (point <= 70)
                    {
                        p = 210 + ((point - 60) * 14);
                    }
                    else if (point <= 80)
                    {
                        p = 280 + ((point - 70) * 16);
                    }
                    else if (point <= 90)
                    {
                        p = 360 + ((point - 80) * 18);
                    }
                    else if (point > 90)
                    {
                        p = 450 + ((point - 90) * 20);
                    }

                    character.Defence += p;
                    character.MagicalDefence += p;
                    character.DistanceDefence += p;

                    point = CharacterHelper.SlPoint(specialist.SlElement, 2) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLElement) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                    if (point > 100) { point = 100; };

                    p = point <= 50 ? point : 50 + ((point - 50) * 2);
                    character.ElementRateSP += p;

                    character.slhpbonus = GetShellWeaponEffectValue(ShellWeaponEffectType.SLHP) + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                }
            }

            // TODO: add base stats


            if (mainWeapon != null)
            {
                weaponUpgrade = mainWeapon.Upgrade;
                character.MinHit += mainWeapon.DamageMinimum + mainWeapon.Item.DamageMinimum + GetShellWeaponEffectValue(ShellWeaponEffectType.DamageImproved);
                character.MaxHit += mainWeapon.DamageMaximum + mainWeapon.Item.DamageMaximum + GetShellWeaponEffectValue(ShellWeaponEffectType.DamageImproved);
                character.HitRate += mainWeapon.HitRate + mainWeapon.Item.HitRate;
                character.HitCriticalChance += mainWeapon.CriticalLuckRate + mainWeapon.Item.CriticalLuckRate;
                character.HitCriticalRate += mainWeapon.CriticalRate + mainWeapon.Item.CriticalRate;

                // maxhp-mp
            }


            if (secondaryWeapon != null)
            {
                secondaryUpgrade = secondaryWeapon.Upgrade;
                character.SecondWeaponMinHit += secondaryWeapon.DamageMinimum + secondaryWeapon.Item.DamageMinimum + GetShellWeaponEffectValue(ShellWeaponEffectType.DamageImproved);
                character.SecondWeaponMaxHit += secondaryWeapon.DamageMaximum + secondaryWeapon.Item.DamageMaximum + GetShellWeaponEffectValue(ShellWeaponEffectType.DamageImproved);
                character.SecondWeaponHitRate += secondaryWeapon.HitRate + secondaryWeapon.Item.HitRate;
                character.SecondWeaponCriticalChance += secondaryWeapon.CriticalLuckRate + secondaryWeapon.Item.CriticalLuckRate;
                character.SecondWeaponCriticalRate += secondaryWeapon.CriticalRate + secondaryWeapon.Item.CriticalRate;

                // maxhp-mp
            }

            ItemInstance armor = character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Armor, InventoryType.Wear);
            if (armor != null)
            {
                armorUpgrade = armor.Upgrade;
                character.Defence += armor.CloseDefence + armor.Item.CloseDefence;
                character.DefenceRate += armor.DefenceDodge + armor.Item.DefenceDodge;
                character.MagicalDefence += armor.MagicDefence + armor.Item.MagicDefence;
                character.DistanceDefence += armor.DistanceDefence + armor.Item.DistanceDefence;
                character.DistanceDefenceRate += armor.DistanceDefenceDodge + armor.Item.DistanceDefenceDodge;
            }

            ItemInstance fairy = character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (fairy != null)
            {
                bool hasEreniaMedal = character.HasStaticBonus(StaticBonusType.EreniaMedal);
                character.ElementRate += fairy.ElementRate + fairy.Item.ElementRate + (character.IsUsingFairyBooster ? 30 : 0) + (hasEreniaMedal ? 10 : 0) + (character.HasBuff(8017) ? 20 : 0)
                                         + character.GetStuffBuff(BCardType.CardType.PixieCostumeWings, (byte)AdditionalTypes.PixieCostumeWings.IncreaseFairyElement)[0];
            }

            for (short i = 1; i < 14; i++)
            {
                ItemInstance item = character.Inventory?.LoadBySlotAndType(i, InventoryType.Wear);
                if (item != null && item.Item.EquipmentSlot != EquipmentType.MainWeapon
                        && item.Item.EquipmentSlot != EquipmentType.SecondaryWeapon
                        && item.Item.EquipmentSlot != EquipmentType.Armor
                        && item.Item.EquipmentSlot != EquipmentType.Sp)
                {
                    character.FireResistance += item.FireResistance + item.Item.FireResistance;
                    character.LightResistance += item.LightResistance + item.Item.LightResistance;
                    character.WaterResistance += item.WaterResistance + item.Item.WaterResistance;
                    character.DarkResistance += item.DarkResistance + item.Item.DarkResistance;
                    character.Defence += item.CloseDefence + item.Item.CloseDefence;
                    character.DefenceRate += item.DefenceDodge + item.Item.DefenceDodge;
                    character.MagicalDefence += item.MagicDefence + item.Item.MagicDefence;
                    character.DistanceDefence += item.DistanceDefence + item.Item.DistanceDefence;
                    character.DistanceDefenceRate += item.DistanceDefenceDodge + item.Item.DistanceDefenceDodge;
                }
            }

            //BCards
            int BCardFireResistance = character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireIncreased)[0] + character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
            int BCardLightResistance = character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightIncreased)[0] + character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
            int BCardWaterResistance = character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterIncreased)[0] + character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];
            int BCardDarkResistance = character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkIncreased)[0] + character.GetStuffBuff(BCardType.CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0];

            int BCardHitCritical = character.GetStuffBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased)[0] + character.GetStuffBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased)[0];
            int BCardHitCriticalRate = character.GetStuffBuff(BCardType.CardType.Critical, (byte)AdditionalTypes.Critical.InflictingIncreased)[0];

            int BCardHit = character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)[0];
            int BCardSecondHit = character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)[0];

            int BCardHitRate = character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.AllHitRateIncreased)[0];
            int BCardSecondHitRate = character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.AllHitRateIncreased)[0];

            int BCardMeleeDodge = character.GetStuffBuff(BCardType.CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.Target.AllHitRateIncreased)[0];
            int BCardRangeDodge = character.GetStuffBuff(BCardType.CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.Target.AllHitRateIncreased)[0];

            int BCardMeleeDefence = character.GetStuffBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0] + character.GetStuffBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.MeleeIncreased)[0];
            int BCardRangeDefence = character.GetStuffBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0] + character.GetStuffBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased)[0];
            int BCardMagicDefence = character.GetStuffBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0] + character.GetStuffBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.MagicalIncreased)[0];

            switch (character.Class)
            {
                case ClassType.Adventurer:
                case ClassType.Swordsman:
                    BCardHit += character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    BCardSecondHit += character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)[0];
                    BCardHitRate += character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.MeleeHitRateIncreased)[0];
                    BCardSecondHitRate += character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.RangedHitRateIncreased)[0];
                    break;

                case ClassType.Archer:
                    BCardHit += character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)[0];
                    BCardSecondHit += character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0];
                    BCardHitRate += character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.RangedHitRateIncreased)[0];
                    BCardSecondHitRate += character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.MeleeHitRateIncreased)[0];
                    break;

                case ClassType.Magician:
                    BCardHit += character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksIncreased)[0];
                    BCardSecondHit += character.GetStuffBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)[0];
                    BCardHitRate += character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.MagicalConcentrationIncreased)[0];
                    BCardSecondHitRate += character.GetStuffBuff(BCardType.CardType.Target, (byte)AdditionalTypes.Target.RangedHitRateIncreased)[0];
                    break;
            }

            byte type = character.Class == ClassType.Adventurer ? (byte)0 : (byte)(character.Class - 1);

            List<string> packets = new List<string>();
            packets.Add($"sc {type} {(weaponUpgrade == 10 ? weaponUpgrade : weaponUpgrade + character.GetBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AttackLevelIncreased)[0])} {character.MinHit + BCardHit} {character.MaxHit + BCardHit} {character.HitRate + BCardHitRate} {character.HitCriticalChance + BCardHitCriticalRate} {character.HitCriticalRate + BCardHitCritical} {(character.Class == ClassType.Archer ? 1 : 0)} {(secondaryUpgrade == 10 ? secondaryUpgrade : secondaryUpgrade + character.GetBuff(BCardType.CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AttackLevelIncreased)[0])} {character.SecondWeaponMinHit + BCardSecondHit} {character.SecondWeaponMaxHit + BCardSecondHit} {character.SecondWeaponHitRate + BCardSecondHitRate} {character.SecondWeaponCriticalChance + BCardHitCriticalRate} {character.SecondWeaponCriticalRate + BCardHitCritical} {(armorUpgrade == 10 ? armorUpgrade : armorUpgrade + character.GetBuff(BCardType.CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelIncreased)[0])} {character.Defence + BCardMeleeDefence} {character.DefenceRate + BCardMeleeDodge} {character.DistanceDefence + BCardRangeDefence} {character.DistanceDefenceRate + BCardRangeDodge} {character.MagicalDefence + BCardMagicDefence} {character.FireResistance + BCardFireResistance} {character.WaterResistance + BCardWaterResistance} {character.LightResistance + BCardLightResistance} {character.DarkResistance + BCardDarkResistance}");
            packets.AddRange(character.GenerateScN());
            packets.AddRange(character.GenerateScP());

            character.LoadSpeed();

            return packets;
        }

        public static string GenerateStatInfo(this Character character) => $"st 1 {character.CharacterId} {character.Level} {character.HeroLevel} {(int)(character.Hp / (float)character.HPLoad() * 100)} {(int)(character.Mp / (float)character.MPLoad() * 100)} {character.Hp} {character.Mp}{character.Buff.GetAllItems().Where(s => !s.StaticBuff || new short[] { 339, 340 }.Contains(s.Card.CardId)).Aggregate("", (current, buff) => current + $" {buff.Card.CardId}.{buff.Level}")}";

        public static string GenerateTaF(this Character character, byte victoriousteam)
        {
            ConcurrentBag<ArenaTeamMember> tm = ServerManager.Instance.ArenaTeams.ToList().FirstOrDefault(s => s.Any(o => o.Session == character.Session));
            var score1 = 0;
            var score2 = 0;
            var life1 = 0;
            var life2 = 0;
            var call1 = 0;
            var call2 = 0;
            var atype = ArenaTeamType.Erenia;
            if (tm == null)
            {
                return $"ta_f 0 {victoriousteam} {(byte)atype} {score1} {life1} {call1} {score2} {life2} {call2}";
            }

            var tmem = tm.FirstOrDefault(s => s.Session == character.Session);
            if (tmem == null)
            {
                return $"ta_f 0 {victoriousteam} {(byte)atype} {score1} {life1} {call1} {score2} {life2} {call2}";
            }

            atype = tmem.ArenaTeamType;
            IEnumerable<long> ids = tm.Replace(s => tmem.ArenaTeamType == s.ArenaTeamType).Select(s => s.Session.Character.CharacterId);
            ConcurrentBag<ArenaTeamMember> oposit = tm.Replace(s => tmem.ArenaTeamType != s.ArenaTeamType);
            ConcurrentBag<ArenaTeamMember> own = tm.Replace(s => tmem.ArenaTeamType == s.ArenaTeamType);
            score1 = 3 - character.MapInstance.InstanceBag.DeadList.Count(s => ids.Contains(s));
            score2 = 3 - character.MapInstance.InstanceBag.DeadList.Count(s => !ids.Contains(s));
            life1 = 3 - own.Count(s => s.Dead);
            life2 = 3 - oposit.Count(s => s.Dead);
            call1 = 5 - own.Sum(s => s.SummonCount);
            call2 = 5 - oposit.Sum(s => s.SummonCount);
            return $"ta_f 0 {victoriousteam} {(byte)atype} {score1} {life1} {call1} {score2} {life2} {call2}";
        }

        public static string GenerateTaFc(this Character character, byte type) => $"ta_fc {type} {character.CharacterId}";
        
        public static TalkPacket GenerateTalk(this Character character, string message)
        {
            return new TalkPacket
            {
                CharacterId = character.CharacterId,
                Message = message
            };
        }

        public static string GenerateTaM(this Character character, int type)
        {
            if (character == null) return string.Empty;
            var score1 = 0;
            var score2 = 0;
            switch (character.TalentArenaBattle.ArenaTeamType)
            {
                case ArenaTeamType.Zenas:
                    score1 = character.TalentArenaBattle.RoundWin;
                    score2 = character.TalentArenaBattle.Opponent.Character.TalentArenaBattle.RoundWin;
                    break;

                case ArenaTeamType.Erenia:
                    score1 = character.TalentArenaBattle.Opponent.Character.TalentArenaBattle.RoundWin;
                    score2 = character.TalentArenaBattle.RoundWin;
                    break;
            }
            return $"ta_m {type} {score1} {score2} {(type == 3 ? character.MapInstance.InstanceBag.Clock.SecondsRemaining / 10 : 0)} 0";
        }

        public static string GenerateTaP(this Character character, Character opponent, byte tatype, ArenaTeamType type, bool showOponent)
        {
            var groups = "";

            if (character == null) return $"ta_p {tatype} {(byte)type} 5 5 {groups.TrimEnd(' ')}";

            groups += $"{(character.TalentArenaBattle.IsDead ? 1 : 0)}.{character.CharacterId}.{(byte)character.Class}.{(byte)character.Gender}.{(byte)character.Morph} -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 ";

            if (showOponent)
            {
                groups += $"{(opponent.TalentArenaBattle.IsDead ? 1 : 0)}.{opponent.CharacterId}.{(byte)opponent.Class}.{(byte)opponent.Gender}.{(byte)opponent.Morph} -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 ";
            }

            return $"ta_p {tatype} {(byte)type} 0 0 {groups.TrimEnd(' ')}";
        }

        public static string GenerateTaPs(this Character character, Character opponent)
        {
            string groups = "";

            if (character == null || opponent == null) return $"ta_ps {groups.TrimEnd(' ')}";

            groups += $"{character.CharacterId}.{(int)(character.Hp / character.HPLoad() * 100)}.{(int)(character.Mp / character.MPLoad() * 100)}.0 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 ";
            groups += $"{opponent.CharacterId}.{(int)(opponent.Hp / opponent.HPLoad() * 100)}.{(int)(opponent.Mp / opponent.MPLoad() * 100)}.0 -1.-1.-1.-1.-1 -1.-1.-1.-1.-1 ";

            return $"ta_ps {groups.TrimEnd(' ')}";
        }

        public static string GenerateTit(this Character character)
        {
            var classtype = 35;

            switch (character.Class)
            {
                case ClassType.Archer:
                    classtype = 37;
                    break;
                case ClassType.Magician:
                    classtype = 38;
                    break;
                case ClassType.MartialArtist:
                    classtype = 39;
                    break;
                case ClassType.Swordsman:
                    classtype = 36;
                    break;
            }

            return $"tit {classtype} {character.Name}";
        }

        public static string GenerateTitInfo(this Character character)
        {
            long tit = 0;
            long eff = 0;
            if (character.Title.Find(s => s.Stat.Equals(3)) != null)
            {
                tit = character.Title.Find(s => s.Stat.Equals(3)).TitleVnum;
            }
            if (character.Title.Find(s => s.Stat.Equals(7)) != null)
            {
                tit = character.Title.Find(s => s.Stat.Equals(7)).TitleVnum;
            }
            if (character.Title.Find(s => s.Stat.Equals(5)) != null)
            {
                eff = character.Title.Find(s => s.Stat.Equals(5)).TitleVnum;
            }
            return $"titinfo 1 {character.CharacterId} {tit} {(character.Title.Find(s => s.Stat.Equals(7)) != null ? tit : eff)}";
        }

        public static string GenerateTitle(this Character character)
        {
            string tit = string.Empty;
            foreach (var t in character.Title.ToList())
            {
                tit += $"{t.TitleVnum - 9300}.{t.Stat} ";
            }
            return $"title {tit}";
        }

        public static string GenerateTp(this Character character) => character.BattleEntity.GenerateTp();

        public static string GetMinilandObjectList(this Character character)
        {
            string mlobjstring = "mlobjlst";
            foreach (ItemInstance item in character.Inventory.Values.Where(s => s.Type == InventoryType.Miniland).OrderBy(s => s.Slot))
            {
                MinilandObject mp = character.MinilandObjects.Find(s => s.ItemInstanceId == item.Id);
                bool used = mp != null;
                mlobjstring += $" {item.Slot}.{(used ? 1 : 0)}.{(used ? mp.MapX : 0)}.{(used ? mp.MapY : 0)}.{(item.Item.Width != 0 ? item.Item.Width : 1) }.{(item.Item.Height != 0 ? item.Item.Height : 1) }.{(used ? mp.ItemInstance.DurabilityPoint : 0)}.100.0.1";
            }

            return mlobjstring;
        }
    }
}
