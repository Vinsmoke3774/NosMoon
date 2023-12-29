using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.RainbowBattle;

namespace OpenNos.Handler.World.Basic
{
    public class RevivePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RevivePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// revival packet
        /// </summary>
        /// <param name="revivalPacket"></param>
        public void Revive(RevivalPacket revivalPacket)
        {
            if (Session.Character.Hp > 0)
            {
                return;
            }

            switch (revivalPacket.Type)
            {
                case 0:
                    switch (Session.CurrentMapInstance.MapInstanceType)
                    {
                        case MapInstanceType.RainbowBattleInstance:
                            var rbb = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));
                            IDisposable obs = null;
                            obs = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                            {
                                if (Session == null)
                                {
                                    obs?.Dispose();
                                    return;
                                }

                                Session.CurrentMapInstance?.Broadcast(Session.Character?.GenerateEff(35));
                            });

                            rbb.SecondTeam.Score += 2;
                            RainbowBattleManager.SendFbs(Session.CurrentMapInstance);

                            Session.CurrentMapInstance?.Broadcast((UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RAINBOW_KILL"),
                            Session.Character.Name, Session.Character.Name), 0)));
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RESP_RBB"), Session.Character.Name), 10));

                            Session.Character.Hp = (int)Session.Character.HPLoad();
                            Session.Character.Mp = (int)Session.Character.MPLoad();
                            Session.SendPacket(Session?.Character?.GenerateStat());
                            Session.Character.NoMove = true;
                            Session.Character.NoAttack = true;
                            Session.Character.IsFrozen = true;
                            Session.SendPacket(Session?.Character?.GenerateCond());
                            Observable.Timer(TimeSpan.FromSeconds(20)).SafeSubscribe(o =>
                            {
                                if (Session?.Character == null)
                                {
                                    obs?.Dispose();
                                    return;
                                }

                                if (Session.Character.IsFrozen)
                                {
                                    Session.Character.PositionX = rbb.TeamEntity == RainbowTeamBattleType.Red ? ServerManager.RandomNumber<short>(30, 32) : ServerManager.RandomNumber<short>(83, 85);
                                    Session.Character.PositionY = rbb.TeamEntity == RainbowTeamBattleType.Red ? ServerManager.RandomNumber<short>(73, 76) : ServerManager.RandomNumber<short>(2, 4);
                                    Session?.CurrentMapInstance?.Broadcast(Session.Character.GenerateTp());
                                    Session.Character.NoAttack = false;
                                    Session.Character.NoMove = false;
                                    Session.Character.IsFrozen = false;
                                    Session?.SendPacket(Session.Character.GenerateCond());
                                }
                                obs?.Dispose();
                            });
                            break;

                        case MapInstanceType.LodInstance:
                            const int saver = 1211;
                            if (Session.Character.Inventory.CountItem(saver) < 1)
                            {
                                Session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_SAVER"), 0));
                                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            }
                            else
                            {
                                Session.Character.Inventory.RemoveItemAmount(saver);
                                Session.Character.Hp = (int)Session.Character.HPLoad();
                                Session.Character.Mp = (int)Session.Character.MPLoad();
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                                Session.SendPacket(Session.Character.GenerateStat());
                            }

                            break;

                        case MapInstanceType.Act4Berios:
                        case MapInstanceType.Act4Calvina:
                        case MapInstanceType.Act4Hatus:
                        case MapInstanceType.Act4Morcos:
                            if (Session.Character.Reputation < Session.Character.Level * 10)
                            {
                                Session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_REPUT"), 0));
                                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                            }
                            else
                            {
                                Session.Character.GetReputation(Session.Character.Level * -10);
                                Session.Character.Hp = (int)Session.Character.HPLoad();
                                Session.Character.Mp = (int)Session.Character.MPLoad();
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                                Session.SendPacket(Session.Character.GenerateStat());
                            }

                            break;

                        case MapInstanceType.CaligorInstance:
                            Session.Character.Hp = (int)Session.Character.HPLoad();
                            Session.Character.Mp = (int)Session.Character.MPLoad();
                            short x = 0;
                            short y = 0;
                            switch (Session.Character.Faction)
                            {
                                case FactionType.Angel:
                                    x = 50;
                                    y = 172;
                                    break;

                                case FactionType.Demon:
                                    x = 130;
                                    y = 172;
                                    break;
                            }
                            ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.Character.MapInstance.MapInstanceId, x, y);
                            Session.Character.AddBuff(new Buff(169, Session.Character.Level), Session.Character.BattleEntity);
                            break;

                        default:
                            const int seed = 1012;
                            if (Session.Character.Inventory.CountItem(seed) < 10 && Session.Character.Level > 20)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("NOT_ENOUGH_POWER_SEED"), 0));
                                ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                                Session.SendPacket(
                                    Session.Character.GenerateSay(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_SEED_SAY"), 0));
                            }
                            else
                            {
                                if (Session.Character.Level > 20)
                                {
                                    Session.SendPacket(Session.Character.GenerateSay(
                                        string.Format(Language.Instance.GetMessageFromKey("SEED_USED"), 10), 10));
                                    Session.Character.Inventory.RemoveItemAmount(seed, 10);
                                    Session.Character.Hp = (int)(Session.Character.HPLoad() / 2);
                                    Session.Character.Mp = (int)(Session.Character.MPLoad() / 2);
                                    Session.Character.AddBuff(new Buff(44, Session.Character.Level), Session.Character.BattleEntity);
                                }
                                else
                                {
                                    Session.Character.Hp = (int)Session.Character.HPLoad();
                                    Session.Character.Mp = (int)Session.Character.MPLoad();
                                }

                                if (Session.Character.HeroLevel > 0 && (Session.Character.MapId >= 228) && (Session.Character.MapId <= 246) || (Session.Character.MapId >= 2628) && (Session.Character.MapId <= 2650))
                                {
                                    Session.Character.HeroXp -= Session.Character.HeroXp * (long)0.04;
                                    Session.SendPacket(Session.Character.GenerateLev());
                                }

                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateTp());
                                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                                Session.SendPacket(Session.Character.GenerateStat());
                            }
                            break;
                    }

                    break;

                case 1:
                    ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                    {
                        if (Session.Character.Level > 20)
                        {
                            Session.Character.AddBuff(new Buff(44, Session.Character.Level), Session.Character.BattleEntity);
                        }
                    }
                    break;

                case 2:
                    if ((Session.CurrentMapInstance == ServerManager.Instance.ArenaInstance || Session.CurrentMapInstance == ServerManager.Instance.FamilyArenaInstance) &&
                        Session.Character.Gold >= 1000)
                    {
                        Session.Character.Hp = (int)Session.Character.HPLoad();
                        Session.Character.Mp = (int)Session.Character.MPLoad();
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateTp());
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateRevive());
                        Session.SendPacket(Session.Character.GenerateStat());
                        Session.Character.Gold -= 1000;
                        Session.SendPacket(Session.Character.GenerateGold());
                        Session.Character.LastPVPRevive = DateTime.Now;
                        Observable.Timer(TimeSpan.FromSeconds(5)).SafeSubscribe(observer =>
                        {
                            if (Session == null)
                            {
                                return;
                            }

                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PVP_ACTIVE"), 10));
                        });
                    }
                    else
                    {
                        ServerManager.Instance.ReviveFirstPosition(Session.Character.CharacterId);
                    }

                    break;
            }
            Session.Character.BattleEntity.SendBuffsPacket();
        }
    }
}
