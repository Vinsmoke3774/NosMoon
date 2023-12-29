using System.Linq;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Family
{
    public class FamilyTalkPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public FamilyTalkPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// : packet
        /// </summary>
        /// <param name="familyChatPacket"></param>
        public void FamilyChat(FamilyChatPacket familyChatPacket)
        {
            if (string.IsNullOrEmpty(familyChatPacket.Message))
            {
                return;
            }

            if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RainbowBattleInstance ||
                Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.CaligorInstance)
            {
                Session.SendPacket(Session.Character.GenerateSay("You cannot speak to your family on this map.", 12));
                return;
            }

            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
            {
                return;
            }

            string msg = familyChatPacket.Message;
            string ccmsg = $"[{Session.Character.Name}]:{msg}";
            if (Session.Account.Authority >= AuthorityType.TMOD)
            {
                ccmsg = $"[{Session.Account.Authority} {Session.Character.Name}]:{msg}";
            }

            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = Session.Character.Family.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = ccmsg,
                Type = MessageType.FamilyChat
            });
            Parallel.ForEach(ServerManager.Instance.Sessions.ToList(), session =>
            {
                if (!session.HasSelectedCharacter || session.Character.Family == null ||
                    Session.Character.Family == null ||
                    session.Character.Family?.FamilyId != Session.Character.Family?.FamilyId)
                {
                    return;
                }

                if (Session.HasCurrentMapInstance && session.HasCurrentMapInstance
                                                  && Session.CurrentMapInstance == session.CurrentMapInstance)
                {
                    if (Session.Account.Authority != AuthorityType.GS || Session.Account.Authority != AuthorityType.TGS && !Session.Character.InvisibleGm)
                    {
                        session.SendPacket(Session.Character.GenerateSay(msg, 6));
                    }
                    else
                    {
                        session.SendPacket(Session.Character.GenerateSay(ccmsg, 6, true));
                    }
                }
                else
                {
                    session.SendPacket(Session.Character.GenerateSay(ccmsg, 6));
                }

                if (!Session.Character.InvisibleGm)
                {
                    session.SendPacket(Session.Character.GenerateSpk(msg, 1));
                }
            });
        }
    }
}
