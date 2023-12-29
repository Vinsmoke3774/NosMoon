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

using OpenNos.DAL.DAO;
using OpenNos.DAL.Interface;

namespace OpenNos.DAL
{
    public static class DAOFactory
    {
        #region Members

        private static IQuestLogDAO _questLogDAO;
        private static IAccountDAO _accountDAO;
        private static IBazaarItemDAO _bazaarItemDAO;
        private static IBCardDAO _bcardDAO;
        private static ICardDAO _cardDAO;
        private static ICellonOptionDAO _cellonOptionDAO;
        private static ICharacterDAO _characterDAO;
        private static ICharacterQuestDAO _characterQuestDAO;
        private static ICharacterRelationDAO _characterRelationDAO;
        private static ICharacterSkillDAO _characterSkillDAO;
        private static ICharacterTitleDAO _characterTitleDAO;
        private static ICharacterTimespaceLogDAO _characterTimespaceLogDAO;
        private static IChatLogDao _chatLogsDAO;
        private static IComboDAO _comboDAO;
        private static IDropDAO _dropDAO;
        private static IFamilyCharacterDAO _familyCharacterDAO;
        private static IFamilyDAO _familyDAO;
        private static IFamilyLogDAO _familyLogDAO;
        private static IGeneralLogDAO _generalLogDAO;
        private static IItemDAO _itemDAO;
        private static IItemInstanceDAO _itemInstanceDAO;
        private static IMailDAO _mailDAO;
        private static IMaintenanceLogDAO _maintenanceLogDAO;
        private static IMapDAO _mapDAO;
        private static IMapMonsterDAO _mapMonsterDAO;
        private static IMapNpcDAO _mapNpcDAO;
        private static IMapTypeDAO _mapTypeDAO;
        private static IMapTypeMapDAO _mapTypeMapDAO;
        private static IMateDAO _mateDAO;
        private static IMimicRotationDAO _mimicRotationDao;
        private static IMinigameLogDAO _minigameLogDAO;
        private static IMinilandObjectDAO _minilandObjectDAO;
        private static INpcMonsterDAO _npcMonsterDAO;
        private static INpcMonsterSkillDAO _npcMonsterSkillDAO;
        private static IPartnerSkillDAO _partnerSkillDAO;
        private static IPenaltyLogDAO _penaltyLogDAO;
        private static IPortalDAO _portalDAO;
        private static IQuestDAO _questDAO;
        private static IQuestObjectiveDAO _questObjectiveDAO;
        private static IQuestRewardDAO _questRewardDAO;
        private static IQuicklistEntryDAO _quicklistEntryDAO;
        private static IRecipeDAO _recipeDAO;
        private static IRecipeItemDAO _recipeItemDAO;
        private static IRecipeListDAO _recipeListDAO;
        private static IRespawnDAO _respawnDAO;
        private static IRespawnMapTypeDAO _respawnMapTypeDAO;
        private static IRollGeneratedItemDAO _rollGeneratedItemDAO;
        private static IScriptedInstanceDAO _scriptedInstanceDAO;
        private static IShellEffectDAO _shellEffectDAO;
        private static IShopDAO _shopDAO;
        private static IShopItemDAO _shopItemDAO;
        private static IShopSkillDAO _shopSkillDAO;
        private static ISkillDAO _skillDAO;
        private static IStaticBonusDAO _staticBonusDAO;
        private static IStaticBuffDAO _staticBuffDAO;
        private static ITeleporterDAO _teleporterDAO;
        private static ITitleLogDAO _titleLogDAO;
        private static IInstantBattleLogDAO _instantBattleLogDao;
        private static ITwoFactorBackupsDAO _twoFactorBackupsDao;
        private static ITitleWearConditionDao _titleWearConditionDAO;
        private static ILevelUpRewardDao _levelUpRewardDao;
        private static IWhitelistedCharacterDAO _whitelistedCharactersDao;
        private static IBattlePassItemDAO _battlePassItemDao;
        private static IBattlePassPalierDAO _battlePassPalierDao;
        private static IBattlePassQuestDAO _battlePassQuestDao;
        private static IBattlePassItemLogsDAO _battlePassItemLogsDao;
        private static IBattlePassQuestLogsDAO _battlePassQuestLogsDao;
        private static IFishingLogDAO _fishingLogDao;
        private static IFishingSpotsDAO _fishingSpotsDao;
        private static IFishInfoDAO _fishInfoDao;

