using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.CharacterScreen
{
    public class RenameCharacterPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RenameCharacterPacketHandler(ClientSession session) => Session = session;

        public void CharRename(CharacterRenamePacket e)
        {
            if (Session.HasCurrentMapInstance) return;

            if (e == null) return;

            var character = DAOFactory.CharacterDAO.LoadBySlot(Session.Account.AccountId, e.Slot);

            if (character == null) return;

            if (!character.IsChangeName) return;

            if (e.Slot > 3) return;



            if (e.Name.Length <= 3 || e.Name.Length >= 15) return;

            var rg = new Regex(
                @"^[\u0021-\u007E\u00A1-\u00AC\u00AE-\u00FF\u4E00-\u9FA5\u0E01-\u0E3A\u0E3F-\u0E5B\u002E]*$");
            if (rg.Matches(e.Name).Count != 1)
            {
                Session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("INVALID_CHARNAME")}");
                return;
            }

            if (DAOFactory.CharacterDAO.LoadByName(e.Name) != null)
            {
                Session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("ALREADY_TAKEN")}");
                return;
            }

            var BlackListed = new List<string>
            {
                "[",
                "]",
                "[gm]",
                "[nh]",
                " ", // Space
                " " // Alt+255
            };

            if (BlackListed.Any(s => e.Name.ToLower().Contains(s)))
            {
                Session.SendPacketFormat($"info {Language.Instance.GetMessageFromKey("INVALID_CHARNAME")}");
                return;
            }

            character.IsChangeName = !character.IsChangeName;

            character.Name = e.Name;

            PenaltyLogDTO penaltyLog = new PenaltyLogDTO
            {
                AccountId = character.AccountId,
                Reason = "Namechange",
                Penalty = PenaltyType.Namechange,
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now.AddDays(30),
                AdminName = "Renaming Card"
            };
            Character.InsertOrUpdatePenalty(penaltyLog);

            DAOFactory.CharacterDAO.InsertOrUpdate(ref character);

            new EntryPointPacketHandler(Session).LoadCharacters(new EntryPointPacket { IgnoreSecurity = true, PacketData = string.Empty });
        }
    }
}
