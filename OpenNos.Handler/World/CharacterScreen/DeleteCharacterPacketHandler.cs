using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.CharacterScreen
{
    public class DeleteCharacterPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DeleteCharacterPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// Char_DEL packet
        /// </summary>
        /// <param name="characterDeletePacket"></param>
        public void DeleteCharacter(CharacterDeletePacket characterDeletePacket)
        {
            if (Session.HasCurrentMapInstance)
            {
                return;
            }

            if (characterDeletePacket.Password == null)
            {
                return;
            }

            if (Session.Account.ShowCharacters.HasValue && !Session.Account.ShowCharacters.Value)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("You are not allowed to delete characters on this account. Please contact the support."));
                return;
            }

            Logger.Log.LogUserEvent("DELETECHARACTER", Session.GenerateIdentity(),
                $"[DeleteCharacter]Name: {characterDeletePacket.Slot}");
            AccountDTO account = DAOFactory.AccountDAO.LoadById(Session.Account.AccountId);
            if (account == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(account?.LockCode))
            {
                Session.SendPacket("info Remove your lock code first.");
                return;
            }

            if (account.Password.ToLower() == CryptographyBase.Sha512(characterDeletePacket.Password))
            {
                CharacterDTO character =
                    DAOFactory.CharacterDAO.LoadBySlot(account.AccountId, characterDeletePacket.Slot);
                if (character == null)
                {
                    return;
                }

                DAOFactory.CharacterDAO.DeleteByPrimaryKey(account.AccountId, characterDeletePacket.Slot);
                new EntryPointPacketHandler(Session).LoadCharacters(new EntryPointPacket { PacketData = string.Empty, IgnoreSecurity = true });
            }

            else
            {
                Session.SendPacket($"info {Language.Instance.GetMessageFromKey("BAD_PASSWORD")}");
            }
        }
    }
}
