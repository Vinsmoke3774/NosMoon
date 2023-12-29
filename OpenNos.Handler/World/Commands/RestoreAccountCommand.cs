using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.CommandPackets;
using NosByte.Packets.CustomPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Commands
{
    public class RestoreAccountCommand : IPacketHandler
    {
        private ClientSession Session { get; }

        public RestoreAccountCommand(ClientSession session) => Session = session;

        public void Execute(RestoreAccountPacket packet)
        {
            if (string.IsNullOrEmpty(packet.AccountName))
            {
                return;
            }

            var foundAccount = DAOFactory.AccountDAO.LoadByName(packet.AccountName);

            if (foundAccount == null)
            {
                return;
            }

            if (!foundAccount.ShowCharacters.HasValue || foundAccount.ShowCharacters.Value)
            {
                return;
            }

            foundAccount.ShowCharacters = true;
            DAOFactory.AccountDAO.InsertOrUpdate(ref foundAccount);
            Session.SendPacket(Session.Character.GenerateSay($"Account restored.", 11));
            var characters = DAOFactory.CharacterDAO.LoadAllByAccount(foundAccount.AccountId);

            if (!characters.Any(s => s.Name.Contains($"DBLEAK")))
            {
                return;
            }


            Session.SendPacket(Session.Character.GenerateSay($"Locked Characters on this account:", 12));
            foreach (var character in characters)
            {
                if (character.Name.Contains("[DBLEAK] "))
                {
                    Session.SendPacket(Session.Character.GenerateSay(character.Name, 12));
                }
            }
        }
    }
}
