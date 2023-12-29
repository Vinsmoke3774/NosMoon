using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.CharacterScreen
{
    public class CreateMartialArtistPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public CreateMartialArtistPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// Char_NEW_JOB character creation character
        /// </summary>
        /// <param name="characterJobCreatePacket"></param>
        public void CreateCharacterJob(CharacterJobCreatePacket characterJobCreatePacket)
            => Session.CreateCharacterAction(characterJobCreatePacket, ClassType.MartialArtist);
    }
}
