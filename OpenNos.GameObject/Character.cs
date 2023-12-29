/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using NosByte.Packets.ServerPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.ConcurrencyExtensions;
using OpenNos.Core.Extensions;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Battle;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.GameObject.RainbowBattle;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class Character : CharacterDTO
    {
        #region Members

        public bool _isStaticBuffListInitial;
        public int OriginalFaction = -1;
        public int slhpbonus;
        private readonly object _syncObj = new();
        private Random _random;
        private byte _speed;

        #endregion

        #region Instantiation

        public Character()
        {
            GroupSentRequestCharacterIds = new();
            FamilyInviteCharacters = new();
            TradeRequests = new();
            FriendRequestCharacters = new();
            MarryRequestCharacters = new();
            StaticBonusList = new();
            MinilandObjects = new();
            Mates = new();
            LastMonsterAggro = DateTime.Now;
            LastPulse = DateTime.Now;
            LastFreeze = DateTime.Now;
            MTListTargetQueue = new();
            MeditationDictionary = new();
            PVELockObject = new();
            SpeedLockObject = new();
            ShellEffectArmor = new();
            ShellEffectMain = new();
            ShellEffectSecondary = new();
            RuneEffectMain = new();
            Quests = new();
            Title = new();
            EffectFromTitle = new();
            QuestLogs = new();
            InstantBattleLogs = new();
            VisualFromTitle = new();
            BazaarItems = new();
            LastKilledChars = new();
            BazaarActionTimer = new();
            EnergyFields = new();
            BattlePassItemLogs = new();
            BattlePassQuestLogs = new();
        }

        public Character(CharacterDTO input) : this()
        {
            AccountId = input.AccountId;
            Act4Dead = input.Act4Dead;
            Act4Kill = input.Act4Kill;
            Act4Points = input.Act4Points;
            ArenaWinner = input.ArenaWinner;
            Biography = input.Biography;
            BuffBlocked = input.BuffBlocked;
            HatInVisible = input.HatInVisible;
            LockInterface = input.LockInterface;
            CharacterId = input.CharacterId;
            Class = input.Class;
            Compliment = input.Compliment;
            Dignity = input.Dignity;
            EmoticonsBlocked = input.EmoticonsBlocked;
            ExchangeBlocked = input.ExchangeBlocked;
            Faction = input.Faction;
            FamilyRequestBlocked = input.FamilyRequestBlocked;
            FriendRequestBlocked = input.FriendRequestBlocked;
            Gender = input.Gender;
            Gold = input.Gold;
            GroupRequestBlocked = input.GroupRequestBlocked;
            HairColor = input.HairColor;
            HairStyle = input.HairStyle;
            HeroChatBlocked = input.HeroChatBlocked;
            HeroLevel = input.HeroLevel;
            HeroXp = input.HeroXp;
            Hp = input.Hp;
            HpBlocked = input.HpBlocked;
            IsPetAutoRelive = input.IsPetAutoRelive;
            IsPartnerAutoRelive = input.IsPartnerAutoRelive;
            IsSeal = input.IsSeal;
            JobLevel = input.JobLevel;
            JobLevelXp = input.JobLevelXp;
            LastFamilyLeave = input.LastFamilyLeave;
            Level = input.Level;
            LevelXp = input.LevelXp;
            MapId = input.MapId;
            MapX = input.MapX;
            MapY = input.MapY;
            MasterPoints = input.MasterPoints;
            MasterTicket = input.MasterTicket;
            MaxMateCount = input.MaxMateCount;
            MaxPartnerCount = input.MaxPartnerCount;
            MinilandInviteBlocked = input.MinilandInviteBlocked;
            MinilandMessage = input.MinilandMessage;
            MinilandPoint = input.MinilandPoint;
            MinilandState = input.MinilandState;
            MouseAimLock = input.MouseAimLock;
            Mp = input.Mp;
            Name = input.Name;
            QuickGetUp = input.QuickGetUp;
            RagePoint = input.RagePoint;
            Reputation = input.Reputation;
            Slot = input.Slot;
            SpAdditionPoint = input.SpAdditionPoint;
            SpPoint = input.SpPoint;
            State = input.State;
            TalentLose = input.TalentLose;
            TalentSurrender = input.TalentSurrender;
            TalentWin = input.TalentWin;
            WhisperBlocked = input.WhisperBlocked;
            ArenaDie = input.ArenaDie;
            ArenaKill = input.ArenaKill;
            ArenaTc = input.ArenaTc;
            IsChangeName = input.IsChangeName;
            LastSave = input.LastSave;
            RP = input.RP;
            SkyTowerLevel = input.SkyTowerLevel;
            BattlePassPoints = input.BattlePassPoints;
            HavePremiumBattlePass = input.HavePremiumBattlePass;
            AliveCountdown = input.AliveCountdown;
        }

        #endregion

        #region Properties

        public short LastMapId { get; set; }

        public short LastMapX { get; set; }

        public short LastMapY { get; set; }

        public IDisposable ArenaDisposable { get; set; }

        public IDisposable StairsDisposable { get; set; }

        public byte ArenaCooldown { get; set; } = 30;

        public short DefaultTimer { get; set; } = 120;

        public List<ClientSession> RedDead = new();

        public List<ClientSession> BlueDead = new();

        public TalentArenaBattle TalentArenaBattle = new();

        public DeathMatchBattle DeathMatchMember = new();

        public OneVersusOneBattle OneVersusOneBattle = new();

        public TwoVersusTwoMember TwoVersusTwoBattle = new();

        public KingOfTheHillCharacter kingOfTheHill = new();

        public DateTime LastRbbEffect { get; set; }

        public DateTime LastRbbDeath { get; set; }

        public bool HealthStop { get; set; }

        public IDisposable AutoBetInterval { get; set; }

        public BazaarActionTimer BazaarActionTimer { get; set; }

        public short OldMapId { get; set; }

        public DateTime LastExchange { get; set; }
        public ConcurrentDictionary<long, BazaarItemDTO> BazaarItems { get; set; }

        public ThreadSafeGenericList<InstantBattleLogDTO> InstantBattleLogs { get; set; }

        public ThreadSafeGenericList<QuestLogDTO> QuestLogs { get; set; }

        public DateTime LastSpeakerUse { get; set; }

        public AuthorityType Authority => Session.Account.Authority;

        public BattleEntity BattleEntity { get; set; }

        public byte BeforeDirection { get; set; }

        public string BubbleMessage { get; set; }

        public DateTime BubbleMessageEnd { get; set; }

        public byte SnackRequests { get; set; }

        public byte PotionRequests { get; set; }

        public byte BazarRequests { get; set; }

        public byte SayRequests { get; set; }

        public byte LastDropRequests { get; set; }

        public byte LastPdtseRequests { get; set; }

        public ThreadSafeSortedList<short, Buff> Buff => BattleEntity.Buffs;

        public ThreadSafeSortedList<short, IDisposable> BuffObservables => BattleEntity.BuffObservables;

        public bool CanCreateTimeSpace { get; set; }

        public bool CanFight => !IsSitting && ExchangeInfo == null;

        public ThreadSafeGenericList<CellonOptionDTO> CellonOptions => BattleEntity.CellonOptions;

        public ServerManager Channel { get; set; }

        public List<CharacterRelationDTO> CharacterRelations
        {
            get
            {
                lock (ServerManager.Instance.CharacterRelations)
                {
                    return ServerManager.Instance.CharacterRelations == null ? new List<CharacterRelationDTO>() : ServerManager.Instance.CharacterRelations.Where(s => s.CharacterId == CharacterId || s.RelatedCharacterId == CharacterId).ToList();
                }
            }
        }

        public int ChargeValue { get; set; }

        public int ConvertedDamageToHP { get; set; }

        public long CurrentDie { get; set; }

        public long CurrentKill { get; set; }

        public short CurrentMinigame { get; set; }

        public long CurrentTc { get; set; }

        public int DarkResistance { get; set; }

        public int Defence { get; set; }

        public int DefenceRate { get; set; }

        public byte Direction { get; set; }

        public int DistanceDefence { get; set; }

        public int DistanceDefenceRate { get; set; }

        public IDisposable DragonModeObservable { get; set; }

        public ThreadSafeGenericList<BCard> EffectFromTitle { get; set; }

        public ThreadSafeGenericList<BCard> VisualFromTitle { get; set; }

        public byte Element { get; set; }

        public int ElementRate { get; set; }

        public int ElementRateSP { get; internal set; }

        public List<EnergyField> EnergyFields { get; set; }

        public List<BattlePassItemLogsDTO> BattlePassItemLogs { get; set; }

        public List<BattlePassQuestLogsDTO> BattlePassQuestLogs { get; set; }

        public ThreadSafeGenericLockedList<BCard> EquipmentBCards => BattleEntity.BCards;

        public ExchangeInfo ExchangeInfo { get; set; }

        public Family Family { get; set; }

        public FamilyCharacterDTO FamilyCharacter => Family?.FamilyCharacters.Find(s => s.CharacterId == CharacterId);

        public ThreadSafeGenericList<long> FamilyInviteCharacters { get; set; }

        public int FireResistance { get; set; }

        public int FoodAmount { get; set; }

        public int FoodHp { get; set; }

        public int FoodMp { get; set; }

        public ThreadSafeGenericList<long> FriendRequestCharacters { get; set; }

        public ThreadSafeGenericList<GeneralLogDTO> GeneralLogs { get; set; }

        public bool GmPvtBlock { get; set; }

        public Group Group { get; set; }

        public ThreadSafeGenericList<long> GroupSentRequestCharacterIds { get; set; }

        public bool HasGodMode { get; set; }

        public bool HasMagicalFetters => HasBuff(608);

        public bool HasMagicSpellCombo => HasBuff(617) && (LastComboCastId >= 11 && LastComboCastId <= 15);

        public bool HasShopOpened { get; set; }

        public int HitCriticalChance { get; set; }

        public int HitCriticalRate { get; set; }

        public int HitRate { get; set; }

        public bool InExchangeOrTrade => ExchangeInfo != null || Speed == 0;

        public Inventory Inventory { get; set; }

        public bool Invisible { get; set; }

        public bool InvisibleGm { get; set; }

        public bool IsChangingMapInstance { get; set; }

        public bool IsCustomSpeed { get; set; }

        public bool IsDancing { get; set; }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Defines if the Character Is currently sending or getting items thru exchange.
        /// </summary>
        public bool IsExchanging { get; set; }

        public bool IsFrozen { get; set; }

        public bool IsLocked { get; set; }

        public bool IsMarried => CharacterRelations.Any(c => c.RelationType == CharacterRelationType.Spouse);

        public bool IsMorphed { get; set; }

        public bool IsShopping { get; set; }

        public bool IsSitting { get; set; }

        public bool IsUsingFairyBooster => BuffObservables.ContainsKey(131);

        public bool IsVehicled { get; set; }

        public bool IsWaitingForEvent { get; set; }

        public int LastComboCastId { get; set; }

        public DateTime LastDefence { get; set; }

        public DateTime LastDelay { get; set; }

        public DateTime LastDeposit { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastFreeze { get; set; }

        public DateTime LastFunnelUse { get; set; }

        public DateTime LastHealth { get; set; }

        public DateTime LastArenaHealth { get; set; }

        public bool CanGetNewBuffElement { get; set; }

        public short LastItemVNum { get; set; }

        public DateTime LastLoyalty { get; set; }

        public DateTime LastMapObject { get; set; }

        public DateTime LastMonsterAggro { get; set; }

        public DateTime LastMove { get; set; }

        public int LastNpcMonsterId { get; set; }

        public int LastNRunId { get; set; }

        public DateTime LastPermBuffRefresh { get; set; }

        public double LastPortal { get; set; }

        public DateTime LastPotion { get; set; }

        public DateTime LastPulse { get; set; }

        public ClientSession LastPvPKiller { get; set; }

        public DateTime LastPVPRevive { get; set; }

        public DateTime LastQuest { get; set; }

        public DateTime LastQuestSummon { get; set; }

        public DateTime LastSkillComboUse { get; set; }

        public DateTime LastSkillUse { get; set; }

        public double LastSp { get; set; }

        public DateTime LastSpeedChange { get; set; }

        public DateTime LastSpGaugeRemove { get; set; }

        public DateTime LastTransform { get; set; }

        public DateTime LastVessel { get; set; }

        public DateTime LastWithdraw { get; set; }

        public IDisposable Life { get; set; }

        public int LightResistance { get; set; }

        public int MagicalDefence { get; set; }

        public ConcurrentDictionary<int, MailDTO> MailList { get; set; }

        public MapInstance MapInstance => ServerManager.GetMapInstance(MapInstanceId);

        public Guid MapInstanceId { get; set; }

        public ThreadSafeGenericList<long> MarryRequestCharacters { get; set; }

        public ThreadSafeGenericList<Mate> Mates { get; set; }

        public int MaxFood { get; set; }

        public int MaxHit { get; set; }

        public int MaxSnack { get; set; }

        public Dictionary<short, DateTime> MeditationDictionary { get; set; }

        public int MessageCounter { get; set; }

        public int MinHit { get; set; }

        public MinigameLogDTO MinigameLog { get; set; }

        public MapInstance Miniland { get; private set; }

        public List<MinilandObject> MinilandObjects { get; set; }

        public int Morph { get; set; }

        public int MorphUpgrade { get; set; }

        public int MorphUpgrade2 { get; set; }

        public ConcurrentStack<MTListHitTarget> MTListTargetQueue { get; set; }

        public bool NoAttack { get; set; }

        public bool NoMove { get; set; }

        public List<EventContainer> OnDeathEvents => BattleEntity.OnDeathEvents;

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public int PreviousMorph { get; set; }

        public object PVELockObject { get; set; }

        public bool PyjamaDead { get; set; }

        public ConcurrentBag<CharacterQuest> Quests { get; internal set; }

        public List<QuicklistEntryDTO> QuicklistEntries { get; private set; }

        public bool CanCreatePrivateArena { get; set;}

        public byte SpRemoveConfirmation { get; set; }

        public RespawnMapTypeDTO Respawn
        {
            get
            {
                RespawnMapTypeDTO respawn = new RespawnMapTypeDTO
                {
                    DefaultX = 79,
                    DefaultY = 116,
                    DefaultMapId = 1,
                    RespawnMapTypeId = -1
                };

                if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Count > 0)
                {
                    long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes[0].RespawnMapTypeId;
                    if (respawnmaptype != null)
                    {
                        RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == respawnmaptype);
                        if (resp == null)
                        {
                            RespawnMapTypeDTO defaultresp = Session.CurrentMapInstance.Map.DefaultRespawn;
                            if (defaultresp != null)
                            {
                                respawn.DefaultX = defaultresp.DefaultX;
                                respawn.DefaultY = defaultresp.DefaultY;
                                respawn.DefaultMapId = defaultresp.DefaultMapId;
                                respawn.RespawnMapTypeId = (long)respawnmaptype;
                            }
                        }
                        else
                        {
                            respawn.DefaultX = resp.X;
                            respawn.DefaultY = resp.Y;
                            respawn.DefaultMapId = resp.MapId;
                            respawn.RespawnMapTypeId = (long)respawnmaptype;
                        }
                    }
                }
                else if (Session.HasCurrentMapInstance)
                {
                    RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == 0);
                    if (resp != null)
                    {
                        respawn.DefaultX = resp.X;
                        respawn.DefaultY = resp.Y;
                        respawn.DefaultMapId = resp.MapId;
                        respawn.RespawnMapTypeId = (long)1;
                    }
                }
                return respawn;
            }
        }

        public List<RespawnDTO> Respawns { get; set; }

        public RespawnMapTypeDTO Return
        {
            get
            {
                RespawnMapTypeDTO respawn = new RespawnMapTypeDTO();
                if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Count > 0)
                {
                    long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes[0].ReturnMapTypeId;
                    if (respawnmaptype != null)
                    {
                        RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == respawnmaptype);
                        if (resp == null)
                        {
                            RespawnMapTypeDTO defaultresp = Session.CurrentMapInstance.Map.DefaultReturn;
                            if (defaultresp != null)
                            {
                                respawn.DefaultX = defaultresp.DefaultX;
                                respawn.DefaultY = defaultresp.DefaultY;
                                respawn.DefaultMapId = defaultresp.DefaultMapId;
                                respawn.RespawnMapTypeId = (long)respawnmaptype;
                            }
                        }
                        else
                        {
                            respawn.DefaultX = resp.X;
                            respawn.DefaultY = resp.Y;
                            respawn.DefaultMapId = resp.MapId;
                            respawn.RespawnMapTypeId = (long)respawnmaptype;
                        }
                    }
                }
                else if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                {
                    RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == 1);
                    if (resp != null)
                    {
                        respawn.DefaultX = resp.X;
                        respawn.DefaultY = resp.Y;
                        respawn.DefaultMapId = resp.MapId;
                        respawn.RespawnMapTypeId = 1;
                    }
                }
                return respawn;
            }
        }

        public ConcurrentBag<ShellEffectDTO> RuneEffectMain { get; set; }

        public MapCell SavedLocation { get; set; }

        public IDisposable SaveObs { get; set; }

        public short SaveX { get; set; }

        public short SaveY { get; set; }

        public byte ScPage { get; set; }

        public IDisposable SealDisposable { get; set; }

        public int SecondWeaponCriticalChance { get; set; }

        public int SecondWeaponCriticalRate { get; set; }

        public int SecondWeaponHitRate { get; set; }

        public int SecondWeaponMaxHit { get; set; }

        public int SecondWeaponMinHit { get; set; }

        public ClientSession Session { get; private set; }

        public ConcurrentBag<ShellEffectDTO> ShellEffectArmor { get; set; }

        public ConcurrentBag<ShellEffectDTO> ShellEffectMain { get; set; }

        public ConcurrentBag<ShellEffectDTO> ShellEffectSecondary { get; set; }

        public int Size { get; set; } = 10;

        public byte SkillComboCount { get; set; }

        public ThreadSafeSortedList<int, CharacterSkill> Skills { get; private set; }

        public ThreadSafeSortedList<int, CharacterSkill> SkillsSp { get; set; }

        public int SnackAmount { get; set; }

        public int SnackHp { get; set; }

        public int SnackMp { get; set; }

        public int SpCooldown { get; set; }

        public bool IsInArenaLobby { get; set; }

        public bool IsInFightZone { get; set; }

        public byte Speed
        {
            get
            {
                if (_speed > 59)
                {
                    return 59;
                }
                return _speed;
            }

            set
            {
                LastSpeedChange = DateTime.Now;
                _speed = value > 59 ? (byte)59 : value;
            }
        }

        public object SpeedLockObject { get; set; }

        public ItemInstance SpInstance => Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);

        public ThreadSafeGenericLockedList<StaticBonusDTO> StaticBonusList { get; set; }

        public ScriptedInstance Timespace { get; set; }

        public ScriptedInstance SkyTower { get; set; }

        public bool TimespaceRewardGotten { get; set; }

        public int TimesUsed { get; set; }

        public List<CharacterTitleDTO> Title { get; set; }

        public ThreadSafeGenericList<long> TradeRequests { get; set; }

        public int UltimatePoints { get; set; }

        public bool Undercover { get; set; }

        public bool UseSp { get; set; }

        public Item VehicleItem { get; set; }

        public byte VehicleSpeed { private get; set; }

        public IDisposable WalkDisposable { get; set; }

        public List<long> LastKilledChars { get; set; }

        public int WareHouseSize
        {
            get
            {
                MinilandObject mp = MinilandObjects.Where(s => s.ItemInstance.Item.ItemType == ItemType.House && s.ItemInstance.Item.ItemSubType == 2).OrderByDescending(s => s.ItemInstance.Item.MinilandObjectPoint).FirstOrDefault();
                if (mp != null)
                {
                    return mp.ItemInstance.Item.MinilandObjectPoint;
                }
                return 0;
            }
        }

        public int WaterResistance { get; set; }

        #endregion

        #region Methods

        public bool IsBattlePassQuestExpired(long questId)
        {
            var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.Id == questId);

            if (quest == null) return true;
            
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

            return timeSpan.TotalMinutes <= 0;
        }

        public void ResetBattlePassQuestObjectives(long questId, int newValue)
        {
            var battlePassQuest = BattlePassQuestLogs.Find(s => s.QuestId == questId);

            if (battlePassQuest == null) return;

            var index = BattlePassQuestLogs.IndexOf(battlePassQuest);
            BattlePassQuestLogs[index].Advancement = newValue;
            Session.SendPacket(this.GenerateBpm());
        }

        public void IncreaseBattlePassQuestObjectives(long questId, int toAdd)
        {
            var battlePassQuest = BattlePassQuestLogs.Find(s => s.QuestId == questId);
            var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.Id == battlePassQuest.QuestId);

            bool canContinue = true;
            switch (battlePassQuest.Type)
            {
                case BattlePassQuestType.Daily:
                case BattlePassQuestType.Weekly:
                    canContinue = BattlePassQuestLogs.Count(s => s.Type == quest.TaskType) >= 5;
                    break;

                case BattlePassQuestType.Season:
                    canContinue = BattlePassQuestLogs.Count(s => s.Type == BattlePassQuestType.Season) >= 7;
                    break;
            }

            if (!canContinue) return;

            if (battlePassQuest == null)
            {
                BattlePassQuestLogs.Add(new BattlePassQuestLogsDTO
                {
                    Id = Guid.NewGuid(),
                    CharacterId = CharacterId,
                    QuestId = questId,
                    Advancement = toAdd,
                    Type = quest.TaskType
                });
                Session.SendPacket(this.GenerateBpm());
                return;
            }

            if (battlePassQuest.Advancement + toAdd > quest.MaxObjectiveValue) toAdd = int.MaxValue;

            var index = BattlePassQuestLogs.IndexOf(battlePassQuest);
            BattlePassQuestLogs[index].Advancement = toAdd == int.MaxValue ? quest.MaxObjectiveValue : battlePassQuest.Advancement + toAdd;
            Session.SendPacket(this.GenerateBpm());
        }

        public void Lock()
        {
            if (IsLocked)
            {
                return;
            }
            IsLocked = true;
            NoMove = true;
            NoAttack = true;
            Session?.SendPacket(this.GenerateCond());
        }

        public void RemoveLock()
        {
            if (!IsLocked)
            {
                return;
            }

            IsLocked = false;
            NoMove = false;
            NoAttack = false;
            Session?.SendPacket(this.GenerateCond());
        }

        public static void InsertOrUpdatePenalty(PenaltyLogDTO log)
        {
            DAOFactory.PenaltyLogDAO.InsertOrUpdate(ref log);
            CommunicationServiceClient.Instance.RefreshPenalty(log.PenaltyLogId);
        }

        public void AddMate(short mateId, byte level, MateType mateType)
        {
            Mate equipedMate = Session.Character.Mates?.SingleOrDefault(s => s.IsTeamMember && s.MateType == mateType);

            if (equipedMate != null)
            {
                equipedMate.RemoveTeamMember();
                Session.Character.MapInstance.Broadcast(equipedMate.GenerateOut());
            }

            Mate mate = new Mate(Session.Character, ServerManager.GetNpcMonster(mateId), level, mateType);
            Session.Character.Mates?.Add(mate);
            mate.RefreshStats();
            Session.SendPacket($"ctl 2 {mate.PetId} 3");
            Session.Character.MapInstance.Broadcast(mate.GenerateIn());
            Session.SendPacket(UserInterfaceHelper.GeneratePClear());
            Session.SendPackets(Session.Character.GenerateScP());
            Session.SendPackets(Session.Character.GenerateScN());
            Session.SendPacket(Session.Character.GeneratePinit());
            Session.SendPackets(Session.Character.Mates.Where(s => s.IsTeamMember).OrderBy(s => s.MateType).Select(s => s.GeneratePst()));
        }

        public void AddBuff(Buff indicator, BattleEntity sender, bool noMessage = false, short x = 0, short y = 0) => BattleEntity.AddBuff(indicator, sender, noMessage, x, y);

        public void ReplacePet(Mate oldMate, short newVnum, byte newPetLevel)
        {
            var mate = Mates.GetAllItems().FirstOrDefault(s => s.MateTransportId == oldMate.MateTransportId && s.MateType == oldMate.MateType);

            if (mate == null)
            {
                return;
            }

            var inventory = mate.GetInventory();

            if (inventory.Any())
            {
                return;
            }

            if (mate.IsTeamMember)
            {
                mate.BackToMiniland();
            }

            Mates.Remove(oldMate);
            byte i = 0;
            Mates.Where(s => s.MateType == MateType.Partner).ToList().ForEach(s =>
            {
                s.GetInventory().ForEach(item => item.Type = (InventoryType)(13 + i));
                s.PetId = i;
                i++;
            });

            Session.SendPacket(UserInterfaceHelper.GeneratePClear());
            Session.SendPackets(this.GenerateScP());
            Session.SendPackets(this.GenerateScN());
            Session.CurrentMapInstance?.Broadcast(mate.GenerateOut());
            AddPet(new Mate(this, ServerManager.GetNpcMonster(newVnum), newPetLevel, mate.MateType), true);
            Session.SendPacket(this.GeneratePinit());
            Session.SendPackets(this.GeneratePst());
        }

        public bool AddPet(Mate mate, bool addToTeam = false)
        {
            if (CanAddMate(mate) || mate.IsTemporalMate)
            {
                Mates.Add(mate);
                MapInstance.Broadcast(mate.GenerateIn());
                if (!mate.IsTemporalMate)
                {
                    Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_GET_PET"), mate.Name), 12));
                }
                Session.SendPacket(UserInterfaceHelper.GeneratePClear());
                Session.SendPackets(this.GenerateScP());
                Session.SendPackets(this.GenerateScN());
                Session.SendPackets(this.GenerateScN());
                mate.RefreshStats();

                if (addToTeam)
                {
                    mate.AddTeamMember();
                }
                return true;
            }
            return false;
        }

        public void AddPetWithSkill(Mate mate)
        {
            if (mate == null)
            {
                return;
            }
            bool isUsingMate = true;
            if (!Mates.ToList().Any(s => s.IsTeamMember && s.MateType == mate.MateType))
            {
                isUsingMate = false;
                mate.IsTeamMember = true;
            }
            else
            {
                mate?.BackToMiniland();
            }
            Session.SendPacket($"ctl 2 {mate.MateTransportId} 3");
            Mates.Add(mate);
            Session.SendPacket(UserInterfaceHelper.GeneratePClear());
            Session.SendPackets(this.GenerateScP());
            Session.SendPackets(this.GenerateScN());
            if (!isUsingMate)
            {
                Parallel.ForEach(Session?.CurrentMapInstance?.Sessions?.Where(s => s?.Character != null), s =>
                {
                    if (ServerManager.Instance.ChannelId != 51 || Session?.Character?.Faction == s?.Character?.Faction)
                    {
                        s?.SendPacket(mate?.GenerateIn(false, ServerManager.Instance.ChannelId == 51));
                    }
                    else
                    {
                        s?.SendPacket(mate?.GenerateIn(true, ServerManager.Instance.ChannelId == 51, s.Account.Authority));
                    }
                });
                Session?.SendPacket(this.GeneratePinit());
                Session?.SendPacket(UserInterfaceHelper.GeneratePClear());
                Session?.SendPackets(this.GenerateScP());
                Session?.SendPackets(this.GenerateScN());
                Session?.SendPackets(this.GeneratePst());
            }
        }

        public bool AddQuest(long questId, bool isMain = false)
        {
            var characterQuest = new CharacterQuest(questId, CharacterId);

            int QuestLimit = Quests.Any(q => q.QuestId == 5981) ? 8 : 7;

            if (Quests.Any(q => q.QuestId == questId) || characterQuest.Quest == null ||
                isMain && Quests.Any(q => q.IsMainQuest) || characterQuest == null ||
                Quests.Where(q => q.Quest.QuestType != (byte)QuestType.WinRaid).ToList().Count >= QuestLimit &&
                characterQuest.Quest.QuestType != (byte)QuestType.WinRaid && !isMain)
            {
                return false;
            }

            if (Quests.Count >= QuestLimit && characterQuest.Quest.QuestId != 5981) // Flower quest hardcode ftw
            {
                // to many quest
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_MANY_QUEST"), 0));
                return false;
            }

            if (characterQuest.Quest.LevelMin > Level)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_LOW_LVL"), 0));
                return false;
            }

            /*
            if (characterQuest.Quest.LevelMax < Level)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_HIGH_LVL"), 0));
                return false;
            }
            */

            bool isSpQuest = false;

            if ((questId >= 2000 && questId <= 2007) // Pajama
                || (questId >= 2008 && questId <= 2013) // SP 1
                || (questId >= 2014 && questId <= 2020) // SP 2
                || (questId >= 2060 && questId <= 2095) // SP 3
                || (questId >= 2100 && questId <= 2134) // SP 4
                )
            {
                isSpQuest = true;
            }

            if (!isSpQuest)
            {
                var exists = QuestLogs.Any(s => s.QuestId == questId);

                if (questId == 22000 && exists ||
                    questId == 22001 && exists ||
                    questId == 22002 && exists)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("QUEST_ALREADY_DONE_LIFETIME"), 0));
                    return false;
                }

                if ((!characterQuest.Quest.IsDaily && !characterQuest.IsMainQuest && (QuestType)characterQuest.Quest.QuestType != QuestType.FlowerQuest))
                {
                    if (exists)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("QUEST_ALREADY_DONE"), 0));
                        return false;
                    }
                }
                else if (CharacterId != 66556 && characterQuest.Quest.IsDaily && (QuestType)characterQuest.Quest.QuestType != QuestType.FlowerQuest)
                {
                    var list = new List<long> { 7524, 6728, 7607, 6729, 7630, 7627, 6741, 6742, 6743, 6723 };
                    if (Session.Character.QuestLogs.Any(s => s.QuestId == questId && s.LastDaily != null && (s.LastDaily.Value.AddSeconds(-1) <= (list.Contains(questId) ? 
                                                     DateTime.Now.Subtract(DateTime.Now.TimeOfDay) :
                                                    DateTime.Now.Subtract(DateTime.Now.TimeOfDay)) == false)))
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey(list.Contains(questId) ? "QUEST_ALREADY_DONE_WEEK" : "QUEST_ALREADY_DONE_TODAY"), 0));
                        return false;
                    }
                }
            }

            if (characterQuest.Quest.QuestType == (int)QuestType.TimesSpace && ServerManager.Instance.TimeSpaces.All(si => si.QuestTimeSpaceId != (characterQuest.Quest.QuestObjectives.FirstOrDefault()?.SpecialData ?? -1)
                || characterQuest.Quest.QuestType == (int)QuestType.Product || characterQuest.Quest.QuestType == (int)QuestType.Collect3
                || characterQuest.Quest.QuestType == (int)QuestType.TransmitGold || characterQuest.Quest.QuestType == (int)QuestType.TsPoint || characterQuest.Quest.QuestType == (int)QuestType.NumberOfKill
                || characterQuest.Quest.QuestType == (int)QuestType.TargetReput || characterQuest.Quest.QuestType == (int)QuestType.Inspect || characterQuest.Quest.QuestType == (int)QuestType.Needed
                || characterQuest.Quest.QuestType == (int)QuestType.Collect5 || QuestHelper.Instance.SkipQuests.Any(q => q == characterQuest.QuestId)))
            {
                Session.SendPacket(characterQuest.Quest.GetRewardPacket(this, true));
                AddQuest(characterQuest.Quest.NextQuestId ?? -1, isMain);
                return false;
            }

            if (characterQuest.Quest.TargetMap != null)
            {
                Session.SendPacket(characterQuest.Quest.TargetPacket());
            }

            characterQuest.IsMainQuest = isMain;
            Quests.Add(characterQuest);
            Session.SendPacket(this.GenerateQuestsPacket(questId));
            if (characterQuest.Quest.QuestType == (int)QuestType.UnKnow)
            {
                Session.Character.IncrementObjective(characterQuest, isOver: true);
            }

            //Session.SendPacket(GetSqst());
            return true;
        }

        public void AddRelation(long characterId, CharacterRelationType Relation)
        {
            if (characterId == CharacterId)
            {
                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("CANT_RELATION_YOURSELF"), 11));
            }
            CharacterRelationDTO addRelation = new CharacterRelationDTO
            {
                CharacterId = CharacterId,
                RelatedCharacterId = characterId,
                RelationType = Relation
            };

            DAOFactory.CharacterRelationDAO.InsertOrUpdate(ref addRelation);
            ServerManager.Instance.RelationRefresh(addRelation.CharacterRelationId);
            Session.SendPacket(this.GenerateFinit());
            ClientSession target = ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.CharacterId == characterId);
            target?.SendPacket(target?.Character.GenerateFinit());
        }

        public bool AddSkill(short skillVNum)
        {
            Skill skillinfo = ServerManager.GetSkill(skillVNum);
            if (skillinfo == null)
            {
                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_DOES_NOT_EXIST"), 11));
                return false;
            }
            if (skillinfo.SkillVNum < 200)
            {
                if (Skills.GetAllItems()
                    .Any(s => skillinfo.CastId == s.Skill.CastId && s.Skill.SkillVNum < 200 && s.Skill.UpgradeSkill > skillinfo.UpgradeSkill))
                {
                    // Character already has a better passive skill of the same type.
                    return false;
                }
                foreach (CharacterSkill skill in Skills.GetAllItems().Where(s => skillinfo.CastId == s.Skill.CastId && s.Skill.SkillVNum < 200))
                {
                    Skills.Remove(skill.SkillVNum);
                }
            }
            else
            {
                if (Skills.ContainsKey(skillVNum))
                {
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("SKILL_ALREADY_EXIST"), 11));
                    return false;
                }

                if (skillinfo.UpgradeSkill != 0)
                {
                    CharacterSkill oldupgrade = Skills.FirstOrDefault(s =>
                        s.Skill.UpgradeSkill == skillinfo.UpgradeSkill
                        && s.Skill.UpgradeType == skillinfo.UpgradeType && s.Skill.UpgradeSkill != 0);
                    if (oldupgrade != null)
                    {
                        Skills.Remove(oldupgrade.SkillVNum);
                    }
                }
            }
            Skills[skillVNum] = new CharacterSkill
            {
                SkillVNum = skillVNum,
                CharacterId = CharacterId
            };

            Session.SendPackets(this.GenerateQuicklist());
            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
            return true;
        }

        public void AddStaticBuff(StaticBuffDTO staticBuff, bool isPermaBuff = false, bool stack = false)
        {
            Buff bf = new Buff(staticBuff.CardId, Level, isPermaBuff)
            {
                Start = DateTime.Now,
                StaticBuff = true
            };
            Buff oldbuff = Buff[staticBuff.CardId];
            if (oldbuff != null)
            {
                oldbuff.Card.BCards.Where(s => BattleEntity.BCardDisposables[s.BCardId] != null).ToList().ForEach(b => BattleEntity.BCardDisposables[b.BCardId].Dispose());
                oldbuff.StaticVisualEffect?.Dispose();
            }
            if (staticBuff.RemainingTime <= 0)
            {
                bf.RemainingTime = (int)(bf.Card.Duration * 0.6);
                Buff[bf.Card.CardId] = bf;
            }
            else if (staticBuff.RemainingTime > 0)
            {
                bf.RemainingTime = stack ? staticBuff.RemainingTime + (oldbuff == null ? 0 : (int)(oldbuff.RemainingTime - (DateTime.Now - oldbuff.Start).TotalSeconds)) : staticBuff.RemainingTime;
                Buff[bf.Card.CardId] = bf;
            }
            else if (oldbuff != null)
            {
                Buff.Remove(bf.Card.CardId);
                int time = (int)Math.Abs(((oldbuff.Start.AddSeconds(oldbuff.Card.Duration * 6 / 10) - DateTime.Now).TotalSeconds / 10 * 6));
                bf.RemainingTime = (bf.Card.Duration * 6 / 10) + time;
                Buff[bf.Card.CardId] = bf;
            }
            else
            {
                bf.RemainingTime = bf.Card.Duration * 6 / 10;
                Buff[bf.Card.CardId] = bf;
            }
            bf.Card.BCards.ForEach(c => c.ApplyBCards(BattleEntity, BattleEntity));
            if (BuffObservables.ContainsKey(bf.Card.CardId))
            {
                BuffObservables[bf.Card.CardId].Dispose();
                BuffObservables.Remove(bf.Card.CardId);
            }
            if (bf.RemainingTime > 0)
            {
                BuffObservables[bf.Card.CardId] = Observable.Timer(TimeSpan.FromSeconds(bf.RemainingTime)).SafeSubscribe(o =>
                {
                    RemoveBuff(bf.Card.CardId);
                    if (bf.Card.TimeoutBuff != 0 && ServerManager.RandomNumber() < bf.Card.TimeoutBuffChance)
                    {
                        AddBuff(new Buff(bf.Card.TimeoutBuff, Level), BattleEntity);
                    }
                });
            }
            if (!_isStaticBuffListInitial)
            {
                _isStaticBuffListInitial = true;
            }

            Session.SendPacket($"vb {bf.Card.CardId} 1 {(bf.RemainingTime <= 0 ? -1 : bf.RemainingTime * 10)}");
            Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("UNDER_EFFECT"), bf.Card.Name), 12));

            // Visual Effects (eff packet)
            if (bf.Card.CardId == 319)
            {
                bf.StaticVisualEffect = Observable.Interval(TimeSpan.FromSeconds(2)).SafeSubscribe(o =>
                {
                    if (!Invisible)
                    {
                        Session?.CurrentMapInstance?.Broadcast(this.GenerateEff(881));
                    }
                });
            }
        }

        public void AddUltimatePoints(short points)
        {
            UltimatePoints += points;

            if (UltimatePoints > 3000)
            {
                UltimatePoints = 3000;
            }

            Session.SendPacket(this.GenerateFtPtPacket());
            Session.SendPackets(this.GenerateQuicklist());
        }

        public void AddWolfBuffs()
        {
            if (UltimatePoints >= 1000 && !Buff.Any(s => s.Card.CardId == 727 || s.Card.CardId == 728 || s.Card.CardId == 729))
            {
                AddBuff(new Buff(727, 10, false), BattleEntity);
                RemoveBuff(728);
                RemoveBuff(729);
            }

            if (UltimatePoints >= 2000 && !Buff.Any(s => s.Card.CardId == 728 || s.Card.CardId == 729))
            {
                AddBuff(new Buff(728, 10, false), BattleEntity);
                RemoveBuff(727);
                RemoveBuff(729);
            }

            if (UltimatePoints >= 3000 && !Buff.Any(s => s.Card.CardId == 729))
            {
                AddBuff(new Buff(729, 10, false), BattleEntity);
                RemoveBuff(727);
                RemoveBuff(728);
            }
        }

        public bool CanAddMate(Mate mate) => mate.MateType == MateType.Pet ? MaxMateCount > Mates.ToList().Count(s => s.MateType == MateType.Pet) : MaxPartnerCount > Mates.ToList().Count(s => s.MateType == MateType.Partner);

        public bool CanAttack() => !NoAttack && !HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack) && !HasBuff(CardType.FrozenDebuff, (byte)AdditionalTypes.FrozenDebuff.EternalIce);

        public bool CanMove() => !NoMove && !HasBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible) && !HasBuff(CardType.FrozenDebuff, (byte)AdditionalTypes.FrozenDebuff.EternalIce);

        public bool CanUseNosBazaar()
        {
            if (MapInstance == null)
            {
                return false;
            }

            StaticBonusDTO medal = Session.Character.StaticBonusList.Find(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

            if (medal == null)
            {
                // Check if there is NosBazaar in Map
                if (!Session.Character.MapInstance.Npcs.Any(s => s.NpcVNum == 793 || s.NpcVNum == 3081))
                {
                    return false;
                }
            }

            return true;
        }

        public void ChangeChannel(string ip, int port, byte mode)
        {
            Session.SendPacket($"mz {ip} {port} {Slot}");
            Session.SendPacket($"it {mode}");
            Session.IsDisposing = true;
            CommunicationServiceClient.Instance.RegisterCrossServerAccountLogin(Session.Account.AccountId, Session.SessionId);

            //explictly save data before disconnecting to prevent data loss
            Save();

            Session.Disconnect();
        }

        public void ChangeClass(ClassType characterClass, bool fromCommand)
        {
            if (!fromCommand)
            {
                JobLevel = 80;
                JobLevelXp = 0;
            }
            Session.SendPacket("npinfo 0");
            Session.SendPacket(UserInterfaceHelper.GeneratePClear());

            if (characterClass == (byte)ClassType.Adventurer)
            {
                HairStyle = (byte)HairStyle > 1 ? 0 : HairStyle;
                if (JobLevel > 20)
                {
                    JobLevel = 20;
                }
            }
            LoadSpeed();
            Class = characterClass;
            Hp = (int)HPLoad();
            Mp = (int)MPLoad();
            Session.SendPacket(this.GenerateTit());
            Session.SendPacket(this.GenerateStat());
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateEq());
            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 8), PositionX, PositionY);
            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CLASS_CHANGED"), 0));
            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 196), PositionX, PositionY);

            ChangeFaction(Session.Character.Family == null ? (FactionType)ServerManager.RandomNumber(1, 2) : (FactionType)Session.Character.Family.FamilyFaction);

            Session.SendPacket(this.GenerateCond());
            Session.SendPacket(this.GenerateLev());
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateCMode());
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateGidx(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 6), PositionX, PositionY);
            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 198), PositionX, PositionY);
            Session.Character.ResetSkills();

            foreach (QuicklistEntryDTO quicklists in DAOFactory.QuicklistEntryDAO.LoadByCharacterId(CharacterId).Where(quicklists => QuicklistEntries.Any(qle => qle.Id == quicklists.Id)))
            {
                DAOFactory.QuicklistEntryDAO.Delete(quicklists.Id);
            }

            QuicklistEntries = new List<QuicklistEntryDTO>
            {
                new QuicklistEntryDTO
                {
                    CharacterId = CharacterId,
                    Q1 = 0,
                    Q2 = 9,
                    Type = 1,
                    Slot = 3,
                    Pos = 1
                }
            };

            Session.SendPackets(this.GenerateQuicklist());

            if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(Session) && s.GroupType == GroupType.Group))
            {
                Session.CurrentMapInstance?.Broadcast(Session, $"pidx 1 1.{CharacterId}", ReceiverType.AllExceptMe);
            }
        }

        public void ChangeFaction(FactionType faction)
        {
            if (Faction == faction)
            {
                return;
            }

            if (Channel.ChannelId == 51)
            {
                Session.SendPacket(Session.Character.GenerateSay($"You cannot change faction if you're in act4", 10));
                return;
            }

            Faction = faction;
            Act4Kill = 0;
            Act4Dead = 0;
            Act4Points = 0;
            Session.SendPacket("scr 0 0 0 0 0 0");
            Session.SendPacket(this.GenerateFaction());
            Session.SendPackets(this.GenerateStatChar());

            if (faction != FactionType.None)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{(int)Faction}"), 0));
                var effectId = 4799 + (int)faction;
                if (Family != null)
                {
                    effectId += 2;
                }

                Session.SendPacket(this.GenerateEff(effectId));
            }
            Session.SendPacket(this.GenerateCond());
            Session.SendPacket(this.GenerateLev());
        }

        public void ChangeSex()
        {
            Gender = Gender == GenderType.Female ? GenderType.Male : GenderType.Female;
            if (IsVehicled)
            {
                Morph = Gender == GenderType.Female ? Morph + 1 : Morph - 1;
            }
            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SEX_CHANGED"), 0));
            Session.SendPacket(this.GenerateEq());
            Session.SendPacket(this.GenerateGender());
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateIn(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateGidx(), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(this.GenerateCMode());
            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 196), PositionX, PositionY);
        }

        public void CharacterLife()
        {
            if (Session == null) // Can happen, cause thanks observables 
            {
                return;
            }

            if (Session.Character?.MapInstance?.Sessions?.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) <= 1)
            {
                if (AliveCountdown < 604800)
                {
                    AliveCountdown += 0.0003;
                }
                else
                {
                    AliveCountdown = 0;
                    Session.Character.Inventory.AddNewToInventory(20001);
                }
            }

            if (Session.Character.HasBuff(697))
            {
                Session.Character.GenerateQuicklist();
                Session.Character.GenerateQuicklist();
            }

            if (MapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance)
            {
                var effect = 0;
                if (Group != null)
                {
                    var rainbowTeam = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));

                    if (rainbowTeam != null)
                    {
                        var teamType = rainbowTeam.TeamEntity;

                        switch (teamType)
                        {
                            case RainbowTeamBattleType.Blue:
                                effect = (short)RainbowBattleTeamColour.Blue;
                                break;
                            case RainbowTeamBattleType.Red:
                                effect = (short)RainbowBattleTeamColour.Red;
                                break;
                        }

                        if (LastRbbEffect.AddSeconds(3) < DateTime.Now && effect != 0)
                        {
                            MapInstance.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, effect));
                            LastRbbEffect = DateTime.Now;
                        }

                        if (Session.Character.HasBuff(633))
                        {
                            Session.Character.NoMove = false;
                            Session.Character.NoAttack = false;
                            Session.Character.IsFrozen = false;
                        }
                    }
                }
            }

            if ((Session.Character.MapId == 2010 && Session.Character.Group.GroupType == GroupType.RBBRed && ((int)Session.Character.MapX).IsBetween(30, 35) && ((int)Session.Character.MapY).IsBetween(73, 78)) || (Session.Character.MapId == 2010 && Session.Character.Group.GroupType == GroupType.RBBBlue && ((int)Session.Character.MapX).IsBetween(83, 88) && ((int)Session.Character.MapY).IsBetween(2, 7)))
            {
                bool change = false;
                var calculationHp = (int)Math.Round(Session.Character.HPLoad() * 0.25);
                var calculationMp = (int)Math.Round(Session.Character.MPLoad() * 0.25);

                if (Session.Character.Hp < Session.Character.HPLoad() || Session.Character.Mp < Session.Character.MPLoad() && LastHealth.AddSeconds(1) <= DateTime.Now)
                {
                    Session.Character.Hp += calculationHp;
                    Session.Character.Mp += calculationMp;
                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 54), Session.Character.PositionX, Session.Character.PositionY);
                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 55), Session.Character.PositionX, Session.Character.PositionY);
                    change = true;
                }

                if (change == true)
                {
                    Session.Character.MapInstance?.Broadcast(Session.Character.GenerateRc(calculationHp));
                    Session.Character?.Session?.SendPacket(Session.Character?.GenerateStat());
                    LastHealth = DateTime.Now;
                }
            }

            if (MapInstance.MapInstanceId == ServerManager.Instance.ArenaInstance.MapInstanceId && MapInstance.Map.Tiles[Session.Character.PositionX, Session.Character.PositionY].Value != 0 && (MapInstance.Map.Tiles[Session.Character.PositionX, Session.Character.PositionY].Value != 16 || Session.Character.PositionY == 35))
            {
                if (ArenaCooldown == 30)
                {
                    StairsDisposable = Observable.Timer(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                    {
                        ArenaCooldown -= 1;
                    });
                }

                if (ArenaCooldown <= 0)
                {
                    StairsDisposable?.Dispose();

                    StairsDisposable = Observable.Interval(TimeSpan.FromSeconds(2)).SafeSubscribe(s =>
                    {
                        var calculationHp = (int)Math.Round(Session.Character.HPLoad() * 0.05);
                        var calculationMp = (int)Math.Round(Session.Character.MPLoad() * 0.05);

                        if (Session.Character.Hp < Session.Character.HPLoad() || Session.Character.Mp < Session.Character.MPLoad())
                        {
                            Session.Character.Hp += calculationHp;
                            Session.Character.Mp += calculationMp;
                            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 54), Session.Character.PositionX, Session.Character.PositionY);
                            Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 55), Session.Character.PositionX, Session.Character.PositionY);
                            Session.Character.MapInstance?.Broadcast(Session.Character.GenerateRc(calculationHp));
                            Session.Character?.Session?.SendPacket(Session.Character?.GenerateStat());
                            LastArenaHealth = DateTime.Now;
                        }
                    });
                }
            }
            else
            {
                StairsDisposable?.Dispose();
                ArenaCooldown = 30;
            }

            if (Hp == 0 && LastHealth.AddSeconds(2) <= DateTime.Now)
            {
                Mp = 0;
                Session.SendPacket(this.GenerateStat());
                LastHealth = DateTime.Now;
            }
            else
            {
                #region Tart Hapendam's Martial Arts

                /*
                if (Level >= 1 && Level < 81)
                {
                    if (!HasBuff(684))
                    {
                        AddBuff(new Buff(684, Level), BattleEntity);
                    }
                }
                else
                {
                    if (HasBuff(684))
                    {
                        RemoveBuff(684);
                    }
                }
                */

                #endregion

                if (BubbleMessage != null && BubbleMessageEnd <= DateTime.Now)
                {
                    BubbleMessage = null;
                }

                if (CurrentMinigame != 0 && LastEffect.AddSeconds(3) <= DateTime.Now)
                {
                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, CurrentMinigame));
                    LastEffect = DateTime.Now;
                }

                if (LastEffect.AddMilliseconds(400) <= DateTime.Now && MessageCounter > 0)
                {
                    MessageCounter--;
                }

                if (MapInstance != null && HasBuff(CardType.FrozenDebuff, (byte)AdditionalTypes.FrozenDebuff.EternalIce) && LastFreeze.AddSeconds(1) <= DateTime.Now)
                {
                    LastFreeze = DateTime.Now;
                    MapInstance.Broadcast(this.GenerateEff(35));
                }

                if (MapInstance == Miniland && LastLoyalty.AddSeconds(10) <= DateTime.Now)
                {
                    LastLoyalty = DateTime.Now;
                    Mates.ForEach(m =>
                    {
                        m.Loyalty += 100;
                        if (m.Loyalty > 1000) m.Loyalty = 1000;
                    });
                    Session.SendPackets(this.GenerateScP());
                    Session.SendPackets(this.GenerateScN());
                }

                var effect = VisualFromTitle.FirstOrDefault(s => s.Type == (byte)CardType.BonusBCards && s.SubType == (byte)AdditionalTypes.BonusTitleBCards.ShineBrightLikeADiamond);

                if (effect != null && effect.FirstData == 41)
                {
                    MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, effect.FirstData));
                }

                if (LastEffect.AddSeconds(5) <= DateTime.Now)
                {
                    if (effect != null)
                    {
                        MapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, effect.FirstData));
                    }

                    if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        Session.SendPacket(this.GenerateRaid(3));
                    }

                    ItemInstance ring = Inventory.LoadBySlotAndType((byte)EquipmentType.Ring, InventoryType.Wear);
                    ItemInstance bracelet = Inventory.LoadBySlotAndType((byte)EquipmentType.Bracelet, InventoryType.Wear);
                    ItemInstance necklace = Inventory.LoadBySlotAndType((byte)EquipmentType.Necklace, InventoryType.Wear);
                    CellonOptions.Clear();
                    if (ring != null)
                    {
                        CellonOptions.AddRange(ring.CellonOptions);
                    }
                    if (bracelet != null)
                    {
                        CellonOptions.AddRange(bracelet.CellonOptions);
                    }
                    if (necklace != null)
                    {
                        CellonOptions.AddRange(necklace.CellonOptions);
                    }

                    if (!Invisible)
                    {
                        ItemInstance amulet = Inventory.LoadBySlotAndType((byte)EquipmentType.Amulet, InventoryType.Wear);
                        if (amulet != null)
                        {
                            if (amulet.ItemVNum == 4503 || amulet.ItemVNum == 4504)
                            {
                                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, amulet.Item.EffectValue + (Class == ClassType.Adventurer ? 0 : (byte)Class - 1)), PositionX, PositionY);
                            }
                            else
                            {
                                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, amulet.Item.EffectValue), PositionX, PositionY);
                            }
                        }
                        if (Group != null && (Group?.GroupType == GroupType.Team || Group?.GroupType == GroupType.BigTeam || Group?.GroupType == GroupType.GiantTeam || Group?.GroupType == GroupType.LargeTeam))
                        {
                            try
                            {
                                Session.CurrentMapInstance?.Broadcast(Session, StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 828 + (Group.IsLeader(Session) ? 1 : 0)), ReceiverType.AllExceptGroup);
                                Session.CurrentMapInstance?.Broadcast(Session, StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 830 + (Group.IsLeader(Session) ? 1 : 0)), ReceiverType.Group);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log.Error(null, ex);
                            }
                        }
                        Mates.Where(s => s.CanPickUp).ToList().ForEach(s => Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, s.MateTransportId, 3007)));
                        Mates.Where(s => s.IsTsProtected).ToList().ForEach(s => Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, s.MateTransportId, 825)));
                        Mates.Where(s => s.MateType == MateType.Pet && s.Loyalty <= 0).ToList().ForEach(s => Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Npc, s.MateTransportId, 5003)));
                    }

                    LastEffect = DateTime.Now;
                }

                foreach (Mate mate in Mates?.Where(m => m.IsTeamMember && !m.IsEgg))
                {
                    if (mate != null && mate.LastHealth.AddSeconds(mate.IsSitting ? 1.5 : 2) <= DateTime.Now)
                    {
                        mate.LastHealth = DateTime.Now;
                        if (mate.LastDefence.AddSeconds(4) <= DateTime.Now && mate.LastSkillUse.AddSeconds(2) <= DateTime.Now && mate.Hp > 0)
                        {
                            mate.Hp += mate.Hp + mate.HealthHpLoad() < mate.HpLoad() ? mate.HealthHpLoad() : mate.HpLoad() - mate.Hp;
                            mate.Mp += mate.Mp + mate.HealthMpLoad() < mate.MpLoad() ? mate.HealthMpLoad() : mate.MpLoad() - mate.Mp;
                        }
                        Session.SendPackets(this.GeneratePst());
                    }
                }

                if (LastHealth.AddSeconds(2) <= DateTime.Now || (IsSitting && LastHealth.AddSeconds(1.5) <= DateTime.Now))
                {
                    LastHealth = DateTime.Now;

                    if (HealthStop)
                    {
                        HealthStop = false;
                        return;
                    }

                    if (LastDefence.AddSeconds(4) <= DateTime.Now && LastSkillUse.AddSeconds(2) <= DateTime.Now && Hp > 0)
                    {
                        bool change = false;

                        if (Hp + HealthHPLoad() < HPLoad())
                        {
                            change = true;
                            Hp += HealthHPLoad();
                        }
                        else
                        {
                            change |= Hp != (int)HPLoad();
                            Hp = (int)HPLoad();
                        }

                        if (Mp + HealthMPLoad() < MPLoad())
                        {
                            Mp += HealthMPLoad();
                            change = true;
                        }
                        else
                        {
                            change |= Mp != (int)MPLoad();
                            Mp = (int)MPLoad();
                        }

                        if (change)
                        {
                            Session.SendPacket(this.GenerateStat());
                        }
                    }
                }

                if (Session.Character.LastQuestSummon.AddSeconds(7) < DateTime.Now) // Quest in which you make monster spawn
                {
                    Session.Character.CheckHuntQuest();
                    Session.Character.LastQuestSummon = DateTime.Now;
                }

                if (MeditationDictionary.Count != 0)
                {
                    try
                    {
                        if (MeditationDictionary.ContainsKey(534) && MeditationDictionary[534] < DateTime.Now)
                        {
                            Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 4344));
                            AddBuff(new Buff(534, Level), BattleEntity);
                            if (BuffObservables.ContainsKey(533))
                            {
                                BuffObservables[533].Dispose();
                                BuffObservables.Remove(533);
                            }
                            MeditationDictionary.Remove(534);
                        }
                        else if (MeditationDictionary.ContainsKey(533) && MeditationDictionary[533] < DateTime.Now)
                        {
                            Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 4343));
                            AddBuff(new Buff(533, Level), BattleEntity);
                            if (BuffObservables.ContainsKey(532))
                            {
                                BuffObservables[532].Dispose();
                                BuffObservables.Remove(532);
                            }
                            MeditationDictionary.Remove(533);
                        }
                        else if (MeditationDictionary.ContainsKey(532) && MeditationDictionary[532] < DateTime.Now)
                        {
                            Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 4343));
                            AddBuff(new Buff(532, Level), BattleEntity);
                            if (BuffObservables.ContainsKey(534))
                            {
                                BuffObservables[534].Dispose();
                                BuffObservables.Remove(534);
                            }
                            MeditationDictionary.Remove(532);
                        }

                    }
                    catch
                    {

                    }
                }
                

                if (HasMagicSpellCombo)
                {
                    Session.SendPacket($"mslot {LastComboCastId} 0");
                }
                else if (SkillComboCount > 0 && LastSkillComboUse.AddSeconds(5) < DateTime.Now)
                {
                    SkillComboCount = 0;
                    Session.SendPacket($"mslot {LastComboCastId} 0");
                }

                if (LastPermBuffRefresh.AddSeconds(2) <= DateTime.Now)
                {
                    LastPermBuffRefresh = DateTime.Now;

                    foreach (BCard bcard in EquipmentBCards.Where(b => b.Type.Equals(CardType.Buff) && new Buff((short)b.CardId, Level).Card?.BuffType == BuffType.Good))
                    {
                        bcard.ApplyBCards(BattleEntity, BattleEntity);
                    }

                    if (UseSp)
                    {
                        ItemInstance specialist = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                        if (specialist == null)
                        {
                            return;
                        }
                        switch (specialist.WingsBuff)
                        {
                            case 3:
                                if (!Buff.ContainsKey(3042))
                                {
                                    AddBuff(new Buff(3042, Level), BattleEntity, true);
                                }
                                break;


                            case 6:
                                if (!Buff.ContainsKey(387))
                                {
                                    AddBuff(new Buff(387, Level), BattleEntity, true);
                                }
                                break;

                            case 7:
                                if (!Buff.ContainsKey(395))
                                {
                                    AddBuff(new Buff(395, Level), BattleEntity, true);
                                }
                                break;

                            case 8:
                                if (!Buff.ContainsKey(396))
                                {
                                    AddBuff(new Buff(396, Level), BattleEntity, true);
                                }
                                break;

                            case 9:
                                if (!Buff.ContainsKey(397))
                                {
                                    AddBuff(new Buff(397, Level), BattleEntity, true);
                                }
                                break;

                            case 10:
                                if (!Buff.ContainsKey(398))
                                {
                                    AddBuff(new Buff(398, Level), BattleEntity, true);
                                }
                                break;

                            case 11:
                                if (!Buff.ContainsKey(410))
                                {
                                    AddBuff(new Buff(410, Level), BattleEntity, true);
                                }
                                break;

                            case 12:
                                if (!Buff.ContainsKey(411))
                                {
                                    AddBuff(new Buff(411, Level), BattleEntity, true);
                                }
                                break;

                            case 13:
                                if (!Buff.ContainsKey(444))
                                {
                                    AddBuff(new Buff(444, Level), BattleEntity, true);
                                }
                                break;

                            case 14:
                                if (!Buff.ContainsKey(663))
                                {
                                    AddBuff(new Buff(663, Level), BattleEntity, true);
                                }
                                break;

                            case 15:
                                if (!Buff.ContainsKey(686))
                                {
                                    AddBuff(new Buff(686, Level), BattleEntity, true);
                                }
                                break;

                            case 16: //Lightning Wings
                                if (!Buff.ContainsKey(755))
                                {
                                    AddBuff(new Buff(755, Level), BattleEntity, true);
                                }
                                break;
                        }
                    }
                }

                if (UseSp)
                {
                    ItemInstance specialist = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                    if (specialist == null)
                    {
                        return;
                    }
                    if (LastSpGaugeRemove <= new DateTime(0001, 01, 01, 00, 00, 00))
                    {
                        LastSpGaugeRemove = DateTime.Now;
                    }
                    if (LastSkillUse.AddSeconds(15) >= DateTime.Now && LastSpGaugeRemove.AddSeconds(1) <= DateTime.Now)
                    {
                        byte spType = 0;

                        if ((specialist.Item.Morph > 1 && specialist.Item.Morph < 8) || (specialist.Item.Morph > 9 && specialist.Item.Morph < 16))
                        {
                            spType = 3;
                        }
                        else if (specialist.Item.Morph > 16 && specialist.Item.Morph < 29)
                        {
                            spType = 2;
                        }
                        else if (specialist.Item.Morph == 9)
                        {
                            spType = 1;
                        }
                        if (SpPoint >= spType)
                        {
                            SpPoint -= spType;
                        }
                        else if (SpPoint < spType && SpPoint != 0)
                        {
                            spType -= (byte)SpPoint;
                            SpPoint = 0;
                            SpAdditionPoint -= spType;
                        }
                        else if (SpPoint == 0 && SpAdditionPoint >= spType)
                        {
                            SpAdditionPoint -= spType;
                        }
                        else if (SpPoint == 0 && SpAdditionPoint < spType)
                        {
                            SpAdditionPoint = 0;

                            double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                            if (UseSp)
                            {
                                LastSp = currentRunningSeconds;
                                if (Session?.HasSession == true)
                                {
                                    if (IsVehicled)
                                    {
                                        return;
                                    }
                                    UseSp = false;
                                    LoadSpeed();
                                    Session.SendPacket(this.GenerateCond());
                                    Session.SendPacket(this.GenerateLev());
                                    SpCooldown = 30;
                                    if (SkillsSp != null)
                                    {
                                        foreach (CharacterSkill ski in SkillsSp.Where(s => !s.CanBeUsed(true)))
                                        {
                                            short time = ski.Skill.Cooldown;
                                            double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + (time * 100);
                                            temp /= 1000;
                                            SpCooldown = temp > SpCooldown ? (int)temp : SpCooldown;
                                        }
                                    }
                                    Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), SpCooldown), 11));
                                    Session.SendPacket($"sd {SpCooldown}");
                                    Session.CurrentMapInstance?.Broadcast(this.GenerateCMode());
                                    Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 1, CharacterId), PositionX, PositionY);

                                    // ms_c
                                    Session.SendPacket(this.GenerateSki());
                                    Session.SendPacket(this.GenerateStat());
                                    Session.SendPackets(this.GenerateStatChar());

                                    Logger.Log.LogUserEvent("CHARACTER_SPECIALIST_RETURN", Session.GenerateIdentity(), $"SpCooldown: {SpCooldown}");

                                    Observable.Timer(TimeSpan.FromMilliseconds(SpCooldown * 1000)).SafeSubscribe(o =>
                                    {
                                        if (Session == null)
                                        {
                                            return;
                                        }

                                        Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                                        Session.SendPacket("sd 0");
                                    });
                                }
                            }
                        }
                        Session.SendPacket(this.GenerateSpPoint());
                        LastSpGaugeRemove = DateTime.Now;
                    }
                }
            }
        }

        public void CheckHuntQuest()
        {
            CharacterQuest quest = Quests?.FirstOrDefault(q => q?.Quest?.QuestType == (int)QuestType.Hunt && q.Quest?.TargetMap == MapInstance?.Map?.MapId && Math.Abs(PositionX - q.Quest?.TargetX ?? 0) < 2 && Math.Abs(PositionY - q.Quest?.TargetY ?? 0) < 2);
            if (quest == null)
            {
                return;
            }
            List<MonsterToSummon> monsters = new List<MonsterToSummon>();
            if (MapInstance?.Monsters != null && !MapInstance.Monsters.Any(s => s?.MonsterVNum == (short)(quest?.GetObjectiveByIndex(1)?.Data ?? -1) && Math.Abs(s?.MapX - quest?.Quest?.TargetX ?? 0) < 4 && Math.Abs(s?.MapY - quest?.Quest?.TargetY ?? 0) < 4))
            {
                for (int a = 0; a < quest.GetObjectiveByIndex(1)?.Objective / 2 + 1; a++)
                {
                    monsters?.Add(new MonsterToSummon((short)(quest?.GetObjectiveByIndex(1)?.Data ?? -1), new MapCell { X = (short)(PositionX + ServerManager.RandomNumber(-2, 3)), Y = (short)(PositionY + ServerManager.RandomNumber(-2, 3)) }, BattleEntity, true));
                }
                EventHelper.Instance.RunEvent(new EventContainer(MapInstance, EventActionType.SPAWNMONSTERS, monsters.AsEnumerable()));
            }
        }

        public void ClearLaurena()
        {
            if (IsLaurenaMorph())
            {
                IsMorphed = false;
                Morph = PreviousMorph;
                PreviousMorph = 0;
                MapInstance?.Broadcast(this.GenerateCMode());
            }

            RemoveBuff(477, true);
            RemoveBuff(478, true);
        }

        public void CloseExchangeOrTrade()
        {
            if (InExchangeOrTrade)
            {
                long? targetSessionId = ExchangeInfo?.TargetCharacterId;

                if (targetSessionId.HasValue && Session.HasCurrentMapInstance)
                {
                    ClientSession targetSession = Session.CurrentMapInstance.GetSessionByCharacterId(targetSessionId.Value);

                    if (targetSession == null)
                    {
                        return;
                    }

                    Session.SendPacket("exc_close 0");
                    targetSession.SendPacket("exc_close 0");
                    ExchangeInfo = null;
                    targetSession.Character.ExchangeInfo = null;
                }
            }
        }

        public void CloseShop()
        {
            if (HasShopOpened && Session.HasCurrentMapInstance)
            {
                KeyValuePair<long, MapShop> shop = Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop => mapshop.Value.OwnerId.Equals(CharacterId));
                if (!shop.Equals(default))
                {
                    Session.CurrentMapInstance.UserShops.Remove(shop.Key);

                    // declare that the shop cannot be closed
                    HasShopOpened = false;

                    Session.CurrentMapInstance?.Broadcast(this.GenerateShopEnd());
                    Session.CurrentMapInstance?.Broadcast(Session, this.GeneratePlayerFlag(0), ReceiverType.AllExceptMe);
                    IsSitting = false;
                    IsShopping = false; // close shop by character will always completely close the shop

                    LoadSpeed();
                    Session.SendPacket(this.GenerateCond());
                    Session.CurrentMapInstance?.Broadcast(this.GenerateRest());
                }
            }
        }

        public bool CustomQuestRewards(QuestType type, long questId)
        {
            switch (type)
            {
                case QuestType.FlowerQuest:
                    if (questId == 5981)
                    {
                        if (ServerManager.Instance.ChannelId == 51)
                        {
                            Session?.SendPacket("msg 0 You can't receive Sound Flower Blessing in Act 4!");
                            break;
                        }

                        GetDignity(100);
                        if (ServerManager.RandomNumber() < 50)
                        {
                            RemoveBuff(379);
                            AddBuff(new Buff(378, Level), BattleEntity);
                        }
                        else
                        {
                            RemoveBuff(378);
                            AddBuff(new Buff(379, Level), BattleEntity);
                        }
                    }
                    return true;
            }
            switch (questId)
            {
                case 2255:
                    short[] possibleRewards = new short[] { 1894, 1895, 1896, 1897, 1898, 1899, 1900, 1901, 1902, 1903 };
                    GiftAdd(possibleRewards[ServerManager.RandomNumber(0, possibleRewards.Length - 1)], 1);
                    return true;
            }
            return false;
        }

        public void Dance() => IsDancing = !IsDancing;

        public void DecreaseMp(int amount) => BattleEntity.DecreaseMp(amount);

        public Character DeepCopy() => (Character)MemberwiseClone();

        public void DeleteBlackList(long characterId)
        {
            CharacterRelationDTO chara = CharacterRelations.Find(s => s.RelatedCharacterId == characterId);
            if (chara != null)
            {
                long id = chara.CharacterRelationId;
                DAOFactory.CharacterRelationDAO.Delete(id);
                ServerManager.Instance.RelationRefresh(id);
                Session.SendPacket(this.GenerateBlinit());
            }
        }

        public void DeleteItem(InventoryType type, short slot)
        {
            if (Inventory != null)
            {
                Inventory.DeleteFromSlotAndType(slot, type);
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(type, slot));
            }
        }

        public void DeleteItemByItemInstanceId(Guid id)
        {
            if (Inventory != null)
            {
                Tuple<short, InventoryType> result = Inventory.DeleteById(id);
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(result.Item2, result.Item1));
            }
        }

        public void DeleteRelation(long characterId, CharacterRelationType relationType)
        {
            CharacterRelationDTO chara = CharacterRelations.Find(s => (s.RelatedCharacterId == characterId || s.CharacterId == characterId) && s.RelationType == relationType);
            if (chara != null)
            {
                long id = chara.CharacterRelationId;
                CharacterDTO charac = DAOFactory.CharacterDAO.LoadById(characterId);
                DAOFactory.CharacterRelationDAO.Delete(id);
                ServerManager.Instance.RelationRefresh(id);

                Session.SendPacket(this.GenerateFinit());
                if (charac != null)
                {
                    List<CharacterRelationDTO> lst = ServerManager.Instance.CharacterRelations.Where(s => s.CharacterId == characterId || s.RelatedCharacterId == characterId).ToList();
                    string result = "finit";
                    foreach (CharacterRelationDTO relation in lst.Where(c => c.RelationType == CharacterRelationType.Friend || c.RelationType == CharacterRelationType.Spouse))
                    {
                        long id2 = relation.RelatedCharacterId == charac.CharacterId ? relation.CharacterId : relation.RelatedCharacterId;
                        bool isOnline = CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup, id2);
                        result += $" {id2}|{(short)relation.RelationType}|{(isOnline ? 1 : 0)}|{DAOFactory.CharacterDAO.LoadById(id2).Name}";
                    }
                    int? sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                    {
                        DestinationCharacterId = charac.CharacterId,
                        SourceCharacterId = CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = result,
                        Type = MessageType.PrivateChat
                    });
                }
            }
        }

        public void DeleteTimeout()
        {
            if (Inventory == null)
            {
                return;
            }

            foreach (ItemInstance item in Inventory.Values)
            {
                if ((item.IsBound || item.Item.ItemType == ItemType.Box) && item.ItemDeleteTime != null && item.ItemDeleteTime < DateTime.Now)
                {
                    Inventory.DeleteById(item.Id);

                    EquipmentBCards.RemoveAll(o => o.ItemVNum == item.ItemVNum);

                    if (item.Type == InventoryType.Wear)
                    {
                        Session.SendPacket(this.GenerateEquipment());
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(item.Type, item.Slot));
                    }
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                }
            }
        }

        public void DisableBuffs(BuffType type, int level = 100) => BattleEntity.DisableBuffs(type, level);

        public void DisableBuffs(List<BuffType> types, int level = 100) => BattleEntity.DisableBuffs(types, level);

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            Miniland?.StopLife();

            if (OriginalFaction != -1)
            {
                Faction = (FactionType)OriginalFaction;
            }
            DisposeShopAndExchange();
            GroupSentRequestCharacterIds?.Clear();
            FamilyInviteCharacters?.Clear();
            FriendRequestCharacters?.Clear();
            Life?.Dispose();
            WalkDisposable?.Dispose();
            SealDisposable?.Dispose();
            AutoBetInterval?.Dispose();
            MarryRequestCharacters?.Clear();
            BazaarItems = null;

            Mates.Where(s => s.IsTeamMember).ToList().ForEach(s =>
            {
                Session.CurrentMapInstance?.Broadcast(Session, s.GenerateOut(), ReceiverType.AllExceptMe); 
                s.ReviveDisposable?.Dispose();
                s.StopLife();
            });
            Session.CurrentMapInstance?.Broadcast(Session, StaticPacketHelper.Out(UserType.Player, CharacterId), ReceiverType.AllExceptMe);

            if (Hp < 1)
            {
                Hp = 1;
            }

            if (ServerManager.Instance.Groups != null && ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(CharacterId)))
            {
                ServerManager.Instance.GroupLeave(Session);
            }

            LeaveTalentArena(true);
            LeaveIceBreaker();
            SaveStaticBuff();
            BattleEntity?.DisableBuffs(BuffType.All);
            BattleEntity?.RemoveOwnedMonsters();
            BattleEntity?.RemoveOwnedNpcs();
            RemoveTemporalMates();

            BattleEntity?.ClearOwnFalcon();
            BattleEntity?.ClearEnemyFalcon();
            BattleEntity?.ClearSacrificeBuff();

            if (MapInstance != null)
            {
                if (MapInstance.MapInstanceId == Family?.Act4RaidBossMap?.MapInstanceId
                    || MapInstance.MapInstanceId == Family?.Act4Raid?.MapInstanceId)
                {
                    short x = (short)(39 + ServerManager.RandomNumber(-2, 3));
                    short y = (short)(42 + ServerManager.RandomNumber(-2, 3));
                    if (Faction == FactionType.Angel)
                    {
                        MapId = 130;
                        MapX = x;
                        MapY = y;
                    }
                    else if (Faction == FactionType.Demon)
                    {
                        MapId = 131;
                        MapX = x;
                        MapY = y;
                    }
                }
                if (MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance || MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                {
                    MapInstance.InstanceBag.DeadList.Add(CharacterId);
                    if (MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        Group?.Sessions.ForEach(s =>
                        {
                            if (s != null)
                            {
                                s.SendPacket(s.Character.Group.GeneraterRaidmbf(s));
                                s.SendPacket(s.Character.Group.GenerateRdlst());
                            }
                        });
                    }
                }
                if (Miniland != null)
                {
                    ServerManager.RemoveMapInstance(Miniland.MapInstanceId);
                }
            }
            SaveObs?.Dispose();
            BattleEntity?.DisposeBuffs();
        }

        public void DisposeShopAndExchange()
        {
            CloseShop();
            CloseExchangeOrTrade();
        }

        public void EnterInstance(ScriptedInstance input, bool loserMode = false)
        {
            ScriptedInstance instance = input.Copy();
            instance.LoadScript(MapInstanceType.TimeSpaceInstance, this, loserMode);
            if (instance.FirstMap == null)
            {
                return;
            }

            if (Session.Character.Level < instance.LevelMinimum)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_LOW_LVL"), 0));
                return;
            }

            if (Session.Character.Level > instance.LevelMaximum)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_HIGH_LVL"), 0));
                return;
            }

            if (loserMode)
            {
                if (Session.Character.Dignity > -1000)
                {
                    Session.Character.Dignity -= 10;
                    Session.SendPacket(this.GenerateFd());
                    Session.CurrentMapInstance?.Broadcast(Session, this.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, this.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.SendPacket(this.GenerateSay($"You've lost -10 dignity.", 11));
                }
            }

            var entries = instance.DailyEntries - Session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == instance.Id && s.Timestamp.Date == DateTime.Today);
            if (instance.DailyEntries == 0 || entries > 0)
            {
                foreach (Gift requiredItem in instance.RequiredItems)
                {
                    if (Session.Character.Inventory.CountItem(requiredItem.VNum) < requiredItem.Amount)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("NO_ITEM_REQUIRED"),
                                ServerManager.GetItem(requiredItem.VNum).Name), 0));
                        return;
                    }

                    Session.Character.Inventory.RemoveItemAmount(requiredItem.VNum, requiredItem.Amount);
                }

                Session?.SendPacket(instance.FirstMap.InstanceBag.GenerateScore());
                Session?.SendPackets(instance.GenerateMinimap());
                Session?.SendPacket(instance.GenerateMainInfo());

                if (instance.Type == ScriptedInstanceType.HiddenTsRaid)
                {
                    ServerManager.Instance.TimeSpaces.ToList().Remove(instance);
                    Session.CurrentMapInstance.Broadcast(Session, StaticPacketHelper.Out(UserType.Object, instance.ScriptedInstanceId));
                    Session.CurrentMapInstance.Broadcast(Session, $"eff_g 822 {instance.ScriptedInstanceId} {instance.PositionX} {instance.PositionY} 1");
                }

                if (instance.StartX != 0 || instance.StartY != 0)
                {
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                        instance.FirstMap.MapInstanceId, instance.StartX, instance.StartY);
                }
                else
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, instance.FirstMap.MapInstanceId);
                }
                instance.InstanceBag.CreatorId = Session.Character.CharacterId;
                ServerManager.Instance.TimespaceSessions.Add(Session);
                Session.Character.Timespace = instance;
            }
            else
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANCE_NO_MORE_ENTRIES"), 0));
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INSTANCE_NO_MORE_ENTRIES"), 10));
            }
        }

        public void EnterSkyTower(ScriptedInstance input, bool loserMode = false)
        {
            ScriptedInstance instance = input.Copy();
            instance.LoadScript(MapInstanceType.SkyTowerInstance, this, loserMode);
            if (instance.FirstMap == null)
            {
                return;
            }

            if (Session.Character.Level < instance.LevelMinimum)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_LOW_LVL"), 0));
                return;
            }

            if (Session.Character.Level > instance.LevelMaximum)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_HIGH_LVL"), 0));
                return;
            }

            var entries = instance.DailyEntries - Session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == instance.Id && s.Timestamp.Date == DateTime.Today);
            if (instance.DailyEntries == 0 || entries > 0)
            {
                foreach (Gift requiredItem in instance.RequiredItems)
                {
                    if (Session.Character.Inventory.CountItem(requiredItem.VNum) < requiredItem.Amount)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("NO_ITEM_REQUIRED"),
                                ServerManager.GetItem(requiredItem.VNum).Name), 0));
                        return;
                    }

                    Session.Character.Inventory.RemoveItemAmount(requiredItem.VNum, requiredItem.Amount);
                }

                if (instance.StartX != 0 || instance.StartY != 0)
                {
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                        instance.FirstMap.MapInstanceId, instance.StartX, instance.StartY);
                }
                else
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, instance.FirstMap.MapInstanceId);
                }

                instance.InstanceBag.CreatorId = Session.Character.CharacterId;
                //ServerManager.Instance.TimespaceSessions.Add(Session);
                Session.Character.SkyTower = instance;
            }
            else
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANCE_NO_MORE_ENTRIES"), 0));
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INSTANCE_NO_MORE_ENTRIES"), 10));
            }
        }

        public void SendAscrPacket()
        {
            if (Session.CurrentMapInstance.Map.MapId == 2006)
            {
                Session.SendPacket(this.GenerateAscr(Group == null ? AscrPacketType.Alone : AscrPacketType.Group));
                return;
            }
            if (Session.CurrentMapInstance.Map.MapId == 2106)
            {
                Session.SendPacket(this.GenerateAscr(AscrPacketType.Family));
                return;
            }
            Session.SendPacket(this.GenerateAscr(AscrPacketType.Close));
        }

        public void GenerateDignity(NpcMonster monsterinfo)
        {
            if (Level < monsterinfo.Level && Dignity < 100 && Level > 20)
            {
                Dignity += (float)0.5;

                if (Dignity == (int)Dignity)
                {
                    Session.SendPacket(this.GenerateFd());
                    Session.CurrentMapInstance?.Broadcast(Session, this.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, this.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("RESTORE_DIGNITY"), 11));
                }
            }
        }

        // TODO : Clean it up lmfao (just to test)
        public void GenerateEnergyField()
        {
            // All the maps to generate an energy field right there

            List<EnergyField> energyField = new List<EnergyField>
            {
                new EnergyField
                {
                    MapId = 2,
                    MinimumLevel = 15,
                    MaximumLevel = 20
                },
                new EnergyField
                {
                    MapId = 3,
                    MinimumLevel = 21,
                    MaximumLevel = 30
                },
            };

            foreach (var i in energyField)
            {
                var findMyMap = ServerManager.GetMapInstanceByMapId(i.MapId);
                if (findMyMap == null) continue;

                var randomPos = findMyMap.Map.GetRandomPosition();
                i.MapX = randomPos.X;
                i.MapY = randomPos.Y;
            }

            EnergyFields.AddRange(energyField);

        }

        public bool GenerateFamilyXp(int FXP, short InstanceId = -1)
        {
            if (!Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.BlockFExp && s.DateEnd > DateTime.Now) && Family != null && FamilyCharacter != null
             && (InstanceId == -1 || Session.Character.GeneralLogs.CountLinq(s => s.LogType == "InstanceEntry" && short.Parse(s.LogData) == InstanceId && s.Timestamp.Date == DateTime.Today) == 0))
            {
                FamilyCharacterDTO famchar = FamilyCharacter;
                FamilyDTO fam = Family;
                fam.FamilyExperience += FXP * ServerManager.Instance.Configuration.RateFxp;
                famchar.Experience += FXP * ServerManager.Instance.Configuration.RateFxp;
                if (CharacterHelper.LoadFamilyXPData(Family.FamilyLevel) <= fam.FamilyExperience)
                {
                    fam.FamilyExperience -= CharacterHelper.LoadFamilyXPData(Family.FamilyLevel);
                    fam.FamilyLevel++;
                    Family.InsertFamilyLog(FamilyLogType.FamilyLevelUp, level: fam.FamilyLevel);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                    {
                        DestinationCharacterId = Family.FamilyId,
                        SourceCharacterId = CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAMILY_UP"), 0),
                        Type = MessageType.Family
                    });
                }
                DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref famchar);
                DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                ServerManager.Instance.FamilyRefresh(Family.FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                {
                    DestinationCharacterId = Family.FamilyId,
                    SourceCharacterId = CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                });
                if (FXP > 1000)
                {
                    int value = FXP - FXP % 1000;
                    Session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyXP, Session.Character.Name, experience: value);
                }
                else if (famchar.Experience % 1000 == 0)
                {
                    Session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyXP, Session.Character.Name, experience: 1000);
                }
                var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.EarnFamilyActionPoints);
                if (quest != null)
                {
                    Session.Character.IncreaseBattlePassQuestObjectives(quest.Id, FXP);
                }
                return true;
            }
            return false;
        }

        public void GenerateKillBonus(MapMonster monsterToAttack, BattleEntity Killer)
        {
            #region BossMap
            switch (monsterToAttack.MapInstance.Map.MapId)
            {
                #region Comet Meadow
                case 103: // Comet Meadow
                    MapInstance.Map.MonsterKilled++;

                    if (MapInstance.Map.MonsterKilled == 100)
                    {
                        MapMonster monster = new MapMonster
                        {
                            MonsterVNum = 588,
                            MapY = Session.Character.PositionY,
                            MapX = Session.Character.PositionX,
                            MapId = Session.Character.MapInstance.Map.MapId,
                            Position = Session.Character.Direction,
                            IsMoving = true,
                            MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                            ShouldRespawn = false,
                            IsHostile = false,
                        };
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    }

                    if (monsterToAttack.Monster.NpcMonsterVNum == 588)
                    {
                        switch (ServerManager.RandomNumber(0, 4))
                        {
                            case 0:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 5:
                                        Session.Character.GiftAdd(571, 1, rare: 7);
                                        break;
                                    case <= 10:
                                        Session.Character.GiftAdd(571, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(571, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 1:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 5:
                                        Session.Character.GiftAdd(583, 1, rare: 7);
                                        break;
                                    case <= 10:
                                        Session.Character.GiftAdd(583, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(583, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 2:
                                Session.Character.GiftAdd(1003, 50);
                                break;
                            case 3:
                                if (Session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                    Session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                                    return;
                                }
                                var goldAmount = ServerManager.RandomNumber(50000, 90000);
                                Session.Character.Gold += goldAmount;
                                Session.SendPacket(this.GenerateGold());
                                Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(1046).Name} x {goldAmount}", 10);
                                break;
                        }
                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 7369), PositionX, PositionY);
                        MapInstance.Map.MonsterKilled = 0;
                    }
                    break;
                #endregion

                #region Western Path
                case 207: // Western Path
                    MapInstance.Map.MonsterKilled++;

                    if (MapInstance.Map.MonsterKilled == 100)
                    {
                        MapMonster monster = new MapMonster
                        {
                            MonsterVNum = 1904,
                            MapY = Session.Character.PositionY,
                            MapX = Session.Character.PositionX,
                            MapId = Session.Character.MapInstance.Map.MapId,
                            Position = Session.Character.Direction,
                            IsMoving = true,
                            MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                            ShouldRespawn = false,
                            IsHostile = false,
                        };
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    }

                    if (monsterToAttack.Monster.NpcMonsterVNum == 1904)
                    {
                        switch (ServerManager.RandomNumber(0, 4))
                        {
                            case 0:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 10:
                                        Session.Character.GiftAdd(573, 1, rare: 7);
                                        break;
                                    case <= 20:
                                        Session.Character.GiftAdd(573, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(573, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 1:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 10:
                                        Session.Character.GiftAdd(585, 1, rare: 7);
                                        break;
                                    case <= 20:
                                        Session.Character.GiftAdd(585, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(585, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 2:
                                Session.Character.GiftAdd(1011, 50);
                                break;
                            case 3:
                                if (Session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                    Session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                                    return;
                                }
                                var goldAmount = ServerManager.RandomNumber(550000, 900000);
                                Session.Character.Gold += goldAmount;
                                Session.SendPacket(this.GenerateGold());
                                Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(1046).Name} x {goldAmount}", 10);
                                break;
                        }
                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 7369), PositionX, PositionY);
                        MapInstance.Map.MonsterKilled = 0;
                    }
                    break;
                #endregion

                #region Misty Forest
                case 401: // Misty Forest
                    MapInstance.Map.MonsterKilled++;

                    if (MapInstance.Map.MonsterKilled == 100)
                    {
                        MapMonster monster = new MapMonster
                        {
                            MonsterVNum = 589,
                            MapY = Session.Character.PositionY,
                            MapX = Session.Character.PositionX,
                            MapId = Session.Character.MapInstance.Map.MapId,
                            Position = Session.Character.Direction,
                            IsMoving = true,
                            MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                            ShouldRespawn = false,
                            IsHostile = false,
                        };
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    }

                    if (monsterToAttack.Monster.NpcMonsterVNum == 589)
                    {
                        switch (ServerManager.RandomNumber(0, 4))
                        {
                            case 0:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 5:
                                        Session.Character.GiftAdd(571, 1, rare: 7);
                                        break;
                                    case <= 10:
                                        Session.Character.GiftAdd(571, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(571, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 1:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 5:
                                        Session.Character.GiftAdd(583, 1, rare: 7);
                                        break;
                                    case <= 10:
                                        Session.Character.GiftAdd(583, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(583, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 2:
                                Session.Character.GiftAdd(1006, 50);
                                break;
                            case 3:
                                if (Session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                    Session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                                    return;
                                }
                                var goldAmount = ServerManager.RandomNumber(90000, 150000);
                                Session.Character.Gold += goldAmount;
                                Session.SendPacket(this.GenerateGold());
                                Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(1046).Name} x {goldAmount}", 10);
                                break;
                        }
                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 7369), PositionX, PositionY);
                        MapInstance.Map.MonsterKilled = 0;
                    }
                    break;
                #endregion

                #region Secret Labs 3
                case 44: // Secret Labs 3
                    MapInstance.Map.MonsterKilled++;

                    if (MapInstance.Map.MonsterKilled == 100)
                    {
                        MapMonster monster = new MapMonster
                        {
                            MonsterVNum = 600,
                            MapY = Session.Character.PositionY,
                            MapX = Session.Character.PositionX,
                            MapId = Session.Character.MapInstance.Map.MapId,
                            Position = Session.Character.Direction,
                            IsMoving = true,
                            MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                            ShouldRespawn = false,
                            IsHostile = false,
                        };
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    }

                    if (monsterToAttack.Monster.NpcMonsterVNum == 600)
                    {
                        switch (ServerManager.RandomNumber(0, 4))
                        {
                            case 0:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 10:
                                        Session.Character.GiftAdd(572, 1, rare: 7);
                                        break;
                                    case <= 20:
                                        Session.Character.GiftAdd(572, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(572, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 1:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 10:
                                        Session.Character.GiftAdd(583, 1, rare: 7);
                                        break;
                                    case <= 20:
                                        Session.Character.GiftAdd(583, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(583, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 2:
                                Session.Character.GiftAdd(1011, 50);
                                break;
                            case 3:
                                if (Session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                    Session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                                    return;
                                }
                                var goldAmount = ServerManager.RandomNumber(150000, 350000);
                                Session.Character.Gold += goldAmount;
                                Session.SendPacket(this.GenerateGold());
                                Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(1046).Name} x {goldAmount}", 10);
                                break;
                        }
                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 7369), PositionX, PositionY);
                        MapInstance.Map.MonsterKilled = 0;
                    }
                    break;
                #endregion

                #region Robber's Courtyard
                case 193: // Robber's Courtyard
                    MapInstance.Map.MonsterKilled++;

                    if (MapInstance.Map.MonsterKilled == 100)
                    {
                        MapMonster monster = new MapMonster
                        {
                            MonsterVNum = 1019,
                            MapY = Session.Character.PositionY,
                            MapX = Session.Character.PositionX,
                            MapId = Session.Character.MapInstance.Map.MapId,
                            Position = Session.Character.Direction,
                            IsMoving = true,
                            MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                            ShouldRespawn = false,
                            IsHostile = false,
                        };
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    }

                    if (monsterToAttack.Monster.NpcMonsterVNum == 1019)
                    {
                        switch (ServerManager.RandomNumber(0, 4))
                        {
                            case 0:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 10:
                                        Session.Character.GiftAdd(573, 1, rare: 7);
                                        break;
                                    case <= 20:
                                        Session.Character.GiftAdd(573, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(573, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 1:
                                switch (ServerManager.RandomNumber(0, 100))
                                {
                                    case <= 10:
                                        Session.Character.GiftAdd(585, 1, rare: 7);
                                        break;
                                    case <= 20:
                                        Session.Character.GiftAdd(585, 1, rare: 6);
                                        break;
                                    case <= 100:
                                        Session.Character.GiftAdd(585, 1, rare: 5);
                                        break;
                                }
                                break;
                            case 2:
                                Session.Character.GiftAdd(1011, 50);
                                break;
                            case 3:
                                if (Session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                    Session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                                    return;
                                }
                                var goldAmount = ServerManager.RandomNumber(500000, 700000);
                                Session.Character.Gold += goldAmount;
                                Session.SendPacket(this.GenerateGold());
                                Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(1046).Name} x {goldAmount}", 10);
                                break;
                        }
                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 7369), PositionX, PositionY);
                        MapInstance.Map.MonsterKilled = 0;
                    }
                    break;
                    #endregion
            }
            #endregion

            if (ServerManager.Instance.ChannelId == 51 && ServerManager.Instance.Act4DemonStat.Mode == 0
                                                       && ServerManager.Instance.Act4AngelStat.Mode == 0)
            {
                ServerManager.Instance.Sessions.ToList().ForEach(s => s.SendPacket(s.Character.GenerateFc()));
            }

            #region RBB

            if (Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RainbowBattleInstance && monsterToAttack?.MonsterVNum == 589)
            {
                var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));

                rbb.Score += 5;
                Session.CurrentMapInstance.Broadcast($"msg 0 {Session.Character.Name} has killed the Rainbow Mandra and scored 5 points for his team.");
                RainbowBattleManager.SendFbs(Session.CurrentMapInstance);
            }

            #endregion

            #region battlepass quests

            var quest = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionSubType == BattlePassMissionSubType.KillMonstersInPlayerRangeLevel);
            if (quest != null && Convert.ToInt32(monsterToAttack.Monster.Level).IsBetween(Level - 10, Level + 10))
            {
                IncreaseBattlePassQuestObjectives(quest.Id, 1);
            }

            #endregion

            #region Act6Stats

            if (monsterToAttack.MapInstance?.Map.MapId >= 229 && monsterToAttack.MapInstance?.Map.MapId <= 232 &&
                ServerManager.Instance.Act6Zenas.Mode == 0)
            {
                ServerManager.Instance.Act6Zenas.Percentage += 8000 / (ServerManager.Instance.Configuration.CylloanPercentRate - 5);
                ServerManager.Instance.Act6Process();
            }

            if (monsterToAttack.MapInstance?.Map.MapId >= 233 &&
                monsterToAttack.MapInstance?.Map.MapId <= 236 ||
                monsterToAttack.MapInstance?.Map.MapId == 2604 &&
                ServerManager.Instance.Act6Erenia.Mode == 0)
            {
                ServerManager.Instance.Act6Erenia.Percentage += 8000 / (ServerManager.Instance.Configuration.CylloanPercentRate - 5);
                ServerManager.Instance.Act6Process();
            }

            #endregion Act6Stats

            void _handleGoldDrop(DropDTO drop, long maxGold, long? dropOwner, short posX, short posY)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(500)).SafeSubscribe(o =>
                {
                    if (Session == null)
                    {
                        return;
                    }

                    if (Session.HasCurrentMapInstance)
                    {
                        if (CharacterId == dropOwner && StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.AutoLoot))
                        {
                            double multiplier = 1 + (Session.Character.GetBuff(CardType.Item, (byte)AdditionalTypes.Item.IncreaseEarnedGold)[0] / 100D);
                            //multiplier += (Session.Character.ShellEffectMain.FirstOrDefault(s => s.Effect == (byte)ShellWeaponEffectType.GainMoreGold)?.Value ?? 0) / 100D;

                            var titleEffect = Session.Character.EffectFromTitle.ToList().FirstOrDefault(s =>
                                s.Type == (byte)BCardType.CardType.BonusBCards && s.SubType ==
                                (byte)AdditionalTypes.BonusTitleBCards.IncreaseGoldEarned);

                            multiplier += (titleEffect?.FirstData ?? 0) / 100;
                            Gold += (int)(drop.Amount * multiplier);

                            if (Gold > maxGold)
                            {
                                Gold = maxGold;
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                            }

                            Session.SendPacket(this.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(drop.ItemVNum).Name} x {drop.Amount}{(multiplier > 1 ? $" + {(int)(drop.Amount * multiplier) - drop.Amount}" : "")}", 12));
                            Session.SendPacket(this.GenerateGold());
                        }
                        else
                        {
                            Session.CurrentMapInstance.DropItemByMonster(dropOwner, drop, monsterToAttack.MapX, monsterToAttack.MapY);
                        }
                    }
                });
            }

            void _handleItemDrop(DropDTO drop, long? owner, short posX, short posY)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(500)).SafeSubscribe(o =>
                {
                    if (Session == null)
                    {
                        return;
                    }

                    if (Session.HasCurrentMapInstance)
                    {
                        if (CharacterId == owner && StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.AutoLoot))
                        {
                            GiftAdd(drop.ItemVNum, (byte)drop.Amount);
                        }
                        else
                        {
                            Session.CurrentMapInstance
                                .DropItemByMonster(owner, drop, monsterToAttack.MapX, 
                                    monsterToAttack.MapY, 
                                    Quests.Any(q => (q.Quest.QuestType == (int)QuestType.Collect4 || q.Quest.QuestType == (int)QuestType.Collect2 || (q.Quest?.QuestType == (int)QuestType.Collect1 && MapInstance.Map.MapTypes.Any(s => s.MapTypeId != (short)MapTypeEnum.Act4))) && q.Quest.QuestObjectives.Any(qst => qst.Data == drop.ItemVNum)));
                        }
                    }
                });
            }

            lock (_syncObj)
            {
                if (monsterToAttack == null || monsterToAttack.IsAlive)
                {
                    return;
                }
                monsterToAttack.RunDeathEvent();

                if (monsterToAttack.GetBuff(CardType.SpecialEffects, (byte)AdditionalTypes.SpecialEffects.DecreaseKillerHP) is int[] DecreaseKillerHp)
                {
                    bool EffectResistance = false;
                    if (Killer.MapEntityId != CharacterId)
                    {
                        if (Killer.HasBuff(CardType.Buff, (byte)AdditionalTypes.Buff.EffectResistance))
                        {
                            if (ServerManager.RandomNumber() < 90)
                            {
                                EffectResistance = true;
                            }
                        }
                        if (!EffectResistance)
                        {
                            if (DecreaseKillerHp[0] > 0)
                            {
                                if (!HasGodMode)
                                {
                                    int DecreasedHp = 0;
                                    if (Killer.Hp - Killer.Hp * DecreaseKillerHp[0] / 100 > 1)
                                    {
                                        DecreasedHp = Killer.Hp * DecreaseKillerHp[0] / 100;
                                    }
                                    else
                                    {
                                        DecreasedHp = Killer.Hp - 1;
                                    }
                                    Killer.GetDamage(DecreasedHp, monsterToAttack.BattleEntity, true);
                                    Session.SendPacket(Killer.GenerateDm(DecreasedHp));
                                    if (Killer.Mate != null)
                                    {
                                        Session.SendPacket(Killer.Mate.GenerateStatInfo());
                                    }
                                    Session.SendPacket(new EffectPacket { EffectType = Killer.UserType, CallerId = Killer.MapEntityId, EffectId = 6007 });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (HasBuff(CardType.Buff, (byte)AdditionalTypes.Buff.EffectResistance))
                        {
                            if (ServerManager.RandomNumber() < 90)
                            {
                                EffectResistance = true;
                            }
                        }
                        if (!EffectResistance)
                        {
                            if (DecreaseKillerHp[0] > 0)
                            {
                                if (!HasGodMode)
                                {
                                    int DecreasedHp = 0;
                                    if (Hp - Hp * DecreaseKillerHp[0] / 100 > 1)
                                    {
                                        DecreasedHp = Hp * DecreaseKillerHp[0] / 100;
                                    }
                                    else
                                    {
                                        DecreasedHp = Hp - 1;
                                    }
                                    GetDamage(DecreasedHp, monsterToAttack.BattleEntity, true);
                                    Session.SendPacket(this.GenerateDm(DecreasedHp));
                                    Session.SendPacket(this.GenerateStat());
                                    Session.SendPacket(this.GenerateEff(6007));
                                }
                            }
                        }
                    }
                }

                Random random = new Random(DateTime.Now.Millisecond & monsterToAttack.MapMonsterId);

                long? dropOwner;

                lock (monsterToAttack.DamageList)
                {
                    dropOwner = (monsterToAttack.DamageList.FirstOrDefault(s => s.Value > 0).Key?.MapEntityId);
                }

                var quest2 = ServerManager.Instance.BattlePassQuests.Find(s => s.MissionType == BattlePassMissionType.KillMonster);
                if (quest2 != null && quest2.FirstData == monsterToAttack.MonsterVNum)
                {
                    var getMonsterHp = monsterToAttack.MonsterVNum * 0.10;

                    lock (monsterToAttack.DamageList)
                    {
                        foreach (BattleEntity entity in monsterToAttack.DamageList.Where(s => s.Value >= getMonsterHp).Select(s => s.Key))
                        {
                            var monsterDmgList = monsterToAttack.DamageList.FirstOrDefault(s => s.Key.Character.CharacterId == entity.Character.CharacterId);
                            if (monsterDmgList.Key == null) continue;
                            long getValue = entity.Character != null ? monsterDmgList.Value : 0;

                            bool canGetQuest = getValue >= getMonsterHp;
                            if (canGetQuest)
                            {
                                entity.Character.IncreaseBattlePassQuestObjectives(quest2.Id, 1);
                            }
                        }
                    }
                }

                Group group = null;
                if (dropOwner != null)
                {
                    group = ServerManager.Instance.Groups.Find(g => g.IsMemberOfGroup((long)dropOwner) && g.GroupType == GroupType.Group);
                }
                IncrementQuests(QuestType.Hunt, monsterToAttack.MonsterVNum);

                if (ServerManager.Instance.ChannelId == 51)
                {
                    if (ServerManager.Instance.Act4DemonStat.Mode == 0 && ServerManager.Instance.Act4AngelStat.Mode == 0 && !CaligorRaid.IsRunning)
                    {
                        ServerManager.Instance.IncreaseFcPercentage(10, Faction);
                    }

                    if (monsterToAttack.MonsterVNum == 556)
                    {
                        if (ServerManager.Instance.Act4AngelStat.Mode == 1 && Faction != FactionType.Angel)
                        {
                            ServerManager.Instance.Act4AngelStat.Mode = 0;
                            ServerManager.Instance.InterChannelShout($"The Devils have defeated Lord Mukraju in the Frozen Crown!");
                        }
                        if (ServerManager.Instance.Act4DemonStat.Mode == 1 && Faction != FactionType.Demon)
                        {
                            ServerManager.Instance.Act4DemonStat.Mode = 0;
                            ServerManager.Instance.InterChannelShout($"The Angels have defeated Lord Mukraju in the Frozen Crown!");
                        }
                    }
                }

                // end owner set
                if (Session.HasCurrentMapInstance && ((MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance || MapInstance.MapInstanceType == MapInstanceType.LodInstance) || MapInstance.DropAllowed))
                {
                    short[] explodeMonsters = new short[] { 1348, 1906 };

                    List<DropDTO> droplist = monsterToAttack.Monster.Drops.Where(s => (!explodeMonsters.Contains(monsterToAttack.MonsterVNum) && Session.CurrentMapInstance.Map.MapTypes.Any(m => m.MapTypeId == s.MapTypeId)) || s.MapTypeId == null).ToList();

                    int levelDifference = Session.Character.Level - monsterToAttack.Monster.Level;

                    #region Quest

                    Quests.Where(q => (q.Quest?.QuestType == (int)QuestType.Collect4 || q.Quest?.QuestType == (int)QuestType.Collect2 || (q.Quest?.QuestType == (int)QuestType.Collect1 && MapInstance.Map.MapTypes.Any(s => s.MapTypeId != (short)MapTypeEnum.Act4)))).ToList().ForEach(qst =>
                    {
                        qst.Quest.QuestObjectives.ForEach(d =>
                        {
                            if (d.SpecialData == monsterToAttack.MonsterVNum || d.SpecialData == null)
                            {
                                droplist.Add(new DropDTO()
                                {
                                    ItemVNum = (short)d.Data,
                                    Amount = 1,
                                    MonsterVNum = monsterToAttack.MonsterVNum,
                                    DropChance = (int)((d.DropRate ?? 100) * 100 * ServerManager.Instance.Configuration.QuestDropRate), // Approx
                                    IsQuestDrop = true
                                });
                            }
                        });
                    });

                    IncrementQuests(QuestType.FlowerQuest, monsterToAttack.Monster.Level);

                    #endregion

                    if (explodeMonsters.Contains(monsterToAttack.MonsterVNum) && ServerManager.RandomNumber() < 50)
                    {
                        MapInstance.Broadcast($"eff 3 {monsterToAttack.MapMonsterId} 3619");
                        if (Killer.MapEntityId != CharacterId)
                        {
                            if (!HasGodMode)
                            {
                                int DecreasedHp = 0;
                                if (Killer.Hp - Killer.Hp * 50 / 100 > 1)
                                {
                                    DecreasedHp = Killer.Hp * 50 / 100;
                                }
                                else
                                {
                                    DecreasedHp = Killer.Hp - 1;
                                }
                                Killer.GetDamage(DecreasedHp, monsterToAttack.BattleEntity, true);
                                if (Killer.Mate != null)
                                {
                                    Session.SendPacket(Killer.Mate.GenerateStatInfo());
                                }
                            }
                        }
                        else
                        {
                            if (!HasGodMode)
                            {
                                int DecreasedHp = 0;
                                if (Hp - Hp * 50 / 100 > 1)
                                {
                                    DecreasedHp = Hp * 50 / 100;
                                }
                                else
                                {
                                    DecreasedHp = Hp - 1;
                                }
                                GetDamage(DecreasedHp, monsterToAttack.BattleEntity, true);
                                Session.SendPacket(this.GenerateStat());
                            }
                        }
                        return;
                    }

                    if (monsterToAttack.Monster.MonsterType != MonsterType.Special)
                    {
                        #region item drop

                        int dropRate = (ServerManager.Instance.Configuration.RateDrop + MapInstance.DropRate);
                        int x = 0;
                        double rndamount = ServerManager.RandomNumber() * random.NextDouble();
                        foreach (DropDTO drop in droplist.OrderBy(s => random.Next()))
                        {
                            if (x < 4)
                            {
                                if (!explodeMonsters.Contains(monsterToAttack.MonsterVNum))
                                {
                                    rndamount = ServerManager.RandomNumber() * random.NextDouble();
                                }
                                bool divideRate = true;
                                if (MapInstance.Map.MapTypes.Any(m => m.MapTypeId == (byte)MapTypeEnum.Act4)
                                 || MapInstance.Map.MapId == 20001 // Miniland
                                 || explodeMonsters.Contains(monsterToAttack.MonsterVNum))
                                {
                                    divideRate = false;
                                }
                                double divider = !divideRate ? 1D : levelDifference >= 20 ? (levelDifference - 19) * 1.2D : levelDifference <= -20 ? (levelDifference + 19) * 1.2D : 1D;

                                if (drop.IsQuestDrop)
                                {
                                    divider = 1;
                                }

                                if (rndamount <= (double)drop.DropChance * dropRate / 1000.000 / divider)
                                {
                                    x++;
                                    if (Session.CurrentMapInstance != null)
                                    {
                                        if (monsterToAttack.Monster.MonsterType == MonsterType.Elite)
                                        {
                                            List<long> alreadyGifted = new List<long>();
                                            List<BattleEntity> damagers;

                                            lock (monsterToAttack.DamageList)
                                            {
                                                damagers = monsterToAttack.DamageList.Keys.ToList();
                                            }

                                            foreach (BattleEntity damager in damagers)
                                            {
                                                if (!alreadyGifted.Contains(damager.MapEntityId))
                                                {
                                                    ClientSession giftsession = ServerManager.Instance.GetSessionByCharacterId(damager.MapEntityId);
                                                    alreadyGifted.Add(damager.MapEntityId);
                                                }
                                            }
                                        }
                                        else if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                                        {
                                            List<long> alreadyGifted = new List<long>();
                                            List<Character> hitters;

                                            lock (monsterToAttack.DamageList)
                                            {
                                                hitters = monsterToAttack.DamageList.Where(s => s.Key?.Character != null && s.Key.Character.MapInstance == monsterToAttack.MapInstance && s.Value > 0).Select(s => s.Key.Character).ToList();
                                            }

                                            foreach (Character hitter in hitters)
                                            {
                                                var itm = ServerManager.GetItem(drop.ItemVNum);

                                                // Bullshit ? will have to test that one thoroughly
                                                if (itm.Type == InventoryType.Equipment)
                                                {
                                                    continue;
                                                }

                                                if (!alreadyGifted.Contains(hitter.CharacterId))
                                                {
                                                    hitter.GiftAdd(drop.ItemVNum, (byte)drop.Amount);
                                                    alreadyGifted.Add(hitter.CharacterId);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (group?.GroupType == GroupType.Group)
                                            {
                                                if (group.SharingMode == (byte)GroupSharingType.ByOrder)
                                                {
                                                    dropOwner = group.GetNextOrderedCharacterId(this);
                                                    if (dropOwner.HasValue)
                                                    {
                                                        group.Sessions.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ITEM_BOUND_TO"), ServerManager.GetItem(drop.ItemVNum).Name, group.Sessions.Single(c => c.Character.CharacterId == (long)dropOwner).Character.Name, drop.Amount), 10)));
                                                    }
                                                }
                                                else
                                                {
                                                    group.Sessions.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DROPPED_ITEM"), ServerManager.GetItem(drop.ItemVNum).Name, drop.Amount), 10)));
                                                }
                                            }

                                            var dropBCard = EquipmentBCards.Where(s =>
                                                s.Type == (byte) BCardType.CardType.BigCashMoneyBCards &&
                                                s.SubType == (byte) AdditionalTypes.BigCashMoneyBCards
                                                    .IncreaseDropPercentage).Sum(x => x.FirstData);

                                            if (dropBCard > 0)
                                            {
                                                var droppedItem = ServerManager.GetItem(drop.ItemVNum);

                                                if (droppedItem != null)
                                                {
                                                    if ((droppedItem.EquipmentSlot == EquipmentType.Gloves ||
                                                         droppedItem.EquipmentSlot == EquipmentType.Boots) ||
                                                        droppedItem.ItemType == ItemType.Weapon ||
                                                        droppedItem.ItemType == ItemType.Armor)
                                                    {
                                                        var rd = ServerManager.RandomNumber<int>();

                                                        if (rd < dropBCard)
                                                        {
                                                            Session?.CurrentMapInstance?.Broadcast(this.GenerateEff(10), PositionX, PositionY);
                                                            Session?.Character?.Session?.SendPacket(Session?.Character?.GenerateSay(Language.Instance.GetMessageFromKey("RECEIVE_DROP"), 12));
                                                            _handleItemDrop(drop, dropOwner, monsterToAttack.MapX, monsterToAttack.MapY);
                                                        }
                                                    }
                                                }
                                            }

                                            _handleItemDrop(drop, dropOwner, monsterToAttack.MapX, monsterToAttack.MapY);
                                        }
                                    }
                                    if (explodeMonsters.Contains(monsterToAttack.MonsterVNum))
                                    {
                                        break;
                                    }
                                }
                                else if (explodeMonsters.Contains(monsterToAttack.MonsterVNum))
                                {
                                    rndamount -= (double)drop.DropChance * dropRate / 1000.000 / divider;
                                }
                            }
                        }

                        #endregion

                        #region gold drop

                        // gold calculation
                        int gold = GetGold(monsterToAttack);
                        gold *= ServerManager.Instance.Configuration.RateGold;
                        long maxGold = ServerManager.Instance.Configuration.MaxGold;
                        gold = gold > maxGold ? (int)maxGold : gold;
                        double randChance = ServerManager.RandomNumber() * random.NextDouble();

                        if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance && gold > 0 && randChance <= (int)(ServerManager.Instance.Configuration.RateGoldDrop * 10 *
                            (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) ? 1 : CharacterHelper.GoldPenalty(Level, monsterToAttack.Monster.Level))))
                        {
                            DropDTO drop2 = new DropDTO
                            {
                                Amount = gold,
                                ItemVNum = 1046
                            };

                            if (Session.CurrentMapInstance != null)
                            {
                                if (Session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4) || monsterToAttack.Monster.MonsterType == MonsterType.Elite)
                                {
                                    List<long> alreadyGifted = new List<long>();
                                    List<BattleEntity> damagers;

                                    lock (monsterToAttack.DamageList)
                                    {
                                        damagers = monsterToAttack.DamageList.Keys.ToList();
                                    }

                                    foreach (BattleEntity damager in damagers)
                                    {
                                        if (!alreadyGifted.Contains(damager.MapEntityId))
                                        {
                                            ClientSession session = ServerManager.Instance.GetSessionByCharacterId(damager.MapEntityId);
                                            if (session != null)
                                            {
                                                double multiplier = 1 + (GetBuff(CardType.Item, (byte)AdditionalTypes.Item.IncreaseEarnedGold)[0] / 100D);
                                                //multiplier += (ShellEffectMain.FirstOrDefault(s => s.Effect == (byte)ShellWeaponEffectType.GainMoreGold)?.Value ?? 0) / 100D;

                                                session.Character.Gold += (int)(drop2.Amount * multiplier);
                                                if (session.Character.Gold > maxGold)
                                                {
                                                    session.Character.Gold = maxGold;
                                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                                                }
                                                session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(drop2.ItemVNum).Name} x {drop2.Amount}{(multiplier > 1 ? $" + {(int)(drop2.Amount * multiplier) - drop2.Amount}" : "")}", 10));
                                                session.SendPacket(session.Character.GenerateGold());
                                            }
                                            alreadyGifted.Add(damager.MapEntityId);
                                        }
                                    }
                                }
                                else
                                {
                                    if (group != null && MapInstance.MapInstanceType != MapInstanceType.LodInstance)
                                    {
                                        if (group.SharingMode == (byte)GroupSharingType.ByOrder)
                                        {
                                            dropOwner = group.GetNextOrderedCharacterId(this);

                                            if (dropOwner.HasValue)
                                            {
                                                group.Sessions.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ITEM_BOUND_TO"), ServerManager.GetItem(drop2.ItemVNum).Name, group.Sessions.Single(c => c.Character.CharacterId == (long)dropOwner).Character.Name, drop2.Amount), 10)));
                                            }
                                        }
                                        else
                                        {
                                            group.Sessions.ForEach(s => s.SendPacket(s.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("DROPPED_ITEM"), ServerManager.GetItem(drop2.ItemVNum).Name, drop2.Amount), 10)));
                                        }
                                    }

                                    _handleGoldDrop(drop2, maxGold, dropOwner, monsterToAttack.MapX, monsterToAttack.MapY);
                                }
                            }
                        }

                        #endregion
                    }
                }

                #region EXP, Reputation and Dignity

                if (Hp > 0 && !monsterToAttack.BattleEntity.IsMateTrainer(monsterToAttack.MonsterVNum))
                {
                    // If the Halloween event is running then the EXP is disabled in NosVille. -- Is
                    // this official-like or VSalu bullshit?
                    if (!ServerManager.Instance.Configuration.HalloweenEvent || MapInstance.Map.MapId != 1)
                    {
                        GenerateXp(monsterToAttack);
                    }

                    GenerateDignity(monsterToAttack.Monster);
                }

                #endregion
            }
        }

        public void GenerateMiniland()
        {
            if (Miniland == null)
            {
                Miniland = ServerManager.GenerateMapInstance(20001, MapInstanceType.NormalInstance, new InstanceBag(), true);
                foreach (MinilandObjectDTO obj in DAOFactory.MinilandObjectDAO.LoadByCharacterId(CharacterId))
                {
                    MinilandObject mapobj = new MinilandObject(obj);
                    if (mapobj.ItemInstanceId != null)
                    {
                        ItemInstance item = Inventory.GetItemInstanceById((Guid)mapobj.ItemInstanceId);
                        if (item != null)
                        {
                            mapobj.ItemInstance = item;
                            MinilandObjects.Add(mapobj);
                        }
                    }
                }
            }
        }

        [Obsolete("GenerateStartupInventory should be used only on startup, for refreshing an inventory slot please use GenerateInventoryAdd instead.")]
        public void GenerateStartupInventory()
        {
            string inv0 = "inv 0", inv1 = "inv 1", inv2 = "inv 2", inv3 = "inv 3", inv6 = "inv 6", inv7 = "inv 7"; // inv 3 used for miniland objects
            if (Inventory != null)
            {
                foreach (ItemInstance inv in Inventory.Values)
                {
                    switch (inv.Type)
                    {
                        case InventoryType.Equipment:
                            if (inv.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                inv0 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Rare}.{inv.Upgrade}.{inv.SpStoneUpgrade}";
                            }
                            else
                            {
                                inv0 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Rare}.{(inv.Item.IsColored ? inv.Design : inv.Upgrade)}.{inv.SpStoneUpgrade}.{inv.RuneAmount}";
                            }
                            break;

                        case InventoryType.Main:
                            inv1 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                            break;

                        case InventoryType.Etc:
                            inv2 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                            break;

                        case InventoryType.Miniland:
                            inv3 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Amount}";
                            break;

                        case InventoryType.Specialist:
                            inv6 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Rare}.{inv.Upgrade}.{inv.SpStoneUpgrade}";
                            break;

                        case InventoryType.Costume:
                            inv7 += $" {inv.Slot}.{inv.ItemVNum}.{inv.Rare}.{inv.Upgrade}.0";
                            break;
                    }
                }
            }
            Session.SendPacket(inv0);
            Session.SendPacket(inv1);
            Session.SendPacket(inv2);
            Session.SendPacket(inv3);
            Session.SendPacket(inv6);
            Session.SendPacket(inv7);
            Session.SendPacket(this.GetMinilandObjectList());
        }

        public int[] GetBuff(CardType type, byte subtype) => BattleEntity.GetBuff(type, subtype);

        public int GetCP()
        {
            int cpmax = (Class > 0 ? 40 : 0) + (JobLevel * 2);
            int cpused = 0;
            foreach (CharacterSkill ski in Skills.GetAllItems())
            {
                cpused += ski.Skill.CPCost;
            }
            return cpmax - cpused;
        }

        public void GetDamage(int damage, BattleEntity damager, bool dontKill = false) => BattleEntity.GetDamage(damage, damager, dontKill);

        public void GetDignity(int amount)
        {
            Dignity += amount;

            if (Dignity > 100)
            {
                Dignity = 100;
            }

            Session.SendPacket(this.GenerateFd());
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
            Session.CurrentMapInstance?.Broadcast(Session, this.GenerateGidx(), ReceiverType.AllExceptMe);
            Session.SendPacket(this.GenerateSay($"{Language.Instance.GetMessageFromKey("RESTORE_DIGNITY")} (+{amount})", 11));
        }

        public int GetDignityIco()
        {
            int icoDignity = 1;

            if (Dignity <= -100)
            {
                icoDignity = 2;
            }
            if (Dignity <= -200)
            {
                icoDignity = 3;
            }
            if (Dignity <= -400)
            {
                icoDignity = 4;
            }
            if (Dignity <= -600)
            {
                icoDignity = 5;
            }
            if (Dignity <= -800)
            {
                icoDignity = 6;
            }

            return icoDignity;
        }

        public void GetDir(int pX, int pY, int nX, int nY)
        {
            BeforeDirection = Direction;
            if (pX == nX && pY < nY)
            {
                Direction = 2;
            }
            else if (pX > nX && pY == nY)
            {
                Direction = 3;
            }
            else if (pX == nX && pY > nY)
            {
                Direction = 0;
            }
            else if (pX < nX && pY == nY)
            {
                Direction = 1;
            }
            else if (pX < nX && pY < nY)
            {
                Direction = 6;
            }
            else if (pX > nX && pY < nY)
            {
                Direction = 7;
            }
            else if (pX > nX && pY > nY)
            {
                Direction = 4;
            }
            else if (pX < nX && pY > nY)
            {
                Direction = 5;
            }
        }

        public List<Portal> GetExtraPortal(FactionType faction = FactionType.None) => new List<Portal>(MapInstancePortalHandler.GenerateMinilandEntryPortals(MapInstance.Map.MapId, Miniland.MapInstanceId)
            .Concat(MapInstancePortalHandler.GenerateAct4EntryPortals(MapInstance.Map.MapId, faction)));

        public List<string> GetFamilyHistory()
        {
            //TODO: Fix some bugs(missing history etc)
            if (Family != null)
            {
                const string packetheader = "ghis";
                List<string> packetList = new List<string>();
                string packet = "";
                int i = 0;
                int amount = 0;
                foreach (FamilyLogDTO log in Family.FamilyLogs.Where(s => s.FamilyLogType != FamilyLogType.WareHouseAdded && s.FamilyLogType != FamilyLogType.WareHouseRemoved).OrderByDescending(s => s.Timestamp).Take(100))
                {
                    packet += $" {(byte)log.FamilyLogType}|{log.FamilyLogData}|{(int)(DateTime.Now - log.Timestamp).TotalHours}";
                    i++;
                    if (i == 50)
                    {
                        i = 0;
                        packetList.Add(packetheader + (amount == 0 ? " 0 " : "") + packet);
                        amount++;
                    }
                    else if (i + (50 * amount) == Family.FamilyLogs.Count)
                    {
                        packetList.Add(packetheader + (amount == 0 ? " 0 " : "") + packet);
                    }
                }

                return packetList;
            }
            return new List<string>();
        }

        public void GetGold(long val, bool isQuest = false)
        {
            Session.Character.Gold += val;
            if (Session.Character.Gold > ServerManager.Instance.Configuration.MaxGold)
            {
                Session.Character.Gold = ServerManager.Instance.Configuration.MaxGold;
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
            }

            Session.SendPacket(isQuest
                 ? this.GenerateSay($"Quest reward: [ {ServerManager.GetItem(1046).Name} x {val} ]", 10)
                 : Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(1046).Name} x {val}", 10));
            Session.SendPacket(Session.Character.GenerateGold());
        }

        public void RemoveGold(long value, bool showMessage)
        {
            Session.Character.Gold -= value;
            if (Session.Character.Gold < 0)
            {
                Session.Character.Gold = 0;
            }

            Session.SendPacket(this.GenerateGold());

            if (showMessage)
            {
                Session.SendPacket(this.GenerateSay($"You paid {value} gold.", 11));
            }
        }

        public void GetJobExp(long val, bool applyRate = true)
        {
            ItemInstance SpInstance = null;
            if (Inventory != null)
            {
                SpInstance = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            if (!UseSp && SpInstance == null) val *= (applyRate ? ServerManager.Instance.Configuration.RateJXP : 1);
            else val *= (applyRate ? ServerManager.Instance.Configuration.RateSpXP : 1);
            if (UseSp && SpInstance != null)
            {
                if (SpInstance.SpLevel >= ServerManager.Instance.Configuration.MaxSPLevel)
                {
                    return;
                }
                int multiplier = SpInstance.SpLevel < 10 ? 10 : SpInstance.SpLevel < 19 ? 5 : 1;
                SpInstance.XP += (int)(val * (multiplier + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.IncreaseSPXP)[0] / 100D));
                GenerateSpXpLevelUp(SpInstance);
                return;
            }
            if (JobLevel >= ServerManager.Instance.Configuration.MaxJobLevel)
            {
                return;
            }
            JobLevelXp += (int)(val * (1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D));
            GenerateJobXpLevelUp();
            Session.SendPacket(this.GenerateLev());
        }

        public IEnumerable<string> GetMinilandEffects() => MinilandObjects.Select(mp => mp.GenerateMinilandEffect(false)).ToList();

        public List<long> GetMTListTargetQueue_QuickFix(CharacterSkill ski, UserType entityType)
        {
            List<long> result = new List<long>();

            if (BattleEntity != null
                && MapInstance != null
                && ski?.Skill != null)
            {
                foreach (long targetId in MTListTargetQueue.Where(target => target.EntityType == entityType
                    && (byte)target.TargetHitType == ski.Skill.HitType).Select(s => s.TargetId))
                {
                    switch (entityType)
                    {
                        case UserType.Player:
                            {
                                Character targetCharacter = MapInstance.GetCharacterById(targetId);

                                if (targetCharacter?.BattleEntity == null /* Invalid character  */
                                    || targetCharacter.Hp < 1 /* Amen */
                                    || !targetCharacter.IsInRange(PositionX, PositionY, ski.Skill.Range) /* Character not in range */
                                    || !BattleEntity.CanAttackEntity(targetCharacter.BattleEntity) /* Try again later */
                                    )
                                {
                                    continue;
                                }
                            }
                            break;

                        case UserType.Monster:
                            {
                                MapMonster targetMonster = MapInstance.GetMonsterById(targetId);

                                if (targetMonster?.BattleEntity == null /* Invalid monster */
                                    || !targetMonster.IsAlive /* Amen */
                                    || targetMonster.CurrentHp < 1 /* Schrรถdinger's cat */
                                    || !targetMonster.IsInRange(PositionX, PositionY, ski.Skill.Range) /* Monster not in range */
                                    || !BattleEntity.CanAttackEntity(targetMonster.BattleEntity) /* Try again later */
                                    )
                                {
                                    continue;
                                }
                            }
                            break;
                    }

                    result.Add(targetId);
                }
            }

            return result;
        }

        public void GetReputation(int amount, bool applyRate = true)
        {
            amount *= (amount > 0 && applyRate ? ServerManager.Instance.Configuration.RateReputation : 1);

            if (amount > 0)
            {
                var buffs = EffectFromTitle.Where(s =>
                    s.Type == (byte)CardType.BonusBCards &&
                    s.SubType == (byte)AdditionalTypes.BonusTitleBCards.IncreaseReputationPercent);

                double bonus = (buffs.Sum(s => s.FirstData) / 100D);

                if (this.HasStaticBonus(StaticBonusType.EreniaMedal))
                {
                    bonus += 0.2;
                }

                amount = (int)(amount * (1 + bonus));
            }

            int beforeReputIco = GetReputationIco();
            Reputation += amount;
            Session.SendPacket(this.GenerateFd());
            if (beforeReputIco != GetReputationIco())
            {
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
            }
            Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
            if (amount > 0)
            {
                Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REPUT_INCREASE"), amount), 12));
            }
            else if (amount < 0)
            {
                Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REPUT_DECREASE"), amount), 11));
            }
        }

        public int GetReputationIco(bool fake = false)
        {
            if (fake)
            {
                return 26;
            }
            var multiplier = ServerManager.Instance.Configuration.ReputationDifficultyMultiplier;
            if (Reputation >= 5000001 * multiplier)
            {
                switch (IsReputationHero())
                {
                    case 1:
                        return 28;

                    case 2:
                        return 29;

                    case 3:
                        return 30;

                    case 4:
                        return 31;

                    case 5:
                        return 32;
                }
            }
            if (Reputation <= 50 * multiplier)
            {
                return 1;
            }

            if (Reputation <= 150 * multiplier)
            {
                return 2;
            }

            if (Reputation <= 250 * multiplier)
            {
                return 3;
            }

            if (Reputation <= 500 * multiplier)
            {
                return 4;
            }

            if (Reputation <= 750 * multiplier)
            {
                return 5;
            }

            if (Reputation <= 1000 * multiplier)
            {
                return 6;
            }

            if (Reputation <= 2250 * multiplier)
            {
                return 7;
            }

            if (Reputation <= 3500 * multiplier)
            {
                return 8;
            }

            if (Reputation <= 5000 * multiplier)
            {
                return 9;
            }

            if (Reputation <= 9500 * multiplier)
            {
                return 10;
            }

            if (Reputation <= 19000 * multiplier)
            {
                return 11;
            }

            if (Reputation <= 25000 * multiplier)
            {
                return 12;
            }

            if (Reputation <= 40000 * multiplier)
            {
                return 13;
            }

            if (Reputation <= 60000 * multiplier)
            {
                return 14;
            }

            if (Reputation <= 85000 * multiplier)
            {
                return 15;
            }

            if (Reputation <= 115000 * multiplier)
            {
                return 16;
            }

            if (Reputation <= 150000 * multiplier)
            {
                return 17;
            }

            if (Reputation <= 190000 * multiplier)
            {
                return 18;
            }

            if (Reputation <= 235000 * multiplier)
            {
                return 19;
            }

            if (Reputation <= 285000 * multiplier)
            {
                return 20;
            }

            if (Reputation <= 350000 * multiplier)
            {
                return 21;
            }

            if (Reputation <= 500000 * multiplier)
            {
                return 22;
            }

            if (Reputation <= 1500000 * multiplier)
            {
                return 23;
            }

            if (Reputation <= 2500000 * multiplier)
            {
                return 24;
            }

            if (Reputation <= 3750000 * multiplier)
            {
                return 25;
            }

            return Reputation <= 5000000 * multiplier ? 26 : 27;
        }

        public int GetShellArmor(ShellArmorEffectType effectType)
        {
            var armor = Inventory.LoadBySlotAndType((byte)EquipmentType.Armor, InventoryType.Wear);
            List<ShellEffectDTO> effects = new List<ShellEffectDTO>();
            if (armor == null)
            {
                return 0;
            }

            if (armor.ShellEffects == null)
            {
                return 0;
            }

            effects.AddRange(armor.ShellEffects);

            return effects.Where(s => s.Effect == (byte)effectType).OrderByDescending(s => s.Value).FirstOrDefault()?.Value ?? 0;
        }

        public CharacterSkill GetSkill(short skillVNum) => GetSkills()?.FirstOrDefault(s => s.SkillVNum == skillVNum);

        public CharacterSkill GetSkillByCastId(short castId) =>
            GetSkills()?.FirstOrDefault(s => s.Skill?.CastId == castId);

        public List<CharacterSkill> GetSkills()
        {
            var list = new List<CharacterSkill>();
            if (UseSp)
            {
                list.AddRange(SkillsSp.GetAllItems().Concat(Skills.Where(s => s.SkillVNum < 200)).ToList());
                list.AddRange(Skills.GetAllItems().Where(sd => sd.IsTattoo).ToList());
            }
            else
            {
                list.AddRange(Skills.GetAllItems());
            }
            return list;
        }

        /// <summary>
        /// Get Stuff Buffs Useful for Stats for example
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <returns></returns>
        public int[] GetStuffBuff(CardType type, byte subtype)
        {
            int[] result = new int[2] { 0, 0 };

            List<BCard> bcards = new List<BCard>();

            if (Skills != null)
            {
                List<BCard> passiveSkillBCards = PassiveSkillHelper.Instance.PassiveSkillToBCards(Skills.Where(s => s?.Skill?.SkillType == 0));

                if (passiveSkillBCards.Any())
                {
                    bcards.AddRange(passiveSkillBCards);
                }
            }

            if (EffectFromTitle != null)
            {
                List<BCard> entry = EffectFromTitle.Where(s => s?.Type.Equals((byte)type) == true && s.SubType.Equals((byte)(subtype)) && s.FirstData > 0).ToList();

                if (entry.Any())
                {
                    bcards.AddRange(entry);
                }
            }

            List<BCard> equipmentBCards = EquipmentBCards.ToList();

            if (equipmentBCards.Any())
            {
                bcards.AddRange(equipmentBCards);
            }

            foreach (BCard bcard in bcards.Where(s => s?.Type == (byte)type && s.SubType == (byte)(subtype) && s.FirstData > 0))
            {
                result[0] += bcard.IsLevelScaled ? (bcard.FirstData * Level) : bcard.FirstData;
                result[1] += bcard.SecondData;
            }

            return result;
        }

        public void GetXp(long val, bool applyRate = true)
        {

            if (Level >= ServerManager.Instance.Configuration.MaxLevel)
            {
                return;
            }

            LevelXp += val * (applyRate ? ServerManager.Instance.Configuration.RateXP : 1) * (int)(1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D);
            GenerateLevelXpLevelUp();
            Session.SendPacket(this.GenerateLev());
        }

        public void GetHeroXp(long val, bool applyRate = true)
        {
            if (HeroLevel >= ServerManager.Instance.Configuration.MaxHeroLevel)
            {
                return;
            }

            HeroXp += val * (applyRate ? ServerManager.Instance.Configuration.RateHeroicXP : 1) * (int)(1 + GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D);
            GenerateHeroXpLevelUp();
            Session.SendPacket(this.GenerateLev());
        }

        public void GiftAdd(short itemVNum, short amount, byte rare = 0, byte upgrade = 0, short design = 0, bool forceRandom = false, byte minRare = 0)
        {
            if (Inventory != null)
            {
                ItemInstance newItem = Inventory.InstantiateItemInstance(itemVNum, CharacterId, amount);
                if (newItem?.Item == null)
                {
                    Logger.Log.Debug("Item VNum {itemVNum} doesn't exist");
                    return;
                }

                newItem.Design = design;

                if (newItem.Item?.ItemType == ItemType.Armor || newItem.Item.ItemType == ItemType.Weapon || newItem.Item.ItemType == ItemType.Shell || forceRandom)
                {
                    if (rare != 0 && !forceRandom)
                    {
                        try
                        {
                            newItem.RarifyItem(Session, RarifyMode.Drop, RarifyProtection.None, forceRare: rare);
                            newItem.Upgrade = (byte)(newItem.Item.BasicUpgrade + upgrade);
                            if (newItem.Upgrade > 10)
                            {
                                newItem.Upgrade = 10;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"Rarify Failed.", e);
                        }
                    }
                    else if (rare == 0 || forceRandom)
                    {
                        do
                        {
                            try
                            {
                                newItem.RarifyItem(Session, RarifyMode.Drop, RarifyProtection.None);
                                newItem.Upgrade = newItem.Item.BasicUpgrade;
                                if (newItem.Rare >= minRare)
                                {
                                    break;
                                }
                            }
                            catch
                            {
                                break;
                            }
                        } while (forceRandom);
                    }
                }

                if (newItem.Item.Type.Equals(InventoryType.Equipment) && rare != 0 && !forceRandom)
                {
                    newItem.Rare = (sbyte)rare;
                    newItem.SetRarityPoint();
                }

                if (newItem.Item.ItemType == ItemType.Shell)
                {
                    newItem.Upgrade = (byte) (newItem.Item.IsHeroic ? 106 : ServerManager.RandomNumber(50, 81));
                }

                if (newItem.Item.EquipmentSlot == EquipmentType.Gloves || newItem.Item.EquipmentSlot == EquipmentType.Boots)
                {
                    newItem.Upgrade = upgrade;
                    newItem.DarkResistance = (short)(newItem.Item.DarkResistance * upgrade);
                    newItem.LightResistance = (short)(newItem.Item.LightResistance * upgrade);
                    newItem.WaterResistance = (short)(newItem.Item.WaterResistance * upgrade);
                    newItem.FireResistance = (short)(newItem.Item.FireResistance * upgrade);
                }

                List<ItemInstance> newInv = Inventory.AddToInventory(newItem);
                if (newInv.Count > 0)
                {
                    if (newItem.Item.IsHeroic && newItem.Item.ItemType == ItemType.Armor || newItem.Item.ItemType == ItemType.Weapon && newItem.Rare > 0)
                    {
                        newItem.ShellEffects.Clear();
                        DAOFactory.ShellEffectDAO.DeleteByEquipmentSerialId(newItem.EquipmentSerialId);
                        newItem.GenerateHeroicShell(Session, RarifyProtection.RandomHeroicAmulet);
                        newItem.SetRarityPoint();
                    }

                    Session.SendPacket(this.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newItem.Item.Name} x {amount}", 10));
                    if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId != (short)MapTypeEnum.Act4))
                    {
                        Session.SendPacket($"rdi {newItem.ItemVNum} {amount}");
                    }
                }
                else if (MailList.Count(s => s.Value.AttachmentVNum != null) < 40)
                {
                    SendGift(CharacterId, itemVNum, amount, newItem.Rare, newItem.Upgrade, newItem.Design, false);
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PACKET_ARRIVED"), $"{newItem.Item.Name} x {amount}"), 0));
                }
            }
        }

        public bool HasBuff(short cardId) => BattleEntity.HasBuff(cardId);

        public bool HasBuff(CardType type, byte subtype) => BattleEntity.HasBuff(type, subtype);

        public bool HaveBackpack() => StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BackPack);

        public bool HaveExtension() => StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.ExtensionInventory);

        public double HPLoad() => BattleEntity.HPLoad();

        public void IncreaseDollars(int amount)
        {
            try
            {
                if (!ServerManager.Instance.MallApi.SendCurrencyAsync(ServerManager.Instance.Configuration.MallAPIKey, Session.Account.AccountId, amount).Result)
                {
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("MALL_ACCOUNT_NOT_EXISTING"), 10));
                    return;
                }
            }
            catch
            {
                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("MALL_UNKNOWN_ERROR"), 10));
                return;
            }

            Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MALL_CURRENCY_RECEIVE"), amount), 10));
        }

        public void IncrementQuests(QuestType type, int firstData = 0, int secondData = 0, int thirdData = 0, bool forGroupMember = false)
        {
            foreach (CharacterQuest quest in Quests.Where(q => q?.Quest?.QuestType == (int)type))
            {
                switch ((QuestType)quest.Quest.QuestType)
                {
                    case QuestType.Capture1:
                    case QuestType.Capture2:
                    case QuestType.WinRaid:
                        quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex));
                        break;

                    case QuestType.Collect1:
                    case QuestType.Collect2:
                    case QuestType.Collect3:
                    case QuestType.Collect4:
                    case QuestType.Hunt:
                        quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex));
                        if (!forGroupMember)
                        {
                            IncrementGroupQuest(type, firstData, secondData, thirdData);
                        }
                        break;

                    case QuestType.Product:
                        quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex, secondData));
                        break;

                    case QuestType.Dialog1:
                    case QuestType.Dialog2:
                        quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex, isOver: true));
                        break;

                    case QuestType.Wear:
                        if (quest.Quest.QuestObjectives.Any(q => q.SpecialData == firstData &&
                        (Session.Character.Inventory.Any(i => i.Value.ItemVNum == q.Data && i.Value.Type == InventoryType.Wear) || (quest.QuestId == 1541 || quest.QuestId == 1546) && Class != ClassType.Adventurer)))
                        {
                            IncrementObjective(quest, isOver: true);
                        }
                        break;

                    case QuestType.Brings:
                    case QuestType.Required:
                        quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList().ForEach(d =>
                        {
                            if (Inventory.CountItem(d.SpecialData ?? -1) >= d.Objective)
                            {
                                Inventory.RemoveItemAmount(d.SpecialData ?? -1, d.Objective ?? 1);
                                IncrementObjective(quest, d.ObjectiveIndex, d.Objective ?? 1);
                            }
                        });
                        break;

                    case QuestType.GoTo:
                        if (quest.Quest.TargetMap == firstData && Math.Abs(secondData - quest.Quest.TargetX ?? 0) < 3 && Math.Abs(thirdData - quest.Quest.TargetY ?? 0) < 3)
                        {
                            IncrementObjective(quest, isOver: true);
                        }
                        break;

                    case QuestType.Use:
                        quest.Quest.QuestObjectives.Where(o => o.Data == firstData && Mates.Any(m => m.NpcMonsterVNum == o.SpecialData && m.IsTeamMember)).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex, d.Objective ?? 1));
                        break;

                    case QuestType.FlowerQuest:
                        if (firstData + 10 < Level)
                        {
                            continue;
                        }
                        IncrementObjective(quest, 1);
                        break;

                    case QuestType.TimesSpace:
                        quest.Quest.QuestObjectives.Where(o => o.SpecialData == firstData).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex));
                        break;

                    case QuestType.TransmitGold:
                        foreach (QuestObjectiveDTO q in quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList())
                        {
                            if (Session.Character.Gold >= q.Objective)
                            {
                                Gold -= (long)q.Objective;
                                Session.SendPacket(this.GenerateGold());
                                quest.Quest.QuestObjectives.Where(o => o.Data == firstData).ToList().ForEach(d => IncrementObjective(quest, d.ObjectiveIndex, isOver: true));
                                return;
                            }
                        }
                        break;

                    //TODO : Later
                    case QuestType.TsPoint:
                    case QuestType.NumberOfKill:
                    case QuestType.Inspect:
                    case QuestType.Needed:
                    case QuestType.TargetReput:
                    case QuestType.Collect5:
                        break;
                }
            }
        }

        public void Initialize()
        {
            _random = new Random();
            ExchangeInfo = null;
            SpCooldown = 30;
            SaveX = 0;
            SaveY = 0;
            LastDefence = DateTime.Now.AddSeconds(-21);
            LastDelay = DateTime.Now.AddSeconds(-5);
            LastHealth = DateTime.Now;
            LastEffect = DateTime.Now;
            LastDeposit = DateTime.Now;
            LastWithdraw = DateTime.Now;
            Session = null;
            MailList = new ConcurrentDictionary<int, MailDTO>();
            BattleEntity = new BattleEntity(this, null);
            Group = null;
            GmPvtBlock = false;
            CurrentKill = 0;
            CurrentDie = 0;
            CurrentTc = 0;
        }

        public bool IsBlockedByCharacter(long characterId) => CharacterRelations.Any(b => b.RelationType == CharacterRelationType.Blocked && b.CharacterId.Equals(characterId) && characterId != CharacterId);

        public bool IsBlockingCharacter(long characterId) => CharacterRelations.Any(c => c.RelationType == CharacterRelationType.Blocked && c.RelatedCharacterId.Equals(characterId));

        public bool IsCoupleOfCharacter(long characterId) => CharacterRelations.Any(c => characterId != CharacterId && c.RelationType == CharacterRelationType.Spouse && (c.RelatedCharacterId.Equals(characterId) || c.CharacterId.Equals(characterId)));

        public bool IsFriendlistFull() => CharacterRelations.Where(s => s.RelationType == CharacterRelationType.Friend || s.RelationType == CharacterRelationType.Spouse).ToList().Count >= 80;

        public bool IsFriendOfCharacter(long characterId) => CharacterRelations.Any(c => characterId != CharacterId && (c.RelationType == CharacterRelationType.Friend || c.RelationType == CharacterRelationType.Spouse) && (c.RelatedCharacterId.Equals(characterId) || c.CharacterId.Equals(characterId)));

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <param name="range">The range of the coordinates to be maximal distanced.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        public bool IsInRange(int xCoordinate, int yCoordinate, int range = 50)
        {
            return Map.GetDistance(new MapCell
            {
                X = (short)xCoordinate,
                Y = (short)yCoordinate
            }, new MapCell
            {
                X = PositionX,
                Y = PositionY
            }) <= range;
        }

        public bool IsLaurenaMorph() => Morph == 1000099 /* Hamster */ || Morph == 1000156 /* Bushtail */;

        public bool IsMuted() => Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.Muted && s.DateEnd > DateTime.Now);

        public int IsReputationHero()
        {
            int i = 0;

            foreach (CharacterDTO character in ServerManager.Instance.TopReputation)
            {
                i++;

                if (character.CharacterId == CharacterId)
                {
                    if (i == 1)
                    {
                        return 5;
                    }

                    if (i == 2)
                    {
                        return 4;
                    }

                    if (i == 3)
                    {
                        return 3;
                    }

                    if (i <= 13)
                    {
                        return 2;
                    }

                    if (i <= 43)
                    {
                        return 1;
                    }
                }
            }

            return 0;
        }

        public void LearnAdventurerSkills(bool isCommand = false)
        {
            if (Class == 0)
            {
                bool hasLearnedNewSkill = false;

                for (short skillVNum = 200; skillVNum <= 210; skillVNum++)
                {
                    Skill skill = ServerManager.GetSkill(skillVNum);

                    if (skill?.Class == 0 && JobLevel >= skill.LevelMinimum && !Skills.Any(s => s.SkillVNum == skillVNum))
                    {
                        hasLearnedNewSkill = true;

                        Skills[skillVNum] = new CharacterSkill
                        {
                            SkillVNum = skillVNum,
                            CharacterId = CharacterId
                        };
                    }
                }

                if (!isCommand && hasLearnedNewSkill)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
                    Session.SendPacket(this.GenerateSki());
                    Session.SendPackets(this.GenerateQuicklist());
                }
            }
        }

        public void LearnSPSkill()
        {
            ItemInstance specialist = null;

            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            }

            byte SkillSpCount = (byte)SkillsSp.Count;

            SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();

            foreach (Skill ski in ServerManager.GetAllSkill())
            {
                if (specialist != null && ski.UpgradeType == specialist.Item.Morph && ski.SkillType == 1 && specialist.SpLevel >= ski.LevelMinimum)
                {
                    SkillsSp[ski.SkillVNum] = new CharacterSkill { SkillVNum = ski.SkillVNum, CharacterId = CharacterId };
                }
            }

            if (SkillsSp.Count != SkillSpCount)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SKILL_LEARNED"), 0));
            }
        }

        public void LeaveIceBreaker()
        {

        }

        public void LeaveTalentArena(bool surrender = false)
        {
            lock (ServerManager.Instance.ArenaTeams)
            {
                var memb = ServerManager.Instance.ArenaMembers.Find(s => s.Session == Session);
                if (memb == null) return;

                ServerManager.Instance.ArenaMembers.Remove(memb);
                ArenaDisposable?.Dispose();
                Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 2, 0, 0));
                Session.SendPacket(Session.Character.GenerateTaM(1));
                Session.Character.DefaultTimer = 120;

                if (surrender)
                {
                    var countBan = DAOFactory.PenaltyLogDAO.LoadByAccount(Session.Account.AccountId).Count(s => s.DateEnd.Day == DateTime.Now.Day);
                    PenaltyLogDTO log = new()
                    {
                        AccountId = Session.Account.AccountId,
                        Reason = "Surrending AoT",
                        Penalty = PenaltyType.ArenaBan,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(10 * (countBan + 1)),
                        AdminName = "SYSTEM",
                    };

                    Character.InsertOrUpdatePenalty(log);

                    Session.Character.TalentSurrender++;
                    Session.SendPacket(Session.Character.GenerateTaP(Session.Character.TalentArenaBattle.Opponent.Character, 1, Session.Character.TalentArenaBattle.ArenaTeamType, true));
                    Session.SendPacket("ta_sv 1");
                    Session.SendPacket("taw_sv 1");

                    if (UseSp)
                    {
                        SkillsSp.ForEach(c => c.LastUse = DateTime.Now.AddDays(-1));
                    }
                    else
                    {
                        Skills.ForEach(c => c.LastUse = DateTime.Now.AddDays(-1));
                    }
                    Session.SendPacket(this.GenerateSki());
                    Session.SendPackets(this.GenerateQuicklist());

                    List<BuffType> bufftodisable = new() { BuffType.Bad };
                    Session.Character.DisableBuffs(bufftodisable);
                    Session.Character.RemoveBuff(491);

                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                }
            }
        }

        public void LoadInventory()
        {
            IEnumerable<ItemInstanceDTO> inventories = DAOFactory.ItemInstanceDAO.LoadByCharacterId(CharacterId).Where(s => s.Type != InventoryType.FamilyWareHouse).ToList();
            IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadAllByAccount(Session.Account.AccountId);
            IEnumerable<Guid> warehouseInventoryIds = new List<Guid>();
            foreach (CharacterDTO character in characters.Where(s => s.CharacterId != CharacterId))
            {
                IEnumerable<ItemInstanceDTO> characterWarehouseInventory = DAOFactory.ItemInstanceDAO.LoadByCharacterId(character.CharacterId).Where(s => s.Type == InventoryType.Warehouse).ToList();
                inventories = inventories.Concat(characterWarehouseInventory);
                warehouseInventoryIds = warehouseInventoryIds.Concat(characterWarehouseInventory.Select(i => i.Id).ToList());
            }
            DAOFactory.ItemInstanceDAO.DeleteGuidList(warehouseInventoryIds);

            Inventory = new Inventory(this);
            foreach (ItemInstanceDTO inventory in inventories)
            {
                inventory.CharacterId = CharacterId;
                Inventory[inventory.Id] = new ItemInstance(inventory);
                var current = Inventory[inventory.Id];
                var serialId = current.EquipmentSerialId == Guid.Empty ? current.EquipmentSerialId = Guid.NewGuid() : current.EquipmentSerialId;

                var allowedShellTypes = new[] {ItemType.Weapon, ItemType.Armor, ItemType.Shell};

                if (allowedShellTypes.Any(s => s == current.Item.ItemType))
                {
                    current.ShellEffects = new List<ShellEffectDTO>();
                    current.ShellEffects = DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(serialId).ToList();
                }

                if (current.Item.ItemType == ItemType.Jewelery)
                {
                    current.CellonOptions = new List<CellonOptionDTO>();
                    current.CellonOptions = DAOFactory.CellonOptionDAO.GetOptionsByWearableInstanceId(serialId).ToList();
                }
            }

            ItemInstance ring = Inventory.LoadBySlotAndType((byte)EquipmentType.Ring, InventoryType.Wear);
            ItemInstance bracelet = Inventory.LoadBySlotAndType((byte)EquipmentType.Bracelet, InventoryType.Wear);
            ItemInstance necklace = Inventory.LoadBySlotAndType((byte)EquipmentType.Necklace, InventoryType.Wear);
            CellonOptions.Clear();
            if (ring != null)
            {
                CellonOptions.AddRange(ring.CellonOptions);
            }
            if (bracelet != null)
            {
                CellonOptions.AddRange(bracelet.CellonOptions);
            }
            if (necklace != null)
            {
                CellonOptions.AddRange(necklace.CellonOptions);
            }
        }

        public void LoadMail()
        {
            int parcel = 0, letter = 0;
            foreach (MailDTO mail in DAOFactory.MailDAO.LoadSentToCharacter(CharacterId))
            {
                MailList.TryAdd((MailList.Count > 0 ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mail);

                if (mail.AttachmentVNum != null)
                {
                    parcel++;
                    Session.SendPacket(this.GenerateParcel(mail));
                }
                else
                {
                    if (!mail.IsOpened)
                    {
                        letter++;
                    }
                    Session.SendPacket(this.GeneratePost(mail, 1));
                }
            }
            if (parcel > 0)
            {
                Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("GIFTED"), parcel), 11));
            }
            if (letter > 0)
            {
                Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("NEW_MAIL"), letter), 10));
            }
        }

        public void LoadQuicklists()
        {
            QuicklistEntries = new List<QuicklistEntryDTO>();
            IEnumerable<QuicklistEntryDTO> quicklistDTO = DAOFactory.QuicklistEntryDAO.LoadByCharacterId(CharacterId).ToList();
            foreach (QuicklistEntryDTO qle in quicklistDTO)
            {
                QuicklistEntries.Add(qle);
            }
        }

        public void LoadSentMail()
        {
            foreach (MailDTO mail in DAOFactory.MailDAO.LoadSentByCharacter(CharacterId))
            {
                MailList.TryAdd((MailList.Count > 0 ? MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mail);

                Session.SendPacket(this.GeneratePost(mail, 2));
            }
        }

        public void LoadSkills()
        {
            Skills = new ThreadSafeSortedList<int, CharacterSkill>();
            IEnumerable<CharacterSkillDTO> characterskillDTO = DAOFactory.CharacterSkillDAO.LoadByCharacterId(CharacterId).ToList();
            foreach (CharacterSkillDTO characterskill in characterskillDTO.OrderBy(s => s.SkillVNum))
            {
                if (!Skills.ContainsKey(characterskill.SkillVNum))
                {
                    Skills[characterskill.SkillVNum] = new CharacterSkill(characterskill);
                }
            }
        }

        public void LoadSpeed()
        {
            try
            {
                lock (SpeedLockObject)
                {
                    // only load speed if you dont use custom speed
                    if (!IsVehicled && !IsCustomSpeed)
                    {
                        Speed = CharacterHelper.SpeedData[(byte)Class];

                        if (UseSp)
                        {
                            ItemInstance specialist = Inventory?.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);

                            if (specialist?.Item != null)
                            {
                                Speed += specialist.Item.Speed;
                            }
                        }

                        byte fixSpeed = (byte)GetBuff(CardType.Move, (byte)AdditionalTypes.Move.SetMovement)[0];

                        if (fixSpeed != 0)
                        {
                            Speed = fixSpeed;
                        }
                        else
                        {
                            Speed += (byte)GetBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementSpeedIncreased)[0];
                            Speed -= (byte)(GetBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementSpeedDecreased)[0] * -1);
                            Speed = (byte)(Speed + (Speed / 100D * GetBuff(CardType.Move, (byte)AdditionalTypes.Move.MoveSpeedIncreased)[0]));
                            Speed = (byte)(Speed - (Speed / 100D * (GetBuff(CardType.Move, (byte)AdditionalTypes.Move.MoveSpeedDecreased)[0] * -1)));
                        }
                    }

                    if (IsShopping)
                    {
                        Speed = 0;
                        IsCustomSpeed = false;
                        return;
                    }

                    // reload vehicle speed after opening an shop for instance
                    if (IsVehicled && !IsCustomSpeed)
                    {
                        Speed = VehicleSpeed;

                        if (VehicleItem != null)
                        {
                            if (MapInstance == null)
                            {
                                return;
                            }

                            if (MapInstance?.Map?.MapTypes != null && VehicleItem.MapSpeedBoost != null && VehicleItem.ActSpeedBoost != null)
                            {
                                Speed += VehicleItem.MapSpeedBoost[MapInstance.Map.MapId];
                                if (MapInstance.Map.MapTypes.Any(s => new short[] { (short)MapTypeEnum.Act1, (short)MapTypeEnum.CometPlain, (short)MapTypeEnum.Mine1, (short)MapTypeEnum.Mine2, (short)MapTypeEnum.MeadowOfMine, (short)MapTypeEnum.SunnyPlain, (short)MapTypeEnum.Fernon, (short)MapTypeEnum.FernonF, (short)MapTypeEnum.Cliff }.Contains(s.MapTypeId)))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[1];
                                }
                                else if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act2))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[2];
                                }
                                else if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act3))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[3];
                                }
                                else if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[4];
                                }
                                else if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act51))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[51];
                                }
                                else if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act52))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[52];
                                }
                                else if (MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act61
                                || s.MapTypeId == (short)MapTypeEnum.Act61A
                                || s.MapTypeId == (short)MapTypeEnum.Act61D
                                || s.MapTypeId == (short)MapTypeEnum.Act62
                                || s.MapTypeId == (short)MapTypeEnum.Act7))
                                {
                                    Speed += VehicleItem.ActSpeedBoost[6];
                                }
                            }

                            if (HasBuff(CardType.Move, (byte)AdditionalTypes.Move.TempMaximized))
                            {
                                Speed += VehicleItem.SpeedBoost;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Meh, this crashes sometimes for no reason so fuck it
            }
        }

        public double MPLoad() => BattleEntity.MPLoad();

        public bool MuteMessage()
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();

            if (IsMuted() && penalty != null)
            {
                Session.CurrentMapInstance?.Broadcast(Gender == GenderType.Female ? this.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1) : this.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 11));
                Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString(@"hh\:mm\:ss")), 12));
                return true;
            }

            return false;
        }

        /*
        public string OpenFamilyWarehouse()
        {
            if (Family == null || Family.WarehouseSize == 0)
            {
                return UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_WAREHOUSE"));
            }
            return GenerateFStashAll();
        }

        public List<string> OpenFamilyWarehouseHist()
        {
            List<string> packetList = new List<string>();
            if (Family == null || !(FamilyCharacter.Authority == FamilyAuthority.Head
                || FamilyCharacter.Authority == FamilyAuthority.Familydeputy
                || (FamilyCharacter.Authority == FamilyAuthority.Member && Family.MemberCanGetHistory)
                || (FamilyCharacter.Authority == FamilyAuthority.Familykeeper && Family.ManagerCanGetHistory)))
            {
                packetList.Add(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return packetList;
            }
            return GenerateFamilyWarehouseHist();
        }
        */

        public void RemoveBuff(short cardId, bool removePermaBuff = false) => BattleEntity.RemoveBuff(cardId, removePermaBuff);

        public void RemoveBuffByBCardTypeSubType(List<KeyValuePair<byte, byte>> bcardTypes)
        {
            bcardTypes.ForEach(bt => Buff.Where(b => b.Card.BCards.Any(s => s.Type.Equals((byte)bt.Key) && s.SubType.Equals((byte)(bt.Value)) && (s.CastType == 0 || b.Start.AddMilliseconds(b.Card.Delay * 100 + 1500) < DateTime.Now))).ToList().ForEach(a => RemoveBuff(a.Card.CardId)));
        }

        public void RemoveQuest(long questId, bool IsGivingUp = false)
        {
            CharacterQuest questToRemove = Quests.FirstOrDefault(q => q.QuestId == questId);

            if (questToRemove == null)
            {
                return;
            }

            if (questToRemove.Quest.TargetMap != null)
            {
                Session.SendPacket(questToRemove.Quest.RemoveTargetPacket());
            }

            Quests.RemoveWhere(s => s.QuestId != questId, out ConcurrentBag<CharacterQuest> tmp);
            Quests = tmp;

            Session.SendPacket(this.GenerateQuestsPacket());

            if (IsGivingUp)
            {
                return;
            }

            if (questToRemove.Quest.EndDialogId != null)
            {
                Session.SendPacket(this.GenerateNpcDialog((int)questToRemove.Quest.EndDialogId));
            }

            if (questToRemove.Quest.NextQuestId != null)
            {
                AddQuest((long)questToRemove.Quest.NextQuestId, questToRemove.IsMainQuest);
            }

            var dto = new QuestLogDTO
            {
                CharacterId = CharacterId, IpAddress = Session.CleanIpAddress, LastDaily = DateTime.Now,
                QuestId = questToRemove.Quest.QuestId
            };

            QuestLogs.Add(dto);

            //Session.SendPacket(GetSqst());

            #region Specialist Card Quest Reward

            switch (questId)
            {
                case 2007: // Pajama
                    {
                        Session.Character.GiftAdd(900, 1);
                    }
                    break;

                case 2013: // SP 1
                    {
                        switch (Session.Character.Class)
                        {
                            case ClassType.Swordsman:
                                Session.Character.GiftAdd(901, 1);
                                break;

                            case ClassType.Archer:
                                Session.Character.GiftAdd(903, 1);
                                break;

                            case ClassType.Magician:
                                Session.Character.GiftAdd(905, 1);
                                break;

                            case ClassType.MartialArtist:
                                Session.Character.GiftAdd(4486, 1);
                                break;
                        }
                    }
                    break;

                case 2020: // SP 2
                    {
                        switch (Session.Character.Class)
                        {
                            case ClassType.Swordsman:
                                Session.Character.GiftAdd(902, 1);
                                break;

                            case ClassType.Archer:
                                Session.Character.GiftAdd(904, 1);
                                break;

                            case ClassType.Magician:
                                Session.Character.GiftAdd(906, 1);
                                break;

                            case ClassType.MartialArtist:
                                Session.Character.GiftAdd(4485, 1);
                                break;
                        }
                    }
                    break;

                case 2095: // SP 3
                    {
                        switch (Session.Character.Class)
                        {
                            case ClassType.Swordsman:
                                Session.Character.GiftAdd(909, 1);
                                break;

                            case ClassType.Archer:
                                Session.Character.GiftAdd(911, 1);
                                break;

                            case ClassType.Magician:
                                Session.Character.GiftAdd(913, 1);
                                break;

                            case ClassType.MartialArtist:
                                Session.Character.GiftAdd(4437, 1);
                                break;
                        }
                    }
                    break;

                case 2134: // SP 4
                    {
                        switch (Session.Character.Class)
                        {
                            case ClassType.Swordsman:
                                Session.Character.GiftAdd(910, 1);
                                break;

                            case ClassType.Archer:
                                Session.Character.GiftAdd(912, 1);
                                break;

                            case ClassType.Magician:
                                Session.Character.GiftAdd(914, 1);
                                break;

                            case ClassType.MartialArtist:
                                Session.Character.GiftAdd(4532, 1);
                                break;
                        }
                    }
                    break;
            }

            #endregion
        }

        public bool RemoveSp(short vnum, bool forced)
        {
            SpRemoveConfirmation++;

            if (SpRemoveConfirmation == 1)
            {
                Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(o =>
                {
                    SpRemoveConfirmation = 0;
                });

                Session.SendPacket($"say 1 {Session.Character.CharacterId} 10 Press G again to remove your SP.");
                return false;
            }

            if (Session?.HasSession == true && SpRemoveConfirmation >= 2 && (!IsVehicled || forced))
            {
                if (Buff.Any(s => s.Card.BuffType == BuffType.Bad) && !forced)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_UNTRASFORM_WITH_DEBUFFS"), 0));
                    return false;
                }
                LastTransform = DateTime.Now;
                DisableBuffs(BuffType.All);

                EquipmentBCards.RemoveAll(s => s.ItemVNum.Equals(vnum));

                UseSp = false;
                LoadSpeed();
                Session.SendPacket(this.GenerateCond());
                Session.SendPacket(this.GenerateLev());

                if (IsInArenaLobby)
                {
                    SpCooldown = 10;
                }
                else
                {
                    SpCooldown = 30;
                }

                if (SkillsSp != null)
                {
                    foreach (CharacterSkill ski in SkillsSp.Where(s => !s.CanBeUsed(true)))
                    {
                        short time = ski.Skill.Cooldown;
                        double temp = (ski.LastUse - DateTime.Now).TotalMilliseconds + (time * 100);
                        temp /= 1000;
                        SpCooldown = temp > SpCooldown
                            ? (int)temp
                            : SpCooldown;
                    }
                }
                if (Authority >= AuthorityType.TMOD || forced)
                {
                    SpCooldown = 0;
                }
                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                {
                    SpCooldown = 10;
                }
                if (SpCooldown > 0)
                {
                    Session.SendPacket(this.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("STAY_TIME"), SpCooldown), 11));
                    Session.SendPacket($"sd {SpCooldown}");
                }
                Session.CurrentMapInstance?.Broadcast(this.GenerateCMode());
                Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(6, 1, CharacterId), PositionX, PositionY);

                Group grp = Session.Character?.Group;

                if (grp != null)
                {
                    grp.Sessions.ForEach(s =>
                    {
                        s.SendPacket(grp.GenerateRdlst());
                    });
                }

                // ms_c
                Session.SendPacket(this.GenerateSki());
                Session.SendPackets(this.GenerateQuicklist());
                Session.SendPacket(this.GenerateStat());
                Session.SendPackets(this.GenerateStatChar());
                BattleEntity.RemoveOwnedMonsters();
                Logger.Log.LogUserEvent("CHARACTER_SPECIALIST_RETURN", Session.GenerateIdentity(), $"SpCooldown: {SpCooldown}");
                if (SpCooldown > 0)
                {
                    Observable.Timer(TimeSpan.FromMilliseconds(SpCooldown * 1000)).SafeSubscribe(o =>
                    {
                        if (Session == null)
                        {
                            return;
                        }

                        Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("TRANSFORM_DISAPPEAR"), 11));
                        Session.SendPacket("sd 0");
                    });
                }
            }
            return true;
        }

        public void RemoveTemporalMates()
        {
            Mates.Where(s => s.IsTemporalMate).ToList().ForEach(m =>
            {
                m.GetInventory().ForEach(s =>
                {
                    Inventory.TryRemove(s.Id, out _);
                });
                Mates.Remove(m);
                byte i = 0;
                Mates.Where(s => s.MateType == MateType.Partner).ToList().ForEach(s =>
                {
                    s.GetInventory().ForEach(item => item.Type = (InventoryType)(13 + i));
                    s.PetId = i;
                    i++;
                });
                Session.SendPacket(UserInterfaceHelper.GeneratePClear());
                Session.SendPackets(this.GenerateScP());
                Session.SendPackets(this.GenerateScN());
                MapInstance.Broadcast(m.GenerateOut());
            });
        }

        public void RemoveUltimatePoints(short points)
        {
            UltimatePoints -= points;

            if (UltimatePoints < 0)
            {
                UltimatePoints = 0;
            }

            if (UltimatePoints < 3000)
            {
                RemoveBuff(729);
                RemoveBuff(727);
                AddBuff(new Buff(728, 10, false), BattleEntity);
            }

            if (UltimatePoints < 2000)
            {
                RemoveBuff(728);
                RemoveBuff(729);
                AddBuff(new Buff(727, 10, false), BattleEntity);
            }

            if (UltimatePoints < 1000)
            {
                RemoveBuff(727);
                RemoveBuff(728);
                RemoveBuff(729);
            }

            Session.SendPacket(this.GenerateFtPtPacket());
            Session.SendPackets(this.GenerateQuicklist());
        }

        public void RemoveVehicle()
        {
            RemoveBuff(336);
            ItemInstance sp = null;
            if (Inventory != null)
            {
                sp = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            IsVehicled = false;
            VehicleItem = null;
            LoadSpeed();
            if (UseSp)
            {
                if (sp != null)
                {
                    Morph = sp.Item.Morph;
                    MorphUpgrade = sp.Upgrade;
                    MorphUpgrade2 = sp.Design;
                }
            }
            else
            {
                Morph = 0;
            }
            Session.CurrentMapInstance?.Broadcast(this.GenerateCMode());
            Session.SendPacket(this.GenerateCond());
            LastSpeedChange = DateTime.Now;
        }

        public void ResetSkills()
        {
            Skills.ClearAll();

            switch ((byte)Class)
            {
                case 0:
                    {
                        LearnAdventurerSkills(true);
                    }
                    break;

                case 1:
                    {
                        Session.Character.AddSkill(220);
                        Session.Character.AddSkill(221);
                        Session.Character.AddSkill(235);
                    }
                    break;

                case 2:
                    {
                        Session.Character.AddSkill(240);
                        Session.Character.AddSkill(241);
                        Session.Character.AddSkill(236);
                    }
                    break;

                case 3:
                    {
                        Session.Character.AddSkill(260);
                        Session.Character.AddSkill(261);
                        Session.Character.AddSkill(237);
                    }
                    break;

                case 4:
                    {
                        Enumerable.Range(1525, 15).ToList().ForEach(skillVNum => Session.Character.AddSkill((short)skillVNum));
                        Session.Character.AddSkill(1565);
                    }
                    break;
            }

            if (!Session.Character.UseSp)
            {
                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPackets(Session.Character.GenerateQuicklist());
            }
        }

        public void Rest()
        {
            if (LastSkillUse.AddSeconds(4) > DateTime.Now || LastDefence.AddSeconds(4) > DateTime.Now)
            {
                return;
            }
            if (!IsVehicled)
            {
                IsSitting = !IsSitting;
                Session.CurrentMapInstance?.Broadcast(this.GenerateRest());
            }
            else
            {
                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("IMPOSSIBLE_TO_USE"), 10));
            }
        }

        private void SaveInventory()
        {
            if (Inventory == null)
            {
                return;
            }

            // Make a copy of the current inventory.
            //var inventoryCopy = Inventory.DeepCopy();

            lock (Inventory)
            {
                // load and concat inventory with equipment
                List<ItemInstance> inventories = Inventory.Values.ToList();

                // Load items for other characters
                IEnumerable<Guid> currentlySavedInventoryIds = DAOFactory.ItemInstanceDAO.LoadSlotAndTypeByCharacterId(CharacterId);
                IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);
                foreach (CharacterDTO characteraccount in characters.Where(s => s.CharacterId != CharacterId))
                {
                    currentlySavedInventoryIds = currentlySavedInventoryIds.Concat(DAOFactory.ItemInstanceDAO
                        .LoadByCharacterId(characteraccount.CharacterId).Where(s => s.Type == InventoryType.Warehouse)
                        .Select(i => i.Id).ToList());
                }

                // Loading miniland objects
                IEnumerable<MinilandObjectDTO> currentlySavedMinilandObjectEntries = DAOFactory.MinilandObjectDAO.LoadByCharacterId(CharacterId).ToList();
                foreach (MinilandObjectDTO mobjToDelete in currentlySavedMinilandObjectEntries.Except(MinilandObjects))
                {
                    DAOFactory.MinilandObjectDAO.DeleteById(mobjToDelete.MinilandObjectId);
                }

                var idsToDelete = currentlySavedInventoryIds.Except(inventories.Select(i => i.Id));
                Logger.Log.Debug("Deleting items");
                // Deleting already saved inventory from database
                DAOFactory.ItemInstanceDAO.DeleteGuidList(idsToDelete);

                Logger.Log.Debug("Saving Items");
                // create or update all which are new or do still exist
                ConcurrentBag<ItemInstance> saveInventory = new ConcurrentBag<ItemInstance>(inventories.Where(s =>
                    s.Type != InventoryType.Bazaar && s.Type != InventoryType.FamilyWareHouse));
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(saveInventory);


                Logger.Log.Debug("Saving equipment options");
                var allowedShellTypes = new[] {ItemType.Weapon, ItemType.Armor, ItemType.Shell};
                foreach (var item in saveInventory)
                {
                    if (allowedShellTypes.Any(s => s == item.Item.ItemType))
                    {
                        DAOFactory.ShellEffectDAO.InsertOrUpdateFromList(item.ShellEffects, item.EquipmentSerialId);
                    }

                    if (item.Item.ItemType == ItemType.Jewelery)
                    {
                        DAOFactory.CellonOptionDAO.InsertOrUpdateFromList(item.CellonOptions, item.EquipmentSerialId);
                    }
                }



                Logger.Log.Info($"Inventory of character {Name} have been correctly saved.");
            }
        }

        private void SaveMates()
        {
            //Mates
            IEnumerable<long> matesToDelete = DAOFactory.MateDAO.LoadByCharacterId(CharacterId).Select(s => s.MateId).Except(Mates?.ToList().Select(s => s.MateId));

            DAOFactory.MateDAO.DeleteFromList(matesToDelete);

            DAOFactory.MateDAO.InsertOrUpdateFromList(Mates.ToList());

            Logger.Log.Info($"Mates of character {Name} have been correctly saved.");
        }

        private void SaveQuests()
        {
            //Quest
            DAOFactory.CharacterQuestDAO.DeleteForCharacterId(CharacterId);
            DAOFactory.CharacterQuestDAO.InsertOrUpdateFromList(Quests);

            Logger.Log.Info($"Quests have been saved for character: {Name}");
        }

        private void SaveSkills()
        {
            if (Skills != null)
            {
                IEnumerable<Guid> currentlySavedCharacterSkills = DAOFactory.CharacterSkillDAO.LoadKeysByCharacterId(CharacterId).ToList();

                foreach (Guid characterSkillToDeleteId in currentlySavedCharacterSkills.Except(Skills.Select(s => s.Id)))
                {
                    DAOFactory.CharacterSkillDAO.Delete(characterSkillToDeleteId);
                }

                foreach (CharacterSkill characterSkill in Skills.GetAllItems())
                {
                    DAOFactory.CharacterSkillDAO.InsertOrUpdate(characterSkill);
                }
            }
        }

        private void SaveQuickList()
        {
            IEnumerable<QuicklistEntryDTO> quickListEntriesToInsertOrUpdate = QuicklistEntries.ToList();

            IEnumerable<Guid> currentlySavedQuicklistEntries = DAOFactory.QuicklistEntryDAO.LoadKeysByCharacterId(CharacterId).ToList();
            foreach (Guid quicklistEntryToDelete in currentlySavedQuicklistEntries.Except(QuicklistEntries.Select(s => s.Id)))
            {
                DAOFactory.QuicklistEntryDAO.Delete(quicklistEntryToDelete);
            }
            foreach (QuicklistEntryDTO quicklistEntry in quickListEntriesToInsertOrUpdate)
            {
                DAOFactory.QuicklistEntryDAO.InsertOrUpdate(quicklistEntry);
            }
        }

        private void SaveMiniland()
        {
            foreach (MinilandObjectDTO mobjEntry in (IEnumerable<MinilandObjectDTO>)MinilandObjects.ToList())
            {
                MinilandObjectDTO mobj = mobjEntry;
                DAOFactory.MinilandObjectDAO.InsertOrUpdate(ref mobj);
            }
        }

        private void SaveStaticBuff()
        {
            IEnumerable<short> currentlySavedBuff = DAOFactory.StaticBuffDAO.LoadByTypeCharacterId(CharacterId);
            foreach (short bonusToDelete in currentlySavedBuff.Except(Buff.Select(s => s.Card.CardId)))
            {
                DAOFactory.StaticBuffDAO.Delete(bonusToDelete, CharacterId);
            }
            if (_isStaticBuffListInitial)
            {
                foreach (Buff buff in Buff.Where(s => s.StaticBuff).ToArray())
                {
                    StaticBuffDTO bf = new StaticBuffDTO
                    {
                        CharacterId = CharacterId,
                        RemainingTime = (int)(buff.RemainingTime - (DateTime.Now - buff.Start).TotalSeconds),
                        CardId = buff.Card.CardId
                    };
                    DAOFactory.StaticBuffDAO.InsertOrUpdate(ref bf);
                }
            }
        }

        private void SaveBattlePassItemLogs()
        {
            DAOFactory.BattlePassItemLogsDAO.InsertOrUpdateFromList(BattlePassItemLogs);
            Logger.Log.Info("BattlePassItemLogs are saved");
        }

        private void SaveBattlePassQuestLogs()
        {
            DAOFactory.BattlePassQuestLogsDAO.InsertOrUpdateFromList(BattlePassQuestLogs);
            Logger.Log.Info("BattlePassQuestLogs are saved");
        }

        private void SaveTitles()
        {
            //Title
            IEnumerable<long> currentlySavedTitles = DAOFactory.CharacterTitleDAO.
                LoadByCharacterId(CharacterId).Select(s => s.CharacterTitleId);

            foreach (long TitleToDeleteId in currentlySavedTitles.Except(Title.Select(s => s.CharacterTitleId)))
            {
                DAOFactory.CharacterTitleDAO.Delete(TitleToDeleteId);
            }

            foreach (var tit in Title)
            {
                CharacterTitleDTO titsave = tit;
                DAOFactory.CharacterTitleDAO.InsertOrUpdate(ref titsave);
            }
        }

        private void SaveStaticBonuses()
        {
            foreach (StaticBonusDTO bonus in StaticBonusList.ToArray())
            {
                StaticBonusDTO bonus2 = bonus;
                DAOFactory.StaticBonusDAO.InsertOrUpdate(ref bonus2);
            }
        }

        private void SaveGeneralLog()
        {
            foreach (GeneralLogDTO general in GeneralLogs.GetAllItems())
            {
                if (!DAOFactory.GeneralLogDAO.IdAlreadySet(general.LogId))
                {
                    DAOFactory.GeneralLogDAO.Insert(general);
                }
            }

        }

        private void SaveRespawns()
        {
            foreach (RespawnDTO Resp in Respawns)
            {
                RespawnDTO res = Resp;
                if (Resp.MapId != 0 && Resp.X != 0 && Resp.Y != 0)
                {
                    DAOFactory.RespawnDAO.InsertOrUpdate(ref res);
                }
            }
        }

        private void SaveLogs()
        {
            //Quest logs
            var alreadySavedLogs = DAOFactory.QuestLogDAO.LoadByCharacterId(CharacterId);

            var toAdd = new List<QuestLogDTO>();
            foreach (var log in QuestLogs.GetAllItems())
            {
                if (!alreadySavedLogs.Any(s => s.Id == log.Id))
                {
                    toAdd.Add(log);
                }
            }

            DAOFactory.QuestLogDAO.InsertOrUpdateFromList(toAdd);
        }

        private void SaveInstantBattleLogs()
        {
            var alreadySavedLogs = DAOFactory.InstantBattleLogDAO.LoadByCharacterId(CharacterId);

            var toAdd = new List<InstantBattleLogDTO>();
            foreach (var log in InstantBattleLogs.GetAllItems())
            {
                if (!alreadySavedLogs.Any(s => s.CharacterId == log.CharacterId && s.DateTime == log.DateTime))
                {
                    toAdd.Add(log);
                }
            }

            DAOFactory.InstantBattleLogDAO.InsertOrUpdateFromList(toAdd);
        }

        public void Save()
        {
            if (Session?.Account == null)
            {
                return;
            }

            Logger.Log.LogUserEvent("CHARACTER_DB_SAVE", Session.GenerateIdentity(), "START");
            try
            {
                LastSave = DateTime.Now;
                AccountDTO account = Session.Account;
                var accountId = account.AccountId;
                DAOFactory.AccountDAO.InsertOrUpdate(ref account);

                CharacterDTO character = DeepCopy();
                DAOFactory.CharacterDAO.InsertOrUpdate(ref character);

                SaveInventory();

                SaveMates();

                SaveQuests();

                SaveLogs();

                SaveSkills();

                SaveQuickList();

                SaveMiniland();

                SaveTitles();

                SaveStaticBonuses();

                SaveGeneralLog();

                SaveRespawns();

                SaveInstantBattleLogs();

                SaveBattlePassItemLogs();

                SaveBattlePassQuestLogs();

                Logger.Log.LogUserEvent("CHARACTER_DB_SAVE", Session.GenerateIdentity(), "FINISH");
            }
            catch (Exception e)
            {
                Logger.Log.Error("ERROR", e);
            }
        }

        public void SendGift(long id, short vnum, short amount, sbyte rare, byte upgrade, short design, bool isNosmall)
        {
            Item it = ServerManager.GetItem(vnum);

            if (it != null)
            {
                if (it.ItemType != ItemType.Weapon && it.ItemType != ItemType.Armor && it.ItemType != ItemType.Specialist && it.EquipmentSlot != EquipmentType.Gloves && it.EquipmentSlot != EquipmentType.Boots)
                {
                    upgrade = 0;
                }
                else if (it.ItemType != ItemType.Weapon && it.ItemType != ItemType.Armor)
                {
                    rare = 0;
                }
                if (rare > 8 || rare < -2)
                {
                    rare = 0;
                }
                if (upgrade > 10 && it.ItemType != ItemType.Specialist)
                {
                    upgrade = 0;
                }
                else if (it.ItemType == ItemType.Specialist && upgrade > 15)
                {
                    upgrade = 0;
                }

                // maximum size of the amount is 9999
                if (amount > 9999)
                {
                    amount = 9999;
                }

                MailDTO mail = new MailDTO
                {
                    AttachmentAmount = it.Type == InventoryType.Etc || it.Type == InventoryType.Main ? amount : (short)1,
                    IsOpened = false,
                    Date = DateTime.Now,
                    ReceiverId = id,
                    SenderId = CharacterId,
                    AttachmentRarity = (byte)rare,
                    AttachmentUpgrade = upgrade,
                    AttachmentDesign = design,
                    IsSenderCopy = false,
                    Title = isNosmall ? "NOSMALL" : Name,
                    AttachmentVNum = vnum,
                    SenderClass = Class,
                    SenderGender = Gender,
                    SenderHairColor = HairColor,
                    SenderHairStyle = HairStyle,
                    EqPacket = this.GenerateEqListForPacket(),
                    SenderMorphId = Morph == 0 ? (short)-1 : (short)(Morph > short.MaxValue ? 0 : Morph)
                };
                MailServiceClient.Instance.SendMail(mail);
            }
        }

        public void SetRespawnPoint(short mapId, short mapX, short mapY)
        {
            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Count > 0)
            {
                long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes[0].RespawnMapTypeId;
                if (respawnmaptype != null)
                {
                    RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == respawnmaptype);
                    if (resp == null)
                    {
                        resp = new RespawnDTO { CharacterId = CharacterId, MapId = mapId, X = mapX, Y = mapY, RespawnMapTypeId = (long)respawnmaptype };
                        Respawns.Add(resp);
                    }
                    else
                    {
                        resp.X = mapX;
                        resp.Y = mapY;
                        resp.MapId = mapId;
                    }
                }
            }
        }

        public void SetReturnPoint(short mapId, short mapX, short mapY)
        {
            if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.Map.MapTypes.Count > 0)
            {
                long? respawnmaptype = Session.CurrentMapInstance.Map.MapTypes[0].ReturnMapTypeId;
                if (respawnmaptype != null)
                {
                    RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == respawnmaptype);
                    if (resp == null)
                    {
                        resp = new RespawnDTO { CharacterId = CharacterId, MapId = mapId, X = mapX, Y = mapY, RespawnMapTypeId = (long)respawnmaptype };
                        Respawns.Add(resp);
                    }
                    else
                    {
                        resp.X = mapX;
                        resp.Y = mapY;
                        resp.MapId = mapId;
                    }
                }
            }
            else if (Session.HasCurrentMapInstance && Session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
            {
                RespawnDTO resp = Respawns.Find(s => s.RespawnMapTypeId == 1);
                if (resp == null)
                {
                    resp = new RespawnDTO { CharacterId = CharacterId, MapId = mapId, X = mapX, Y = mapY, RespawnMapTypeId = 1 };
                    Respawns.Add(resp);
                }
                else
                {
                    resp.X = mapX;
                    resp.Y = mapY;
                    resp.MapId = mapId;
                }
            }
        }

        public void SetSeal()
        {
            Observable.Timer(TimeSpan.FromMilliseconds(2000)).SafeSubscribe(o =>
            {
                Hp = 0;
                Mp = 0;
                MapInstance.Broadcast(this.GenerateRevive());
                MapInstance.Broadcast(Session, $"c_mode 1 {CharacterId} 1564 0 0 0");
                
            });

            IsSeal = true;
            SealDisposable?.Dispose();
            SealDisposable = Observable.Timer(TimeSpan.FromMilliseconds(30000)).SafeSubscribe(o =>
            {
                if (Session == null)
                {
                    return;
                }

                short x = (short)(39 + ServerManager.RandomNumber(-2, 3));
                short y = (short)(42 + ServerManager.RandomNumber(-2, 3));

                IsSeal = false;

                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                if (Faction == FactionType.Angel)
                {
                    ServerManager.Instance.ChangeMap(CharacterId, 130, x, y);
                }
                else if (Faction == FactionType.Demon)
                {
                    ServerManager.Instance.ChangeMap(CharacterId, 131, x, y);
                }
                else
                {
                    MapId = 145;
                    MapX = 51;
                    MapY = 41;
                    string connection =
                        CommunicationServiceClient.Instance.RetrieveOriginWorld(Session.Account.AccountId);
                    if (string.IsNullOrWhiteSpace(connection))
                    {
                        return;
                    }

                    int port = Convert.ToInt32(connection.Split(':')[1]);
                    Session.Character.ChangeChannel(connection.Split(':')[0], port, 3);
                    return;
                }

                MapInstance?.Broadcast(Session, this.GenerateTp());
                MapInstance?.Broadcast(this.GenerateRevive());
                Session.SendPacket(this.GenerateStat());
            });
        }

        public void StandUp()
        {
            if (!IsVehicled && IsSitting)
            {
                IsSitting = false;
                MapInstance?.Broadcast(this.GenerateRest());
            }
        }

        public void TeleportToDir(int Dir, int Distance)
        {
            WalkDisposable?.Dispose();
            short NewX = PositionX;
            short NewY = PositionY;
            bool BlockedZone = false;
            for (short i = 1; Map.GetDistance(new MapCell { X = PositionX, Y = PositionY }, new MapCell { X = NewX, Y = NewY }) < Math.Abs(Distance) && i < +Math.Abs(Distance) + 5 && !BlockedZone; i++)
            {
                switch (Dir)
                {
                    case 0:
                        if (!MapInstance.Map.IsBlockedZone(NewX, (short)(NewY - i)))
                        {
                            NewX = PositionX;
                            NewY = (short)(PositionY - i);
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 1:
                        if (!MapInstance.Map.IsBlockedZone((short)(NewX + i), NewY))
                        {
                            NewX = (short)(PositionX + i);
                            NewY = PositionY;
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 2:
                        if (!MapInstance.Map.IsBlockedZone(NewX, (short)(NewY + i)))
                        {
                            NewX = PositionX;
                            NewY = (short)(PositionY + i);
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 3:
                        if (!MapInstance.Map.IsBlockedZone((short)(NewX - i), NewY))
                        {
                            NewX = (short)(PositionX - i);
                            NewY = PositionY;
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 4:
                        if (!MapInstance.Map.IsBlockedZone((short)(NewX - i), (short)(NewY - i)))
                        {
                            NewX = (short)(PositionX - i);
                            NewY = (short)(PositionY - i);
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 5:
                        if (!MapInstance.Map.IsBlockedZone((short)(NewX + i), (short)(NewY - i)))
                        {
                            NewX = (short)(PositionX + i);
                            NewY = (short)(PositionY - i);
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 6:
                        if (!MapInstance.Map.IsBlockedZone((short)(NewX + i), (short)(NewY + i)))
                        {
                            NewX = (short)(PositionX + i);
                            NewY = (short)(PositionY + i);
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;

                    case 7:
                        if (!MapInstance.Map.IsBlockedZone((short)(NewX - i), (short)(NewY + i)))
                        {
                            NewX = (short)(PositionX - i);
                            NewY = (short)(PositionY + i);
                        }
                        else
                        {
                            BlockedZone = true;
                        }
                        break;
                }
            }
            PositionX = NewX;
            PositionY = NewY;
            MapInstance.Broadcast(this.GenerateTp());
        }

        public bool WeaponLoaded(CharacterSkill ski)
        {
            if (ski != null)
            {
                switch (Class)
                {
                    default:
                        return false;

                    case ClassType.Adventurer:
                        if (ski.Skill.Type == 1 && Inventory != null)
                        {
                            ItemInstance wearable = Inventory.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            if (wearable != null)
                            {
                                if (wearable.Ammo > 0)
                                {
                                    wearable.Ammo--;
                                    return true;
                                }
                                if (Inventory.CountItem(2081) < 1)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ADVENTURER"), 10));
                                    return false;
                                }
                                Inventory.RemoveItemAmount(2081);
                                wearable.Ammo = 100;
                                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ADVENTURER"), 10));
                                return true;
                            }
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                            return false;
                        }
                        return true;

                    case ClassType.Swordsman:
                        if (ski.Skill.Type == 1 && Inventory != null)
                        {
                            ItemInstance inv = Inventory.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            if (inv != null)
                            {
                                if (inv.Ammo > 0)
                                {
                                    inv.Ammo--;
                                    return true;
                                }
                                if (Inventory.CountItem(2082) < 1)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_SWORDSMAN"), 10));
                                    return false;
                                }

                                Inventory.RemoveItemAmount(2082);
                                inv.Ammo = 100;
                                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_SWORDSMAN"), 10));
                                return true;
                            }
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                            return false;
                        }
                        return true;

                    case ClassType.Archer:
                        if (ski.Skill.Type == 1 && Inventory != null)
                        {
                            ItemInstance inv = Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                            if (inv != null)
                            {
                                if (inv.Ammo > 0)
                                {
                                    inv.Ammo--;
                                    return true;
                                }
                                if (Inventory.CountItem(2083) < 1)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_AMMO_ARCHER"), 10));
                                    return false;
                                }

                                Inventory.RemoveItemAmount(2083);
                                inv.Ammo = 100;
                                Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("AMMO_LOADED_ARCHER"), 10));
                                return true;
                            }
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                            return false;
                        }
                        return true;

                    case ClassType.Magician:
                        if (ski.Skill.Type == 1 && Inventory != null)
                        {
                            ItemInstance inv = Inventory.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                            if (inv == null)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WEAPON"), 10));
                                return false;
                            }
                        }
                        return true;

                    case ClassType.MartialArtist:
                        return true;
                }
            }

            return false;
        }

        internal void RefreshValidity()
        {
            lock (StaticBonusList)
            {
                // check static bonus list
                foreach (var bonus in StaticBonusList.Where(bonus => bonus.DateEnd < DateTime.Now))
                {
                    StaticBonusList.Remove(bonus);
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                    Session.SendPacket(this.GenerateExts());
                }

                if (StaticBonusList.RemoveAll(s => s.DateEnd < DateTime.Now) > 0)
                {
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                }

                if (Inventory == null)
                {
                    return;
                }

                foreach (var suit in Enum.GetValues(typeof(EquipmentType)))
                {
                    var item = Inventory.LoadBySlotAndType((byte)suit, InventoryType.Wear);
                    if (!(item?.DurabilityPoint > 0) || item.Item.EquipmentSlot == EquipmentType.Amulet)
                    {
                        continue;
                    }

                    item.DurabilityPoint--;
                    if (item.DurabilityPoint != 0)
                    {
                        continue;
                    }

                    Inventory.DeleteById(item.Id);
                    Session.SendPackets(this.GenerateStatChar());
                    Session.SendPacket(this.GenerateEquipment());
                    Session.CurrentMapInstance?.Broadcast(this.GenerateEq());
                    Session.SendPacket(this.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_TIMEOUT"), 10));
                }
            }
        }

        internal void SetSession(ClientSession clientSession) => Session = clientSession;

        private void GenerateHeroXpLevelUp()
        {

            double t = HeroXPLoad();
            while (HeroXp >= t)
            {
                HeroXp -= (long)t;
                HeroLevel++;
                t = HeroXPLoad();
                if (HeroLevel >= ServerManager.Instance.Configuration.MaxHeroLevel)
                {
                    HeroLevel = ServerManager.Instance.Configuration.MaxHeroLevel;
                    HeroXp = 0;
                }
                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(this.GenerateStat());

                if (Family != null)
                {
                    Family.InsertFamilyLog(FamilyLogType.ChampionLevelUp, Name, level: HeroLevel);
                    ServerManager.Instance.FamilyRefresh(Family.FamilyId);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                    {
                        DestinationCharacterId = Family.FamilyId,
                        SourceCharacterId = CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = "fhis_stc",
                        Type = MessageType.Family
                    });
                }

                Session.SendPacket(this.GenerateLevelUp());
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HERO_LEVELUP"), 0));
                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 8), PositionX, PositionY);
                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 198), PositionX, PositionY);
            }
        }

        private void GenerateJobXpLevelUp()
        {
            ItemInstance amulet = Inventory.LoadBySlotAndType((byte)EquipmentType.Amulet, InventoryType.Wear);

            if (amulet?.ItemVNum == 20052)
            {
                return;
            }

            var t = JobXPLoad();
            while (JobLevelXp >= t)
            {
                JobLevelXp -= (long)t;
                JobLevel++;
                t = JobXPLoad();
                if (JobLevel >= 20 && Class == 0)
                {
                    JobLevel = 20;
                    JobLevelXp = 0;
                }
                else if (JobLevel >= ServerManager.Instance.Configuration.MaxJobLevel)
                {
                    JobLevel = ServerManager.Instance.Configuration.MaxJobLevel;
                    JobLevelXp = 0;
                }

                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(this.GenerateStat());
                Session.SendPacket(this.GenerateLevelUp());
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("JOB_LEVELUP"), 0));
                LearnAdventurerSkills();
                Session.SendPackets(this.GenerateQuicklist());
                Session.CurrentMapInstance?.Broadcast(this.GenerateEff(8), PositionX, PositionY);
                Session.CurrentMapInstance?.Broadcast(this.GenerateEff(198), PositionX, PositionY);
            }
        }

        public void SendLevelUpRewards()
        {
            var rewards = ServerManager.Instance.LevelUpRewards.Where(s => s.RequiredLevel.Equals(Level));

            foreach (var reward in rewards)
            {
                switch (reward.Type)
                {
                    case LevelupRewardType.Gold:
                        GetGold(reward.Value);
                        break;
                    case LevelupRewardType.Item:
                        GiftAdd((short)reward.Value, reward.Amount);
                        break;
                }
            }
        }

        private void GenerateLevelXpLevelUp()
        {
            var t = XpLoad();
            while (LevelXp >= t)
            {
                LevelXp -= (long)t;
                Level++;
                SendLevelUpRewards();
                t = XpLoad();
                if (Level >= ServerManager.Instance.Configuration.MaxLevel)
                {
                    Level = ServerManager.Instance.Configuration.MaxLevel;
                    LevelXp = 0;
                }

                if (Level == ServerManager.Instance.Configuration.HeroicStartLevel && HeroLevel == 0)
                {
                    HeroLevel = 1;
                    HeroXp = 0;
                }

                Hp = (int)HPLoad();
                Mp = (int)MPLoad();
                Session.SendPacket(this.GenerateStat());
                if (Family != null)
                {
                    if (Level > 20 && (Level % 10) == 0)
                    {
                        Family.InsertFamilyLog(FamilyLogType.LevelUp, Name, level: Level);
                        GenerateFamilyXp(20 * Level);
                    }
                    else if (Level > 80)
                    {
                        Family.InsertFamilyLog(FamilyLogType.LevelUp, Name, level: Level);
                    }
                    else
                    {
                        ServerManager.Instance.FamilyRefresh(Family.FamilyId);
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage()
                        {
                            DestinationCharacterId = Family.FamilyId,
                            SourceCharacterId = CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = "fhis_stc",
                            Type = MessageType.Family
                        });
                    }
                }

                Session.SendPacket(this.GenerateLevelUp());
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LEVELUP"), 0));
                Session.CurrentMapInstance?.Broadcast(this.GenerateEff(6), PositionX, PositionY);
                Session.CurrentMapInstance?.Broadcast(this.GenerateEff(198), PositionX, PositionY);
                ServerManager.Instance.UpdateGroup(CharacterId);

                if (Session.Character.Level == 25)
                {
                    // Add Bob
                    AddMate(317, 24, MateType.Partner);
                }

                if (Session.Character.Level == 32)
                {
                    // Add Tom
                    AddMate(318, 31, MateType.Partner);
                }

                if (Session.Character.Level == 49)
                {
                    // Add Kliff
                    AddMate(319, 48, MateType.Partner);
                }

                if (Level >= 20)
                {
                    GetReputation(500);
                }
            }
        }

        private void GenerateSpXpLevelUp(ItemInstance specialist)
        {
            double t = SpXpLoad();
            while (UseSp && specialist.XP >= t)
            {
                specialist.XP -= (long)t;
                specialist.SpLevel++;
                t = SpXpLoad();
                Session.SendPacket(this.GenerateStat());
                Session.SendPacket(this.GenerateLevelUp());
                if (specialist.SpLevel >= ServerManager.Instance.Configuration.MaxSPLevel)
                {
                    specialist.SpLevel = ServerManager.Instance.Configuration.MaxSPLevel;
                    specialist.XP = 0;
                }
                LearnSPSkill();
                SkillsSp.ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
                Session.SendPacket(this.GenerateSki());
                Session.SendPackets(this.GenerateQuicklist());

                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SP_LEVELUP"), 0));
                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 8), PositionX, PositionY);
                Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 198), PositionX, PositionY);
            }
        }

        private void GenerateXp(MapMonster monster, Dictionary<BattleEntity, long> damageList = null, bool isGroupMember = false)
        {
            if (monster?.DamageList == null)
            {
                return;
            }

            if (Session?.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
            {
                return;
            }

            ItemInstance amulet = Inventory.LoadBySlotAndType((byte)EquipmentType.Amulet, InventoryType.Wear);

            if (amulet?.ItemVNum == 20052)
            {
                return;
            }

            bool isKiller = false;

            if (damageList == null)
            {
                damageList = new Dictionary<BattleEntity, long>();

                lock (monster.DamageList)
                {
                    // Deep copy monster.DamageList to damageList.

                    foreach (KeyValuePair<BattleEntity, long> keyValuePair in monster.DamageList)
                    {
                        damageList.Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }

                isKiller = true;
            }

            Group grp = null;

            if (Group?.GroupType == GroupType.Group)
            {
                grp = Group;
            }

            bool checkMonsterOwner(long entityId, Group group)
            {
                if (damageList.FirstOrDefault(s => s.Value > 0).Key is BattleEntity monsterOwner)
                {
                    return monsterOwner.MapEntityId == entityId || monsterOwner.Mate?.Owner?.CharacterId == entityId || monsterOwner.MapMonster?.Owner?.MapEntityId == entityId || group != null && group.IsMemberOfGroup(monsterOwner.MapEntityId);
                }

                return false;
            }

            bool isMonsterOwner = checkMonsterOwner(CharacterId, grp);

            lock (monster.DamageList)
            {
                if (monster.DamageList.Any())
                {
                    monster.DamageList.Where(s => s.Key.MapEntityId == CharacterId).ToList().ForEach(s => monster.DamageList.TryRemove(s.Key, out _));

                    // Call GenerateXp() for group members.

                    if (grp?.Sessions != null && !isGroupMember)
                    {
                        foreach (ClientSession groupMember in grp.Sessions.GetAllItems().Where(g => g.Character != null && g.Character.CharacterId != CharacterId && g.Character.MapInstanceId == MapInstanceId).ToList())
                        {
                            try
                            {
                                groupMember.Character?.GenerateXp(monster, damageList, true);
                            }
                            catch (Exception e)
                            {
                                Logger.Log.Error(null, e);
                            }
                        }
                    }
                }

                // Call GenerateXp() for others.

                if (monster.DamageList.Any() && isKiller)
                {
                    try
                    {
                        monster.DamageList.Where(s => s.Value > 0 && s.Key.MapEntityId != BattleEntity.MapEntityId).ToList().ForEach(s => s.Key.Character?.GenerateXp(monster, damageList));
                    }
                    catch (Exception e)
                    {
                        Logger.Log.Error(null, e);
                    }
                }
            }

            // Exp percent regarding the damge
            double totalDamage = damageList.Sum(s => s.Value);
            double damageByCharacterOrGroup = damageList.Where(s => s.Key != null && s.Key.MapEntityId == CharacterId || Mates.Any(m => m.MateTransportId == s.Key.MapEntityId) || grp != null && grp.IsMemberOfGroup(s.Key.MapEntityId)).Sum(s => s.Value);
            double expDamageRate = damageByCharacterOrGroup / totalDamage * (isMonsterOwner && damageList.Any(s => s.Key != null && s.Value > 0 && s.Key.MapEntityId != CharacterId && (grp == null || !grp.IsMemberOfGroup(s.Key.MapEntityId))) ? 1.2f : 1);

            if (double.IsNaN(expDamageRate))
            {
                expDamageRate = 0;
            }

            NpcMonster monsterInfo = monster.Monster;

            if (!Session.Account.PenaltyLogs.Any(s => s.Penalty == PenaltyType.BlockExp && s.DateEnd > DateTime.Now))
            {
                if (Hp <= 0)
                {
                    return;
                }

                if ((int)(LevelXp / (XpLoad() / 10)) < (int)((LevelXp + monsterInfo.XP * expDamageRate) / (XpLoad() / 10)))
                {
                    Hp = (int)HPLoad();
                    Mp = (int)MPLoad();
                    Session.SendPacket(this.GenerateStat());
                    Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, CharacterId, 5));
                }

                ItemInstance specialist = null;

                if (Inventory != null)
                {
                    specialist = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                }

                double medalBonus = 0;

                if (this.HasStaticBonus(StaticBonusType.EreniaMedal))
                {
                    medalBonus += 0.15d;
                }

                if (this.HasStaticBonus(StaticBonusType.AdventurerMedal))
                {
                    medalBonus += 0.15d;
                }

                int xp = (int)(GetXP(monster, grp) * expDamageRate * (isMonsterOwner ? 1 : 0.8f) * (1 + (GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D)) * (1 + (GetBuff(CardType.DragonSkills, (byte)AdditionalTypes.DragonSkills.IncreaseBattleAndJobExperience)[0] / 100)) * (1 + (GetBuff(CardType.UpdateChampionExp, (byte)AdditionalTypes.UpdateChampionExp.IncreaseChampionExp)[0] / 100)));
                xp = (int) (xp * (1 + medalBonus));

                if (Level < ServerManager.Instance.Configuration.MaxLevel)
                {
                    LevelXp += xp;
                }

                foreach (Mate mate in Mates.Where(x => x.IsTeamMember && x.IsAlive))
                {
                    mate.GenerateXp(xp);

                    if (mate.IsUsingSp)
                    {
                        mate.Sp.AddXp(xp);
                        mate.Owner?.Session?.SendPacket(mate.GenerateScPacket());
                    }
                }

                if ((Class == 0 && JobLevel < 20) || (Class != 0 && JobLevel < ServerManager.Instance.Configuration.MaxJobLevel))
                {
                    if (specialist != null && UseSp && specialist.SpLevel < ServerManager.Instance.Configuration.MaxSPLevel && specialist.SpLevel > 19)
                    {
                        JobLevelXp += (int)(GetJXP(monster, grp) * expDamageRate * (isMonsterOwner ? 1 : 0.8f) / 2D * (1 + (GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D)) * (1 + (GetBuff(CardType.DragonSkills, (byte)AdditionalTypes.DragonSkills.IncreaseBattleAndJobExperience)[0] / 100)));
                    }
                    else
                    {
                        JobLevelXp += (int)(GetJXP(monster, grp) * expDamageRate * (isMonsterOwner ? 1 : 0.8f) * (1 + (GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D)) * (1 + (GetBuff(CardType.DragonSkills, (byte)AdditionalTypes.DragonSkills.IncreaseBattleAndJobExperience)[0] / 100)));
                    }
                }

                if (specialist != null && UseSp && specialist.SpLevel < ServerManager.Instance.Configuration.MaxSPLevel)
                {
                    int multiplier = specialist.SpLevel < 10 ? 10 : specialist.SpLevel < 19 ? 5 : 1;

                    specialist.XP += (int)(GetJXP(monster, grp) * expDamageRate * (multiplier + (GetBuff(CardType.Item, (byte)AdditionalTypes.Item.EXPIncreased)[0] / 100D + (GetBuff(CardType.Item, (byte)AdditionalTypes.Item.IncreaseSPXP)[0] / 100D))));
                }

                ItemInstance fairy = Inventory?.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);

                if (fairy != null)
                {
                    if (fairy.ElementRate + fairy.Item.ElementRate < fairy.Item.MaxElementRate
                        && Level <= monsterInfo.Level + 15 && Level >= monsterInfo.Level - 15)
                    {
                        var rate = 1;
                        if (Buff.ContainsKey(393))
                        {
                            rate = 2;
                        }

                        fairy.XP += ServerManager.Instance.Configuration.RateFairyXP * rate;
                    }

                    double experience = CharacterHelper.LoadFairyXPData(fairy.ElementRate + fairy.Item.ElementRate);

                    while (fairy.XP >= experience)
                    {
                        fairy.XP -= (int)experience;
                        fairy.ElementRate++;

                        if (fairy.ElementRate + fairy.Item.ElementRate == fairy.Item.MaxElementRate)
                        {
                            fairy.XP = 0;

                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRYMAX"), fairy.Item.Name), 10));
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVELUP"), fairy.Item.Name), 10));
                        }

                        Session.SendPacket(this.GeneratePairy());
                    }
                }

                if (HeroLevel > 0 && HeroLevel < ServerManager.Instance.Configuration.MaxHeroLevel && monster.MapId >= 228 && monster.MapId <= 250)
                {
                    HeroXp += monster.Monster.XP / 25;
                }

                GenerateLevelXpLevelUp();
                GenerateJobXpLevelUp();

                if (specialist != null)
                {
                    GenerateSpXpLevelUp(specialist);
                }

                GenerateHeroXpLevelUp();

                Session.SendPacket(this.GenerateLev());
            }
        }

        private int GetGold(MapMonster mapMonster)
        {
            if (MapId == 2006 || MapId == 150)
            {
                return 0;
            }
            int lowBaseGold = ServerManager.RandomNumber(6 * mapMonster.Monster?.Level ?? 1, 12 * mapMonster.Monster?.Level ?? 1);
            int actMultiplier = Session?.CurrentMapInstance?.Map.MapTypes?.Any(s => s.MapTypeId == (short)MapTypeEnum.Act52) ?? false ? 10 : 1;
            if (Session?.CurrentMapInstance?.Map.MapTypes?.Any(s => s.MapTypeId == (short)MapTypeEnum.Act61 || s.MapTypeId == (short)MapTypeEnum.Act61A || s.MapTypeId == (short)MapTypeEnum.Act61D) == true)
            {
                actMultiplier = 2;
            }
            return (int)(lowBaseGold * actMultiplier);
        }

        internal int GetHXP(MapMonster mapMonster, Group group)
        {
            if (Session.Character.Level > 0)
            {
                return 0;
            }

            if (HeroLevel >= ServerManager.Instance.Configuration.MaxHeroLevel)
            {
                return 0;
            }

            NpcMonster npcMonster = mapMonster.Monster;

            int partySize = group?.GroupType == GroupType.Group ? group.Sessions.ToList().Count(s => s?.Character != null && s.Character.MapInstance == mapMonster.MapInstance && s.Character.HeroLevel > 0 && s.Character.HeroLevel < ServerManager.Instance.Configuration.MaxHeroLevel) : 1;

            if (partySize < 1)
            {
                partySize = 1;
            }

            double sharedHXp = npcMonster.XP / 25 / partySize;

            double memberHXp = sharedHXp * CharacterHelper.ExperiencePenalty(Level, npcMonster.Level) * ServerManager.Instance.Configuration.RateHeroicXP;

            return (int)memberHXp;
        }

        private int GetJXP(MapMonster mapMonster, Group group)
        {
            if (Session.Character.Level > 0)
            {
                return 0;
            }

            NpcMonster npcMonster = mapMonster.Monster;

            int partySize = group?.GroupType != GroupType.Group ? 1 : group.Sessions.ToList().Count(s =>
            {
                if (s?.Character == null
                    || s.Character.MapInstance != mapMonster.MapInstance)
                {
                    return false;
                }

                if (!s.Character.UseSp)
                {
                    return s.Character.JobLevel < (s.Character.Class == 0 ? 20 : ServerManager.Instance.Configuration.MaxJobLevel);
                }

                ItemInstance sp = s.Character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);

                if (sp != null)
                {
                    return sp.SpLevel < ServerManager.Instance.Configuration.MaxSPLevel;
                }

                return false;
            });

            if (partySize < 1)
            {
                partySize = 1;
            }

            double sharedJXp = Session.Character.UseSp ? (double)npcMonster.JobXP / 3 / partySize : (double)npcMonster.JobXP / partySize;

            double memberJxp = sharedJXp * CharacterHelper.ExperiencePenalty(Level, npcMonster.Level) * (ServerManager.Instance.Configuration.RateJXP);

            return (int)memberJxp;
        }

        private int GetShellArmorEffectValue(ShellArmorEffectType effectType)
        {
            return ShellEffectArmor.FirstOrDefault(s => s.Effect == (byte)effectType)?.Value ?? 0;
        }

        private int GetXP(MapMonster mapMonster, Group group)
        {
            if (Session.Character.Level > 0)
            {
                return 0;
            }

            NpcMonster npcMonster = mapMonster.Monster;

            int partySize = group?.GroupType == GroupType.Group ? group.Sessions.ToList().Count(s => s?.Character != null && s.Character.MapInstance == mapMonster.MapInstance && s.Character.Level < ServerManager.Instance.Configuration.MaxLevel) : 1;

            if (partySize < 1)
            {
                partySize = 1;
            }

            double sharedXp = (double)npcMonster.XP / partySize;

            if (npcMonster.Level >= 80)
            {
                sharedXp *= 3;
            }
            else if (npcMonster.Level >= 75)
            {
                sharedXp *= 2;
            }

            int lvlDifference = Level - npcMonster.Level;

            double memberXp = (lvlDifference < 5 ? sharedXp : (sharedXp / 3 * 2)) * CharacterHelper.ExperiencePenalty(Level, npcMonster.Level) * (ServerManager.Instance.Configuration.RateXP + MapInstance.XpRate);

            if (Level <= 5 && lvlDifference < -4)
            {
                memberXp *= 1.5;
            }

            return (int)memberXp;
        }

        internal int HealthHPLoad()
        {
            if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance)
            {
                int naturalRecovery = 1;
                if (Skills != null)
                {
                    naturalRecovery += Skills.Where(s => s.Skill.SkillType == 0 && s.Skill.CastId == 10).Sum(s => s.Skill.UpgradeSkill);
                }
                if (IsSitting)
                {
                    int regen = GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryIncreased)[0];
                    return (int)((regen + CharacterHelper.HPHealth[(byte)Class] + CellonOptions.Where(s => s.Type == CellonOptionType.HPRestore).Sum(s => s.Value)) * (1 + GetShellArmorEffectValue(ShellArmorEffectType.RecoveryHPOnRest) / 100D));
                }
                return (DateTime.Now - LastDefence).TotalSeconds > 4 ? (int)((CharacterHelper.HPHealthStand[(byte)Class] * (1 + GetShellArmorEffectValue(ShellArmorEffectType.RecoveryHP) / 100D)) * naturalRecovery) : 0;
            }
            return 0;
        }

        internal int HealthMPLoad()
        {
            if (Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance)
            {
                int naturalRecovery = 1;
                if (Skills != null)
                {
                    naturalRecovery += Skills.Where(s => s.Skill.SkillType == 0 && s.Skill.CastId == 10).Sum(s => s.Skill.UpgradeSkill);
                }
                if (IsSitting)
                {
                    int regen = GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryIncreased)[0];
                    return (int)((regen + CharacterHelper.MPHealth[(byte)Class] + CellonOptions.Where(s => s.Type == CellonOptionType.MPRestore).Sum(s => s.Value)) * (1 + GetShellArmorEffectValue(ShellArmorEffectType.RecoveryMPOnRest) / 100D));
                }
                return (DateTime.Now - LastDefence).TotalSeconds > 4 ? (int)((CharacterHelper.MPHealthStand[(byte)Class] * (1 + GetShellArmorEffectValue(ShellArmorEffectType.RecoveryMP) / 100D)) * naturalRecovery) : 0;
            }
            return 0;
        }

        internal double HeroXPLoad() => HeroLevel == 0 ? 1 : CharacterHelper.HeroXpData[HeroLevel - 1];

        private void IncrementGroupQuest(QuestType type, int firstData = 0, int secondData = 0, int thirdData = 0)
        {
            if (Group != null && Group.GroupType == GroupType.Group)
            {
                foreach (ClientSession groupMember in Group.Sessions.Where(s => s.Character.MapInstance == MapInstance && s.Character.CharacterId != CharacterId))
                {
                    groupMember.Character.IncrementQuests(type, firstData, secondData, thirdData, true);
                }
            }
        }

        internal void IncrementObjective(CharacterQuest quest, byte index = 0, int amount = 1, bool isOver = false)
        {
            bool isFinish = isOver;
            Session.SendPacket(quest.GetProgressMessage(index, amount));
            quest.Incerment(index, amount);
            byte a = 1;
            if (quest.GetObjectives().All(q => quest?.GetObjectiveByIndex(a) == null || q >= quest?.GetObjectiveByIndex(a++).Objective))
            {
                isFinish = true;
            }
            Session.SendPacket($"qsti {quest.GetInfoPacket(false)}");
            if (!isFinish)
            {
                return;
            }
            LastQuest = DateTime.Now;
            if (CustomQuestRewards((QuestType)quest?.Quest?.QuestType, quest.Quest.QuestId))
            {
                if (quest.Quest.QuestType == (int)QuestType.FlowerQuest && quest.Quest.QuestId == 22023)
                {
                    Session.SendPacket(quest.Quest.GetRewardPacket(this));
                }
                RemoveQuest(quest.QuestId);
                return;
            }

            
            Session.SendPacket(quest.Quest.GetRewardPacket(this));
            RemoveQuest(quest.QuestId);
        }

        internal double JobXPLoad() => CharacterHelper.JobXpData[(int)Class, JobLevel - 1];

        internal double SpXpLoad()
        {
            ItemInstance specialist = null;
            if (Inventory != null)
            {
                specialist = Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            }
            return specialist != null ? CharacterHelper.SPXPData[specialist.Item.IsSecondarySp(), specialist.SpLevel - 1] : 0;
        }

        internal double XpLoad() => CharacterHelper.XPData[Level - 1];

        #endregion
    }
}
