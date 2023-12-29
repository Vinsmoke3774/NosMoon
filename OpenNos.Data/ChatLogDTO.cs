using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class ChatLogDTO : SynchronizableBaseDTO
    {
        public long CharacterId { get; set; }

        public string CharacterName { get; set; }

        public string MessageType { get; set; }

        public string Message { get; set; }

        public long DestinationCharacterId { get; set; }

        public string DestinationCharacterName { get; set; }

        public DateTime DateTime { get; set; }

        public long FamilyId { get; set; }

        public string FamilyName { get; set; }

        public string NoteTitle { get; set; }
    }
}
