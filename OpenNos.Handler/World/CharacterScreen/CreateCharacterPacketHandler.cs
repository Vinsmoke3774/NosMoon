using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.CharacterScreen
{
    public class CreateCharacterPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public CreateCharacterPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// Char_NEW character creation character
        /// </summary>
        /// <param name="characterCreatePacket"></param>
        public void CreateCharacter(CharacterCreatePacket characterCreatePacket)
            => Session.CreateCharacterAction(characterCreatePacket, ClassType.Adventurer);
    }
}
