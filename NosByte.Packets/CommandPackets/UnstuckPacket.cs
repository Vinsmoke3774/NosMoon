using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$Unstuck", PassNonParseablePacket = true, Authority =  AuthorityType.User )]
    public class UnstuckPacket : PacketDefinition
    {
        #region Methods

        public static string ReturnHelp() => "$Unstuck";

        #endregion
    }
}