        #endregion

        #region Properties

        public static IQuestLogDAO QuestLogDAO => _questLogDAO ??= new QuestLogDAO();

        public static IInstantBattleLogDAO InstantBattleLogDAO => _instantBattleLogDao ??= new InstantBattleLogDAO();

        public static IAccountDAO AccountDAO => _accountDAO ??= new AccountDAO();

        public static IBazaarItemDAO BazaarItemDAO => _bazaarItemDAO ??= new BazaarItemDAO();

        public static IBCardDAO BCardDAO => _bcardDAO ??= new BCardDAO();

        public static ICardDAO CardDAO => _cardDAO ??= new CardDAO();

        public static ICellonOptionDAO CellonOptionDAO => _cellonOptionDAO ??= new CellonOptionDAO();

        public static ICharacterDAO CharacterDAO => _characterDAO ??= new CharacterDAO();

        public static ICharacterQuestDAO CharacterQuestDAO => _characterQuestDAO ??= new CharacterQuestDAO();

        public static ICharacterRelationDAO CharacterRelationDAO => _characterRelationDAO ??= new CharacterRelationDAO();

        public static ICharacterSkillDAO CharacterSkillDAO => _characterSkillDAO ??= new CharacterSkillDAO();

        public static ICharacterTitleDAO CharacterTitleDAO => _characterTitleDAO ??= new CharacterTitleDAO();

        public static ICharacterTimespaceLogDAO CharacterTimespaceLogDAO => _characterTimespaceLogDAO ??= new CharacterTimespaceLogDAO();

        public static IChatLogDao ChatLogsDAO => _chatLogsDAO ??= new ChatLogDao();

        public static IComboDAO ComboDAO => _comboDAO ??= new ComboDAO();

        public static IDropDAO DropDAO => _dropDAO ??= new DropDAO();

        public static IFamilyCharacterDAO FamilyCharacterDAO => _familyCharacterDAO ??= new FamilyCharacterDAO();

        public static IFamilyDAO FamilyDAO => _familyDAO ??= new FamilyDAO();

        public static IFamilyLogDAO FamilyLogDAO => _familyLogDAO ??= new FamilyLogDAO();

        public static IGeneralLogDAO GeneralLogDAO => _generalLogDAO ??= new GeneralLogDAO();

        public static IItemDAO ItemDAO => _itemDAO ??= new ItemDAO();

        public static IItemInstanceDAO ItemInstanceDAO => _itemInstanceDAO ??= new ItemInstanceDAO();

        public static IMailDAO MailDAO => _mailDAO ??= new MailDAO();

        public static IMaintenanceLogDAO MaintenanceLogDAO => _maintenanceLogDAO ??= new MaintenanceLogDAO();

        public static IMapDAO MapDAO => _mapDAO ??= new MapDAO();

        public static IMapMonsterDAO MapMonsterDAO => _mapMonsterDAO ??= new MapMonsterDAO();

        public static IMapNpcDAO MapNpcDAO => _mapNpcDAO ??= new MapNpcDAO();

        public static IMapTypeDAO MapTypeDAO => _mapTypeDAO ??= new MapTypeDAO();

        public static IMapTypeMapDAO MapTypeMapDAO => _mapTypeMapDAO ??= new MapTypeMapDAO();

        public static IMateDAO MateDAO => _mateDAO ??= new MateDAO();

        public static IMimicRotationDAO MimicRotationDAO => _mimicRotationDao ??= new MimicRotationDAO();

        public static IMinigameLogDAO MinigameLogDAO => _minigameLogDAO ??= new MinigameLogDAO();

        public static IMinilandObjectDAO MinilandObjectDAO => _minilandObjectDAO ??= new MinilandObjectDAO();

        public static INpcMonsterDAO NpcMonsterDAO => _npcMonsterDAO ??= new NpcMonsterDAO();

        public static INpcMonsterSkillDAO NpcMonsterSkillDAO => _npcMonsterSkillDAO ??= new NpcMonsterSkillDAO();

        public static IPartnerSkillDAO PartnerSkillDAO => _partnerSkillDAO ??= new PartnerSkillDAO();

