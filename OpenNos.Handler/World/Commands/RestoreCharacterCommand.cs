using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using System.Linq;

namespace OpenNos.Handler.World.Commands
{
    public class RestoreCharacterCommand : IPacketHandler
    {
        private ClientSession Session { get; }

        public RestoreCharacterCommand(ClientSession session) => Session = session;

        public void Execute(RestoreCharacterPacket packet)
        {
            if (string.IsNullOrEmpty(packet.Name))
            {
                RestoreCharacterPacket.ReturnHelp();
            }

            var concat = string.Concat("[DBLEAK] ", packet.Name);
            var foundDto = DAOFactory.CharacterDAO.LoadByName(concat);

            if (foundDto == null)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateModal("This character cannot be found.", 1));
                return;
            }

            var allChars = DAOFactory.CharacterDAO.LoadByAccount(foundDto.AccountId)?.ToList();

            if (allChars == null)
            {
                return;
            }

            byte insertionSlot = 0;
            var slotFound = false;
            for (byte slot = 0; slot < 4; slot++)
            {
                var foundInSlot = allChars.Any(s => s.Slot == slot);

                if (foundInSlot)
                {
                    continue;
                }

                Logger.Log.Debug($"New insertion slot found at: {slot}");
                slotFound = true;
                insertionSlot = slot;
                break;
            }

            if (!slotFound)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateModal("No character slots available.", 1));
                return;
            }

            foundDto.Slot = insertionSlot;
            foundDto.State = CharacterState.Active;
            foundDto.Name = foundDto.Name.Replace("[DBLEAK] ", string.Empty);
            DAOFactory.CharacterDAO.InsertOrUpdate(ref foundDto, true);
            Session.SendPacket(Session.Character.GenerateSay($"Character {foundDto.Name} restored.", 12));
        }
    }
}
