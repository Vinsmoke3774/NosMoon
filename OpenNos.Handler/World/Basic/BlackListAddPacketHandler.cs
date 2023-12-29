using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Basic
{
    public class BlackListAddPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public BlackListAddPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// blins packet
        /// </summary>
        /// <param name="blInsPacket"></param>
        public void BlacklistAdd(BlInsPacket blInsPacket)
        {
            if (Session.Character.CharacterId == blInsPacket.CharacterId)
            {
                return;
            }

            if (DAOFactory.CharacterDAO.LoadById(blInsPacket.CharacterId) is CharacterDTO character
                && DAOFactory.AccountDAO.LoadById(character.AccountId).Authority >= AuthorityType.TMOD)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_BLACKLIST_TEAM")));
                return;
            }

            Session.Character.AddRelation(blInsPacket.CharacterId, CharacterRelationType.Blocked);
            Session.SendPacket(
                UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_ADDED")));
            Session.SendPacket(Session.Character.GenerateBlinit());
        }
    }
}
