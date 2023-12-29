using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets
{
    [PacketHeader("$UserLog", PassNonParseablePacket = true, Authority =  AuthorityType.Administrator)]
    public class UserLogPacket : PacketDefinition
    {
        #region Methods

        public static string ReturnHelp() => "$UserLog";

        #endregion
    }
}