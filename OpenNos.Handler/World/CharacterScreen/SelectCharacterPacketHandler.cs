using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.CharacterScreen
{
    public class SelectCharacterPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SelectCharacterPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// select packet
        /// </summary>
        /// <param name="selectPacket"></param>
        public void SelectCharacter(SelectPacket selectPacket)
        {
            try
            {
                #region Validate Session

                if (Session?.Account == null
                    || Session.HasSelectedCharacter)
                {
                    Session?.Disconnect();
                    return;
                }

                #endregion

                #region Load Character


                CharacterDTO characterDto = DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, selectPacket.Slot);

                if (characterDto == null)
                {
                    Session.Disconnect();
                    return;
                }

                Character character = new Character(characterDto);

                #endregion

                #region Unban Character

                if (ServerManager.Instance.BannedAccounts.Contains(character.AccountId))
                {
                    ServerManager.Instance.BannedAccounts.RemoveAll(s => s == character.AccountId);
                }

                #endregion

                #region Initialize Character

                character.Initialize();

                character.MapInstanceId = ServerManager.GetBaseMapInstanceIdByMapId(character.MapId);
                character.PositionX = character.MapX;
                character.PositionY = character.MapY;

                Session.SetCharacter(character);

                #endregion

                #region Load General Logs

                character.GeneralLogs = new ThreadSafeGenericList<GeneralLogDTO>();
                character.GeneralLogs.AddRange(DAOFactory.GeneralLogDAO.LoadByAccount(Session.Account.AccountId)
                    .Where(s => s.LogType == "DailyReward" || s.CharacterId == character.CharacterId).ToList());

                #endregion

                #region Reset SpPoint

                if (!Session.Character.GeneralLogs.Any(s => s.Timestamp == DateTime.Now && s.LogData == "World" && s.LogType == "Connection"))
                {
                    Session.Character.SpAdditionPoint += (int)(Session.Character.SpPoint / 100D * 20D);
                    Session.Character.SpPoint = 10000;
                }

                #endregion

                #region Other Character Stuffs

                Session.Character.Respawns = DAOFactory.RespawnDAO.LoadByCharacter(Session.Character.CharacterId).ToList();
                Session.Character.StaticBonusList = new ThreadSafeGenericLockedList<StaticBonusDTO>(DAOFactory.StaticBonusDAO.LoadByCharacterId(Session.Character.CharacterId).ToList());
                Session.Character.LoadInventory();
                Session.Character.LoadQuicklists();
                Session.Character.GenerateMiniland();

                #endregion

                #region Questlogs

                DAOFactory.QuestLogDAO.LoadByCharacterId(Session.Character.CharacterId).ToList().ForEach(s => Session.Character.QuestLogs.Add(s));

                #endregion

                #region InstantBattleLogs

                DAOFactory.InstantBattleLogDAO.LoadByCharacterId(Session.Character.CharacterId).ToList().ForEach(s => Session.Character.InstantBattleLogs.Add(s));

                #endregion

                #region Quests

                #region BazaarItems

                DAOFactory.BazaarItemDAO.LoadByCharacterId(Session.Character.CharacterId)?.ToList()?.ForEach(s => Session.Character.BazaarItems.TryAdd(s.BazaarItemId, s));

                #endregion

                try
                {
                    DAOFactory.CharacterQuestDAO.LoadByCharacterId(Session.Character.CharacterId).Distinct().ToList().ForEach(q => Session.Character.Quests.Add(new CharacterQuest(q)));
                }
                catch (Exception e)
                {
                    Logger.Log.Error(null, e);
                }

                #endregion

                #region battlepass

                DAOFactory.BattlePassItemLogsDAO.LoadByCharactedId(Session.Character.CharacterId).Distinct().ToList().ForEach(x => Session.Character.BattlePassItemLogs.Add(x));

                DAOFactory.BattlePassQuestLogsDAO.LoadByCharactedId(Session.Character.CharacterId).Distinct().ToList().ForEach(x => Session.Character.BattlePassQuestLogs.Add(x));

                #endregion

                #region Title

                DAOFactory.CharacterTitleDAO.LoadByCharacterId(Session.Character.CharacterId).ToList().ForEach(s => Session.Character.Title.Add(s));

                #endregion

                #region Fix Partner Slots

                if (character.MaxPartnerCount < 3)
                {
                    character.MaxPartnerCount = 3;
                }

                #endregion

                #region Load Mates

                try
                {
                    DAOFactory.MateDAO.LoadByCharacterId(Session.Character.CharacterId).ToList().ForEach(s =>
                    {
                        Mate mate = new Mate(s)
                        {
                            Owner = Session.Character
                        };

                        mate.GenerateMateTransportId();
                        mate.Monster = ServerManager.GetNpcMonster(s.NpcMonsterVNum);

                        Session.Character.Mates.Add(mate);
                    });
                }
                catch (Exception b)
                {
                    Logger.Log.Error(null, b);
                }

                #endregion

                #region Load Permanent Buff

                Session.Character.LastPermBuffRefresh = DateTime.Now;

                #endregion

                #region CharacterLife

                Session.Character.Life = Observable.Interval(TimeSpan.FromMilliseconds(300)).SafeSubscribe(x => Session?.Character?.CharacterLife());

                #endregion

                #region Load Amulet

                Observable.Timer(TimeSpan.FromSeconds(1))
                    .SafeSubscribe(o =>
                    {
                        if (Session?.Character == null)
                        {
                            return;
                        }

                        ItemInstance amulet = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Amulet, InventoryType.Wear);

                        if (amulet?.ItemDeleteTime != null || amulet?.DurabilityPoint > 0)
                        {
                            Session.Character.AddBuff(new Buff(62, Session.Character.Level), Session.Character.BattleEntity);
                        }
                    });

                #endregion

                #region Load Static Buff

                foreach (StaticBuffDTO staticBuff in DAOFactory.StaticBuffDAO.LoadByCharacterId(Session.Character.CharacterId))
                {
                    if (staticBuff.CardId != 319 /* Wedding */)
                    {
                        Session.Character.AddStaticBuff(staticBuff);
                    }
                }

                #endregion

                #region Enter the World

                Session.Character.GeneralLogs.Add(new GeneralLogDTO
                {
                    AccountId = Session.Account.AccountId,
                    CharacterId = Session.Character.CharacterId,
                    IpAddress = Session.CleanIpAddress,
                    LogData = "World",
                    LogType = "Connection",
                    Timestamp = DateTime.Now
                });

                Session.SendPacket("OK");
                CommunicationServiceClient.Instance.ConnectCharacter(ServerManager.Instance.WorldId, character.CharacterId);
                character.Channel = ServerManager.Instance;

                #endregion
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Failed selecting the character.", ex);
                Session?.Disconnect();
            }
            finally
            {
                // Suspicious activity detected -- kick!
                if (Session != null && (!Session.HasSelectedCharacter || Session.Character == null))
                {
                    Logger.Log.Error("Disconnecting session in select character. For some reason.", null);
                    Session.Disconnect();
                }
            }
        }
    }
}