        public static IPenaltyLogDAO PenaltyLogDAO => _penaltyLogDAO ??= new PenaltyLogDAO();

        public static IPortalDAO PortalDAO => _portalDAO ??= new PortalDAO();

        public static IQuestDAO QuestDAO => _questDAO ??= new QuestDAO();

        public static IQuestObjectiveDAO QuestObjectiveDAO => _questObjectiveDAO ??= new QuestObjectiveDAO();

        public static IQuestRewardDAO QuestRewardDAO => _questRewardDAO ??= new QuestRewardDAO();

        public static IQuicklistEntryDAO QuicklistEntryDAO => _quicklistEntryDAO ??= new QuicklistEntryDAO();

        public static IRecipeDAO RecipeDAO => _recipeDAO ??= new RecipeDAO();

        public static IRecipeItemDAO RecipeItemDAO => _recipeItemDAO ??= new RecipeItemDAO();

        public static IRecipeListDAO RecipeListDAO => _recipeListDAO ??= new RecipeListDAO();

        public static IRespawnDAO RespawnDAO => _respawnDAO ??= new RespawnDAO();

        public static IRespawnMapTypeDAO RespawnMapTypeDAO => _respawnMapTypeDAO ??= new RespawnMapTypeDAO();

        public static IRollGeneratedItemDAO RollGeneratedItemDAO => _rollGeneratedItemDAO ??= new RollGeneratedItemDAO();

        public static IScriptedInstanceDAO ScriptedInstanceDAO => _scriptedInstanceDAO ??= new ScriptedInstanceDAO();

        public static IShellEffectDAO ShellEffectDAO => _shellEffectDAO ??= new ShellEffectDAO();

        public static IShopDAO ShopDAO => _shopDAO ??= new ShopDAO();

        public static IShopItemDAO ShopItemDAO => _shopItemDAO ??= new ShopItemDAO();

        public static IShopSkillDAO ShopSkillDAO => _shopSkillDAO ??= new ShopSkillDAO();

        public static ISkillDAO SkillDAO => _skillDAO ??= new SkillDAO();

        public static IStaticBonusDAO StaticBonusDAO => _staticBonusDAO ??= new StaticBonusDAO();

        public static IStaticBuffDAO StaticBuffDAO => _staticBuffDAO ??= new StaticBuffDAO();

        public static ITeleporterDAO TeleporterDAO => _teleporterDAO ??= new TeleporterDAO();

        public static ITitleLogDAO TitleLogDAO => _titleLogDAO ??= new TitleLogDAO();

        public static ITwoFactorBackupsDAO TwoFactorBackupsDAO => _twoFactorBackupsDao ??= new TwoFactorBackupsDAO();

        public static ITitleWearConditionDao TitleWearConditionDao => _titleWearConditionDAO ??= new TitleWearConditionDao();

        public static ILevelUpRewardDao LevelUpRewardsDao => _levelUpRewardDao ??= new LevelUpRewardsDao();

        public static IWhitelistedCharacterDAO WhitelistedCharacterDao => _whitelistedCharactersDao ??= new WhitelistedCharacterDao();

        public static IBattlePassItemDAO BattlePassItemDAO => _battlePassItemDao ??= new BattlePassItemDAO();

        public static IBattlePassPalierDAO BattlePassPalierDAO => _battlePassPalierDao ??= new BattlePassPalierDAO();

        public static IBattlePassQuestDAO BattlePassQuestDAO => _battlePassQuestDao ??= new BattlePassQuestDAO();

        public static IBattlePassItemLogsDAO BattlePassItemLogsDAO => _battlePassItemLogsDao ??= new BattlePassItemLogsDAO();

        public static IBattlePassQuestLogsDAO BattlePassQuestLogsDAO => _battlePassQuestLogsDao ??= new BattlePassQuestLogsDAO();

        public static IFishInfoDAO FishInfoDAO => _fishInfoDao ??= new FishInfoDAO();

        public static IFishingLogDAO FishingLogDAO => _fishingLogDao ??= new FishingLogDAO();

        public static IFishingSpotsDAO FishingSpotsDAO => _fishingSpotsDao ??= new FishingSpotsDAO();

        #endregion
    }
}