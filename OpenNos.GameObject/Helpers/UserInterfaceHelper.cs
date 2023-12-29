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

using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Modules.Bazaar.Queries;

namespace OpenNos.GameObject.Helpers
{
    public class UserInterfaceHelper
    {
        #region Members

        private static UserInterfaceHelper _instance;

        #endregion

        #region Properties

        public static UserInterfaceHelper Instance => _instance ?? (_instance = new UserInterfaceHelper());

        #endregion

        #region Methods

        public static string GenerateBpt()
        {
            TimeSpan timeSpan = ServerManager.Instance.Configuration.EndSeason - DateTime.Now;
            return $"bpt {Math.Round(timeSpan.TotalMinutes)}";
        }

        public static string GenerateEgClock()
        {
            var difference = (DateTime.Now - DateTime.UtcNow).Hours;
            var channelId = ServerManager.Instance.ChannelId;

            return $"eg_clock {difference} {ServerManager.Instance.ServerGroup} {channelId}";
        }

        public static string GenerateBazarRecollect(long pricePerUnit, int soldAmount, int amount, long taxes, long totalPrice, string name) => $"rc_scalc 1 {pricePerUnit} {soldAmount} {amount} {taxes} {totalPrice} {name.Replace(' ', '^')}";

        public static string GenerateBSInfo(byte mode, short title, short time, short text) => $"bsinfo {mode} {title} {time} {text}";

        public static string GenerateCHDM(int maxhp, int angeldmg, int demondmg, int time) => $"ch_dm {maxhp} {angeldmg} {demondmg} {time}";

        public static string GenerateDelay(int delay, int type, string argument) => $"delay {delay} {type} {argument}";

        public static string GenerateDialog(string dialog) => $"dlg {dialog}";

        public static string GenerateFrank(byte type)
        {
            string packet = "frank_stc";
            int rank = 0;
            long savecount = 0;

            List<Family> familyordered = ServerManager.Instance.FamilyList.Where(s => DAOFactory.FamilyCharacterDAO.LoadByFamilyId(s.FamilyId).FirstOrDefault(c => c.Authority == FamilyAuthority.Head) is FamilyCharacterDTO famChar && DAOFactory.CharacterDAO.LoadById(famChar.CharacterId) is CharacterDTO character && DAOFactory.AccountDAO.LoadById(character.AccountId).Authority <= AuthorityType.TGS);

            switch (type)
            {
                case 0:
                    familyordered = familyordered.OrderByDescending(s => s.FamilyExperience).OrderByDescending(s => s.FamilyLevel).ToList();
                    break;

                case 1:
                    familyordered = familyordered.OrderByDescending(s => s.FamilyLogs.Where(l => l.FamilyLogType == FamilyLogType.FamilyXP && l.Timestamp.AddDays(30) > DateTime.Now).ToList().Sum(c => long.Parse(c.FamilyLogData.Split('|')[1]))).ToList();//use month instead log
                    break;

                case 2:

                    // use month instead log
                    familyordered = familyordered.OrderByDescending(s => s.FamilyCharacters.Sum(c => c.Character.Reputation)).ToList();
                    break;

                case 3:
                    familyordered = familyordered.OrderByDescending(s => s.FamilyCharacters.Sum(c => c.Character.Reputation)).ToList();
                    break;
            }
            int i = 0;
            if (familyordered != null)
            {
                foreach (Family fam in familyordered.Take(100))
                {
                    i++;
                    long sum = 0;
                    switch (type)
                    {
                        case 0:
                            sum = fam.FamilyExperience;
                            for (byte x = 1; x < fam.FamilyLevel; x++)
                            {
                                sum += CharacterHelper.LoadFamilyXPData(x);
                            }
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{sum}";
                            break;

                        case 1:
                            sum = fam.FamilyLogs.Where(l => l.FamilyLogType == FamilyLogType.FamilyXP && l.Timestamp.AddDays(30) > DateTime.Now).ToList().Sum(c => long.Parse(c.FamilyLogData.Split('|')[1]));
                            if (savecount != fam.FamilyExperience)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{sum}";
                            break;

                        case 2:
                            sum = fam.FamilyCharacters.Sum(c => c.Character.Reputation);
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;//replace by month log
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{savecount}";
                            break;

                        case 3:
                            sum = fam.FamilyCharacters.Sum(c => c.Character.Reputation);
                            if (savecount != sum)
                            {
                                rank++;
                            }
                            else
                            {
                                rank = i;
                            }
                            savecount = sum;
                            packet += $" {rank}|{fam.Name}|{fam.FamilyLevel}|{savecount}";
                            break;
                    }
                }
            }
            return packet;
        }

