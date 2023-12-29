using System.Linq;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Commands
{
    public class AccountInfoCommand : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public AccountInfoCommand(ClientSession session) => Session = session;

        public void Execute(AccountInfoPacket packet)
        {
            if (string.IsNullOrEmpty(packet.AccountName))
            {
                AccountInfoPacket.ReturnHelp();
                return;
            }

            var accountDto = DAOFactory.AccountDAO.LoadByName(packet.AccountName);

            if (accountDto == null)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("This account doesn't exist."));
                return;
            }

            var characters = DAOFactory.CharacterDAO.LoadByAccount(accountDto.AccountId).ToList();

            characters.ForEach(s =>
            {
                Session.SendPacket(Session.Character.GenerateSay($"Character on account: {s.Name} | Id: {s.CharacterId}", 11));
            });
        }
    }
}
