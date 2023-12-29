using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class GroupTalkPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public GroupTalkPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// ; packet
        /// </summary>
        /// <param name="groupSayPacket"></param>
        public void GroupTalk(GroupSayPacket groupSayPacket)
        {
            if (!string.IsNullOrEmpty(groupSayPacket.Message))
            {
                GroupType groupType = Session.Character.Group?.GroupType ?? GroupType.Group;
                //ServerManager.Instance.ChatLogs.Add(new ChatLogDTO
                //{
                //    CharacterId = Session.Character.CharacterId,
                //    CharacterName = Session.Character.Name,
                //    DateTime = DateTime.Now,
                //    MessageType = groupType == GroupType.Group ? "Group" : groupType == GroupType.RBBBlue || groupType == GroupType.RBBRed ? "Rainbow Battle" : groupType == GroupType.TalentArena ? "Talent Arena" : "Raid",

                //});
                ServerManager.Instance.Broadcast(Session, Session.Character.GenerateSpk(groupSayPacket.Message, 3),
                    ReceiverType.Group);
            }
        }
    }
}
