using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$AddQuest", PassNonParseablePacket = true, Authority = AuthorityType.GD)]
    public class AddQuestPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short QuestId { get; set; }

        public static string ReturnHelp() => "$AddQuest <QuestId>";

        #endregion
    }
}
