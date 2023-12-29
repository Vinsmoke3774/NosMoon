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
using OpenNos.Core.Handling;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.CharacterScreen
{
    public class EntryPointPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public EntryPointPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// Load Characters, this is the Entrypoint for the Client, Wait for 3 Packets.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <param name="ignoreSecurity"></param>
        public void LoadCharacters(EntryPointPacket entryPoint)
        {
            string[] loginPacketParts = entryPoint.PacketData?.Split(' ') ?? new string[0];
            bool isCrossServerLogin = false;


            // Load account by given SessionId
            if (Session.Account == null)
            {

                bool hasRegisteredAccountLogin = true;
                AccountDTO account = null;
                if (loginPacketParts.Length > 3)
                {
                    if (loginPacketParts.Length > 6 && loginPacketParts[3] == "DAC"
                        && loginPacketParts[7] == "CrossServerAuthenticate")
                    {
                        isCrossServerLogin = true;
                        account = DAOFactory.AccountDAO.LoadByName(loginPacketParts[4]);
                    }
                    else
                    {
                        account = DAOFactory.AccountDAO.LoadByName(loginPacketParts[3]);
                    }
                }

                try
                {
                    if (account != null)
                    {
                        if (isCrossServerLogin)
                        {
                            hasRegisteredAccountLogin =
                                CommunicationServiceClient.Instance.IsCrossServerLoginPermitted(account.AccountId,
                                    Session.SessionId);
                        }
                        else
                        {
                            hasRegisteredAccountLogin =
                                CommunicationServiceClient.Instance.IsLoginPermitted(account.AccountId,
                                    Session.SessionId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log.Error("MS Communication Failed.", ex);
                    Session.Disconnect();
                    return;
                }

                if (loginPacketParts.Length <= 3 || !hasRegisteredAccountLogin)
                {
                    Logger.Log.Debug(
                            $"Client {Session.ClientId} forced Disconnection, login has not been registered or Account is already logged in.");
                    Session.Disconnect();
                    return;
                }

                if (account == null)
                {
                    Logger.Log.Debug($"Client {Session.ClientId} forced Disconnection, invalid AccountName.");
                    Session.Disconnect();
                    return;
                }

                if (!account.Password.ToLower().Equals(CryptographyBase.Sha512(loginPacketParts[7])) &&
                    !isCrossServerLogin)
                {
                    Logger.Log.Debug($"Client {Session.ClientId} forced Disconnection, invalid Password.");
                    Session.Disconnect();
                    return;
                }

                Session.InitializeAccount(new Account(account), isCrossServerLogin);
                ServerManager.Instance.CharacterScreenSessions[Session.Account.AccountId] = Session;
            }

            if (isCrossServerLogin)
            {
                if (byte.TryParse(loginPacketParts[5], out byte slot))
                {
                    new SelectCharacterPacketHandler(Session).SelectCharacter(new SelectPacket { Slot = slot });
                }
            }
            else
            {
                var playerConnected = ServerManager.Instance.Sessions.Count();
                var maxPlayerAllowed = Session.Account.Authority >= AuthorityType.GS ? 999 : 150; // Why the fuck is this hardcoded here ?
                if (playerConnected > maxPlayerAllowed)
                {
                    Session.SendPacket("info Channel is full");
                    Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(o =>
                    {
                        if (Session == null)
                        {
                            return;
                        }

                        Session.Disconnect();
                    });
                    return;
                }
                // TODO: Wrap Database access up to GO
                IEnumerable<CharacterDTO> characters = DAOFactory.CharacterDAO.LoadByAccount(Session.Account.AccountId);

                Logger.Log.Info(string.Format(Language.Instance.GetMessageFromKey("ACCOUNT_ARRIVED"), Session.SessionId));

                // load characterlist packet for each character in CharacterDTO
                Session.SendPacket("clist_start 0");

                if (!Session.Account.ShowCharacters.HasValue || Session.Account.ShowCharacters.Value)
                {
                    foreach (CharacterDTO character in characters)
                    {
                        IEnumerable<ItemInstanceDTO> inventory =
                            DAOFactory.ItemInstanceDAO.LoadByType(character.CharacterId, InventoryType.Wear);

                        ItemInstance[] equipment = new ItemInstance[17];

                        foreach (ItemInstanceDTO equipmentEntry in inventory)
                        {
                            // explicit load of iteminstance
                            ItemInstance currentInstance = new ItemInstance(equipmentEntry);

                            if (currentInstance != null)
                            {
                                equipment[(short)currentInstance.Item.EquipmentSlot] = currentInstance;
                            }
                        }

                        string petlist = "";

                        List<MateDTO> mates = DAOFactory.MateDAO.LoadByCharacterId(character.CharacterId).ToList();

                        for (int i = 0; i < 26; i++)
                        {
                            //0.2105.1102.319.0.632.0.333.0.318.0.317.0.9.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1
                            petlist += (i != 0 ? "." : "") + (mates.Count > i ? $"{mates[i].Skin}.{mates[i].NpcMonsterVNum}" : "-1");
                        }

                        // 1 1 before long string of -1.-1 = act completion
                        Session.SendPacket($"clist {character.Slot} {character.Name} 0 {(byte)character.Gender} {(byte)character.HairStyle} {(byte)character.HairColor} 0 {(byte)character.Class} {character.Level} {character.HeroLevel} {equipment[(byte)EquipmentType.Hat]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Armor]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.WeaponSkin]?.ItemVNum ?? (equipment[(byte)EquipmentType.MainWeapon]?.ItemVNum ?? -1)}.{equipment[(byte)EquipmentType.SecondaryWeapon]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Mask]?.ItemVNum ?? -1}.{equipment[(byte)EquipmentType.Fairy]?.ItemVNum ?? -1}.{(equipment[(byte)EquipmentType.CostumeSuit]?.FusionVnum != 0 ? equipment[(byte)EquipmentType.CostumeSuit]?.FusionVnum : equipment[(byte)EquipmentType.CostumeSuit]?.ItemVNum ?? -1)}.{(equipment[(byte)EquipmentType.CostumeHat]?.FusionVnum != 0 ? equipment[(byte)EquipmentType.CostumeHat]?.FusionVnum : equipment[(byte)EquipmentType.CostumeHat]?.ItemVNum ?? -1)} {character.JobLevel}  1 1 {petlist} {(equipment[(byte)EquipmentType.Hat]?.Item.IsColored == true ? equipment[(byte)EquipmentType.Hat].Design : 0)} {(character.IsChangeName ? 1 : 0)}");
                    }
                }

                Session.SendPacket("clist_end");
            }
        }
    }
}
