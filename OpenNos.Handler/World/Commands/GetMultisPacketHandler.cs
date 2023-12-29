using System.Collections.Generic;
using System.Linq;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.DAL;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Commands
{
    public class GetMultisPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public GetMultisPacketHandler(ClientSession session) => Session = session;

        public void Execute(GetMultisCommandPacket packet)
        {
            if (string.IsNullOrEmpty(packet.CharacterName))
            {
                GetMultisCommandPacket.ReturnHelp();
                return;
            }

            var character = DAOFactory.CharacterDAO.LoadByName(packet.CharacterName);

            if (character == null)
            {
                Session.SendPacket(Session.Character.GenerateSay("This character could not be found.", 11));
                return;
            }

            var baseAccountId = character.AccountId;

            var ipListForId = DAOFactory.GeneralLogDAO.LoadByLogTypeAndAccountId("Connection", baseAccountId).Select(s => s.IpAddress.CleanIpAddress()).ToHashSet();

            var accountList = new HashSet<long>();
            foreach (var ip in ipListForId)
            {
                var logs = DAOFactory.GeneralLogDAO.LoadByIp(ip).ToList();


                logs.ForEach(s =>
                {
                    if (s.AccountId.HasValue)
                    {
                        accountList.Add(s.AccountId.Value);
                    }
                });
            }

            Session.SendPacket(Session.Character.GenerateSay($"ACCOUNT IDS:", 11));

            foreach (var accountId in accountList)
            {
                Session.SendPacket(Session.Character.GenerateSay($"{accountId}", 12));
            }
        }
    }
}
