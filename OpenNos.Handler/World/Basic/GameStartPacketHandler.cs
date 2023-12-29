using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.Basic
{
    public class GameStartPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public GameStartPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// game_start packet
        /// </summary>
        /// <param name="gameStartPacket"></param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public void StartGame(GameStartPacket gameStartPacket)
        {
            try
            {
                if (Session?.Character == null || Session.HasCurrentMapInstance || !Session.HasSelectedCharacter)
                {
                    // character should have been selected in SelectCharacter
                    return;
                }

                bool shouldRespawn = false;

                if (Session.Character.MapInstance?.Map?.MapTypes != null)
                {
                    if (Session.Character.MapInstance.Map.MapTypes.Any(m => m.MapTypeId == (short)MapTypeEnum.Act4)
                        && ServerManager.Instance.ChannelId != 51)
                    {
                        shouldRespawn = true;
                    }
                }

                Session.CurrentMapInstance = Session.Character.MapInstance;

                Session.Character.SaveObs = Observable.Interval(TimeSpan.FromMinutes(30)).SafeSubscribe(s =>
                {
                    if (Session?.IsConnected == true)
                    {
                        Session.Character.Save();
                    }
                    else
                    {
                        Session?.Character?.SaveObs?.Dispose();
                    }
                });

                if (ServerManager.Instance.Configuration.SceneOnCreate
                    && Session.Character.GeneralLogs.CountLinq(s => s.LogType == "Connection") < 2)
                {
                    Session.SendPacket("scene 40");
                }

                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WELCOME_SERVER"), ServerManager.Instance.ServerGroup), 10));

                Session.Character.LoadSpeed();
                Session.Character.LoadSkills();
                Session.SendPacket(Session.Character.GenerateTit());
                Session.SendPacket(Session.Character.GenerateSpPoint());
                Session.SendPacket("rsfi 1 1 0 9 0 9");

                Session.Character.Quests?.Where(q => q?.Quest?.TargetMap != null).ToList()
                    .ForEach(qst => Session.SendPacket(qst.Quest.TargetPacket()));

                if (Session.Character.Authority >= AuthorityType.GM)
                {
                    Session.Character.Invisible = false;
                    Session.Character.InvisibleGm = false;
                    Session.Character.HasGodMode = true;
                    Session.Character.Speed = 59;
                    Session.Character.IsCustomSpeed = true;
                    Session.Character.Undercover = !Session.Character.Undercover;
                    Session.SendPacket(Session.Character.GenerateCond());
                }

                if (Session.Account.GoldBank > 0)
                {
                    var bankGoldMod = Session.Account.GoldBank % 1000;

                    if (bankGoldMod > 0)
                    {
                        Session.Account.GoldBank -= bankGoldMod;
                    }
                }

                if (ServerManager.Instance.ChannelId == 51 && Session.Character.Faction != FactionType.None)
                {
                    /*
                    if (ServerManager.Instance.Sessions.Count(s => s.CleanIpAddress.Equals(Session.CleanIpAddress)) > 2)
                    {
                        Session.Disconnect();
                        return;
                    }
                    */

                    Session.Character.MapId = (short)(Session.Character.Faction == FactionType.Angel ? 130 : 131);
                    Session.Character.MapX = 12;
                    Session.Character.MapY = 40;
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                }
                else
                {

                    if (Session.Character.Hp <= 0 && (!Session.Character.IsSeal || ServerManager.Instance.ChannelId != 51))
                    {
                        ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                    }
                    else
                    {
                        if (shouldRespawn)
                        {
                            RespawnMapTypeDTO resp = Session.Character.Respawn;
                            short x = (short)(resp.DefaultX + ServerManager.RandomNumber(-3, 3));
                            short y = (short)(resp.DefaultY + ServerManager.RandomNumber(-3, 3));
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, resp.DefaultMapId, x, y);
                        }
                        else
                        {
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                        }
                    }
                }

                Session.SendPacket(Session.Character.GenerateSki());
                Session.SendPacket($"fd {Session.Character.Reputation} 0 {(int)Session.Character.Dignity} {Math.Abs(Session.Character.GetDignityIco())}");
                Session.SendPacket(Session.Character.GenerateFd());
                Session.SendPacket("rage 0 250000");
                Session.SendPacket("rank_cool 0 0 18000");
                Session.SendPacket("food 0");
                Session.SendPacket(Session.Character.GenerateBpm());
                Session.SendPacket(UserInterfaceHelper.GenerateBpt());
                ItemInstance specialistInstance = Session.Character.Inventory.LoadBySlotAndType(8, InventoryType.Wear);

                StaticBonusDTO medal = Session.Character.StaticBonusList.Find(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

                if (medal != null)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOGIN_MEDAL"), 12));
                }

                if (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBasket))
                {
                    Session.SendPacket("ib 1278 1");
                }

                if (Session.Character.MapInstance?.Map?.MapTypes?.Any(m => m.MapTypeId == (short)MapTypeEnum.CleftOfDarkness) == true)
                {
                    Session.SendPacket("bc 0 0 0");
                }

                if (specialistInstance != null)
                {
                    Session.SendPacket(Session.Character.GenerateSpPoint());
                }

                Session.SendPacket("scr 0 0 0 0 0 0");

                for (int i = 0; i < 10; i++)
                {
                    Session.SendPacket($"bn {i} {Language.Instance.GetMessageFromKey($"BN{i}")}");
                }

                Session.SendPacket(Session.Character.GenerateExts());
                Session.SendPacket(Session.Character.GenerateMlinfo());
                Session.SendPacket(UserInterfaceHelper.GeneratePClear());

                Session.SendPacket(Session.Character.GeneratePinit());
                Session.SendPackets(Session.Character.GeneratePst());

                Session.SendPacket("zzim");
                Session.SendPacket($"umi {DAOFactory.StaticBonusDAO.LoadByCharacterId(Session.Character.CharacterId).Aggregate("", (current, item) => current + $"{(int)item.StaticBonusType}.{(TimeSpan.TryParse((item.DateEnd - DateTime.Now).ToString(), out TimeSpan value) ? Math.Round(value.TotalHours) : -1)} ")}");
                Session.SendPacket($"twk 1 {Session.Character.CharacterId} {Session.Account.Name} {Session.Character.Name} shtmxpdlfeoqkr uk uk");

                long? familyId = DAOFactory.FamilyCharacterDAO.LoadByCharacterId(Session.Character.CharacterId)?.FamilyId;
                if (familyId.HasValue)
                {
                    Session.Character.Family = ServerManager.Instance.FamilyList[familyId.Value];
                }

                if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
                {
                    if (Session.Character.Faction != (FactionType)Session.Character.Family.FamilyFaction)
                    {
                        Session.Character.Faction = (FactionType)Session.Character.Family.FamilyFaction;
                    }

                    Session.SendPacket(Session.Character.GenerateGInfo());
                    Session.SendPackets(Session.Character.GetFamilyHistory());
                    Session.SendPacket(Session.Character.GenerateFamilyMember());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberExp());

                    if (!string.IsNullOrWhiteSpace(Session.Character.Family.FamilyMessage))
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateInfo("--- Family Message ---\n" + Session.Character.Family.FamilyMessage));
                    }
                }

                //Session.SendPacket(Session.Character.GetSqst());
                Session.SendPacket("act6");
                Session.SendPacket(Session.Character.GenerateFaction());
                Session.SendPackets(Session.Character.GenerateScP());
                Session.SendPackets(Session.Character.GenerateScN());
