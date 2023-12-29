using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.Lock
{
    [PacketHeader("$Lock", PassNonParseablePacket = true, Authority =  AuthorityType.User)]
    public class LockPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public string Code { get; set; }

        #region Properties

        public static string ReturnHelp() => "$Lock <Code>";

        #endregion
    }
}