        public static string GenerateGuri(byte type, byte argument, long callerId, int value = 0, int value2 = 0)
        {
            switch (type)
            {
                case 2:
                    return $"guri 2 {argument} {callerId}";

                case 6:
                    return $"guri 6 1 {callerId} 0 0";

                case 10:
                    return $"guri 10 {argument} {value} {callerId}";

                case 15:
                    return $"guri 15 {argument} 0 0";

                case 31:
                    return $"guri 31 {argument} {callerId} {value} {value2}";

                default:
                    return $"guri {type} {argument} {callerId} {value}";
            }
        }

        public static string GenerateInbox(string value) => $"inbox {value}";

        public static string GenerateInfo(string message) => $"info {message}";

        public static string GenerateMapOut() => "mapout";

        public static string GenerateModal(string message, int type) => $"modal {type} {message}";

        public static string GenerateMsg(string message, int type) => $"msg {type} {message}";

        public static string GeneratePClear() => "p_clear";

        public static string GenerateRCBList(CBListPacket packet)
        {
            return BazaarHttpClient.Instance.GenerateRcbList(new GetRcbListQuery { Packet = packet });
        }

        public static string GenerateRemovePacket(short slot) => $"{slot}.-1.0.0.0";

        public static string GenerateRl(byte type)
        {
            string str = $"rl {type}";

            ServerManager.Instance.GroupList.ForEach(s =>
            {
                if (s.SessionCount > 0)
                {
                    ClientSession leader = s.Sessions.ElementAt(0);
                    str += $" {s.Raid.Id}.{s.Raid?.LevelMinimum}.{s.Raid?.LevelMaximum}.{leader.Character.Name}.{leader.Character.Level}.{(leader.Character.UseSp ? leader.Character.Morph : -1)}.{(byte)leader.Character.Class}.{(byte)leader.Character.Gender}.{s.SessionCount}.{leader.Character.HeroLevel}";
                }
            });

            return str;
        }

        public static string GenerateRp(int mapid, int x, int y, string param) => $"rp {mapid} {x} {y} {param}";

        public static string GenerateSay(string message, int type, long callerId = 0) => $"say 1 {callerId} {type} {message}";

        public static string GenerateShopMemo(int type, string message) => $"s_memo {type} {message}";

        public static string GenerateTeamArenaClose() => "ta_close";

        public static string GenerateTeamArenaMenu(byte mode, byte zenasScore, byte ereniaScore, int time, byte arenaType) => $"ta_m {mode} {zenasScore} {ereniaScore} {time} {arenaType}";

        public static IEnumerable<string> GenerateVb() => new[] { "vb 340 0 0", "vb 339 0 0", "vb 472 0 0", "vb 471 0 0" };

        public string GenerateFStashRemove(short slot) => $"f_stash {GenerateRemovePacket(slot)}";

        public string GenerateInventoryRemove(InventoryType Type, short Slot) => $"ivn {(byte)Type} {GenerateRemovePacket(Slot)}";

        public string GeneratePStashRemove(short slot) => $"pstash {GenerateRemovePacket(slot)}";

        public string GenerateStashRemove(short slot) => $"stash {GenerateRemovePacket(slot)}";

        public string GenerateTaSt(TalentArenaOptionType watch) => $"ta_st {(byte)watch}";

        #endregion
    }
}