#pragma warning disable 618
                Session.Character.GenerateStartupInventory();
#pragma warning restore 618

                Session.SendPacket(Session.Character.GenerateGold());
                Session.SendPackets(Session.Character.GenerateQuicklist());

                string clinit = "clinit";
                string flinit = "flinit";
                string kdlinit = "kdlinit";
                foreach (CharacterDTO character in ServerManager.Instance.TopComplimented)
                {
                    clinit +=
                        $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Compliment}|{character.Name}";
                }

                foreach (CharacterDTO character in ServerManager.Instance.TopReputation)
                {
                    flinit +=
                        $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Reputation}|{character.Name}";
                }

                foreach (CharacterDTO character in ServerManager.Instance.TopPoints)
                {
                    kdlinit +=
                        $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Act4Points}|{character.Name}";
                }

                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());

                Session.SendPacket(Session.Character.GenerateFinit());
                Session.SendPacket(Session.Character.GenerateBlinit());
                Session.SendPacket(clinit);
                Session.SendPacket(flinit);
                Session.SendPacket(kdlinit);

                Session.Character.LastPVPRevive = DateTime.Now;

                // finfo - friends info
                Session.Character.LoadMail();
                Session.Character.LoadSentMail();
                Session.Character.DeleteTimeout();

                // Title
                Session.SendPacket(Session.Character.GenerateTitle());
                Session.SendPacket(Session.Character.GenerateTitInfo());

                //Session.Character.GetTitleFromLevel();
                Session.Character.GetEffectFromTitle();
                Session.Character.GetVisualFromTitle();

                // Clock
                Session.SendPacket(UserInterfaceHelper.GenerateEgClock());

                Session.Character.GenerateEnergyField();;

                if (Session.Character.Quests.Count > 0)
                {
                    Session.SendPacket(Session.Character.GenerateQuestsPacket());
                }

                if (Session.Character.IsSeal)
                {
                    if (ServerManager.Instance.ChannelId == 51)
                    {
                        Session.Character.SetSeal();
                    }
                    else
                    {
                        Session.Character.IsSeal = false;
                    }
                }

                if (Session.Character.Level < 93)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateModal($"Welcome new player !\nCheck the #starter-guide channel on our discord to know all the tips and tricks !", 1));
                    Session.SendPacket(Session.Character.GenerateSay($"Join our discord: discord.gg/nosmoon", 11));
                }

                // Lock
                if (!string.IsNullOrEmpty(Session.Account.LockCode) || Session.Account.TwoFactorEnabled)
                {
                    Session.Character.IsLocked = true;
                    Session.Character.NoMove = true;
                    Session.Character.NoAttack = true;
                    Session?.SendPacket(Session.Character.GenerateCond());
                }
            }
            finally
            {
                // In case char is not selected, disconnect the fucker

                try
                {
                    if (Session != null)
                    {
                        if (Session.Character == null || Session.Account == null || !Session.HasSelectedCharacter ||
                            !Session.HasCurrentMapInstance)
                        {
                            Logger.Log.Error("Disconnecting session in GameStart method. For some reason.", null);
                            Session?.Disconnect();
                        }

                    }
                }
                catch (Exception e)
                {
                    Session?.Disconnect();
                    Logger.Log.Error(null, e);
                }
            }
        }
    }
}
