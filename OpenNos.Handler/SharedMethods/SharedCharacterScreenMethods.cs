using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Interfaces.Packets.ClientPackets;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.World.CharacterScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenNos.Handler.SharedMethods
{
    public static class SharedCharacterScreenMethods
    {
        public static void CreateCharacterAction(this ClientSession session, ICharacterCreatePacket characterCreatePacket, ClassType classType)
        {
            if (session.HasCurrentMapInstance)
            {
                return;
            }

            if (session.Account.ShowCharacters.HasValue && !session.Account.ShowCharacters.Value)
            {
                session.SendPacket(UserInterfaceHelper.GenerateInfo("You are not allowed to create characters on this account. Please contact the support."));
                return;
            }

            Logger.Log.LogUserEvent("CREATECHARACTER", session.GenerateIdentity(), $"[CreateCharacter]Name: {characterCreatePacket.Name} Slot: {characterCreatePacket.Slot} Gender: {characterCreatePacket.Gender} HairStyle: {characterCreatePacket.HairStyle} HairColor: {characterCreatePacket.HairColor}");

            if (characterCreatePacket.Slot <= 3
                && DAOFactory.CharacterDAO.LoadBySlot(session.Account.AccountId, characterCreatePacket.Slot) == null
                && characterCreatePacket.Name != null
                && (characterCreatePacket.Gender == GenderType.Male || characterCreatePacket.Gender == GenderType.Female)
                && (characterCreatePacket.HairStyle == HairStyleType.HairStyleA || (classType != ClassType.MartialArtist && characterCreatePacket.HairStyle == HairStyleType.HairStyleB))
                && Enumerable.Range(0, 10).Contains((byte)characterCreatePacket.HairColor)
                && (characterCreatePacket.Name.Length >= 4 && characterCreatePacket.Name.Length <= 14))
            {
                if (classType == ClassType.MartialArtist)
                {
                    IEnumerable<CharacterDTO> characterDTOs = DAOFactory.CharacterDAO.LoadByAccount(session.Account.AccountId);

                    if (!characterDTOs.Any(s => s.Level >= 80))
                    {
                        return;
                    }

                    if (characterDTOs.Any(s => s.Class == ClassType.MartialArtist))
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("MARTIAL_ARTIST_ALREADY_EXISTING")));
                        return;
                    }
                }

                Regex regex = new Regex(@"^[A-Za-z0-9_áéíóúÁÉÍÓÚäëïöüÄËÏÖÜ]+$");

                if (regex.Matches(characterCreatePacket.Name).Count != 1)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INVALID_CHARNAME")));
                    return;
                }

                if (DAOFactory.CharacterDAO.LoadByName(characterCreatePacket.Name) != null)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CHARNAME_ALREADY_TAKEN")));
                    return;
                }

                CharacterDTO characterDTO = new CharacterDTO
                {
                    AccountId = session.Account.AccountId,
                    Slot = characterCreatePacket.Slot,
                    Class = classType,
                    Gender = characterCreatePacket.Gender,
                    HairStyle = characterCreatePacket.HairStyle,
                    HairColor = characterCreatePacket.HairColor,
                    Name = characterCreatePacket.Name,
                    Reputation = 400000,
                    MapId = 1,
                    MapX = 79,
                    MapY = 116,
                    MaxMateCount = 10,
                    MaxPartnerCount = 3,
                    SpPoint = 10000,
                    SpAdditionPoint = 0,
                    MinilandMessage = "Welcome",
                    State = CharacterState.Active,
                    MinilandPoint = 2000,
                    LastSave = DateTime.Now,
                    SkyTowerLevel = 1,
                    AliveCountdown = 0,
                    Faction = (FactionType)ServerManager.RandomNumber(1, 2)
                };

                switch (characterDTO.Class)
                {
                    case ClassType.MartialArtist:
                        {
                            characterDTO.Level = 99;
                            characterDTO.HeroLevel = 30;
                            characterDTO.JobLevel = 99;
                            characterDTO.Hp = 9401;
                            characterDTO.Mp = 3156;
                        }
                        break;

                    default:
                        {
                            characterDTO.Level = 99;
                            characterDTO.HeroLevel = 30;
                            characterDTO.JobLevel = 20;
                            characterDTO.Hp = 6640;
                            characterDTO.Mp = 3270;
                        }
                        break;
                }

                DAOFactory.CharacterDAO.InsertOrUpdate(ref characterDTO);

                if (classType != ClassType.MartialArtist)
                {

                    DAOFactory.CharacterSkillDAO.InsertOrUpdate(new CharacterSkillDTO { CharacterId = characterDTO.CharacterId, SkillVNum = 200 });
                    DAOFactory.CharacterSkillDAO.InsertOrUpdate(new CharacterSkillDTO { CharacterId = characterDTO.CharacterId, SkillVNum = 201 });
                    DAOFactory.CharacterSkillDAO.InsertOrUpdate(new CharacterSkillDTO { CharacterId = characterDTO.CharacterId, SkillVNum = 209 });
                    Inventory inventory = new Inventory(new Character(characterDTO));

                    inventory.AddNewToInventory(884, 1, InventoryType.Equipment);
                    inventory.AddNewToInventory(885, 1, InventoryType.Equipment);
                    inventory.AddNewToInventory(886, 1, InventoryType.Equipment);
                    inventory.AddNewToInventory(887, 1, InventoryType.Equipment);
                    inventory.Values.ToList().ForEach(i => DAOFactory.ItemInstanceDAO.InsertOrUpdate(i));

                    new EntryPointPacketHandler(session).LoadCharacters(new EntryPointPacket { IgnoreSecurity = true, PacketData = characterCreatePacket.OriginalContent });
                }
                else
                {
                    for (short skillVNum = 1525; skillVNum <= 1539; skillVNum++)
                    {
                        DAOFactory.CharacterSkillDAO.InsertOrUpdate(new CharacterSkillDTO
                        {
                            CharacterId = characterDTO.CharacterId,
                            SkillVNum = skillVNum
                        });
                    }

                    DAOFactory.CharacterSkillDAO.InsertOrUpdate(new CharacterSkillDTO { CharacterId = characterDTO.CharacterId, SkillVNum = 1565 });

                    Inventory inventory = new Inventory(new Character(characterDTO));
                    inventory.AddNewToInventory(5826, 1, InventoryType.Main);
                    inventory.Values.ToList().ForEach(i => DAOFactory.ItemInstanceDAO.InsertOrUpdate(i));
                    new EntryPointPacketHandler(session).LoadCharacters(new EntryPointPacket { IgnoreSecurity = true, PacketData = characterCreatePacket.OriginalContent });
                }
            }
        }
    }
}
