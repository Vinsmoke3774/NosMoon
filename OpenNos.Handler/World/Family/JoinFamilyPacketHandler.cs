using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.Family
{
    public class JoinFamilyPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public JoinFamilyPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// gjoin packet
        /// </summary>
        /// <param name="joinFamilyPacket"></param>
        public void JoinFamily(JoinFamilyPacket joinFamilyPacket)
        {
            long characterId = joinFamilyPacket.CharacterId;

            if (joinFamilyPacket.Type == 1)
            {
                if (Session.Character.Family != null)
                {
                    return;
                }

                ClientSession inviteSession = ServerManager.Instance.GetSessionByCharacterId(characterId);

                if (inviteSession?.Character.FamilyInviteCharacters.GetAllItems().Contains(Session.Character.CharacterId) == true
                    && inviteSession.Character.Family != null
                    && inviteSession.Character.Family.FamilyCharacters != null)
                {
                    if (inviteSession.Character.Family.FamilyCharacters.Count + 1 > inviteSession.Character.Family.MaxSize)
                    {
                        return;
                    }

                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = Session.Character.CharacterId,
                        DailyMessage = "",
                        Experience = 0,
                        Authority = FamilyAuthority.Member,
                        FamilyId = inviteSession.Character.Family.FamilyId,
                        Rank = 0
                    };

                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);

                    inviteSession.Character.Family.InsertFamilyLog(FamilyLogType.UserManaged,
                        inviteSession.Character.Name, Session.Character.Name);

                    Logger.Log.LogUserEvent("GUILDJOIN", Session.GenerateIdentity(),
                        $"[FamilyJoin][{inviteSession.Character.Family.FamilyId}]");

                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                    {
                        DestinationCharacterId = inviteSession.Character.Family.FamilyId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("FAMILY_JOINED"), Session.Character.Name,
                                inviteSession.Character.Family.Name), 0),
                        Type = MessageType.Family
                    });

                    long familyId = inviteSession.Character.Family.FamilyId;

                    Session.Character.Family = inviteSession.Character.Family;
                    Session.Character.ChangeFaction((FactionType)inviteSession.Character.Family.FamilyFaction);
                    Observable.Timer(TimeSpan.FromSeconds(5)).SafeSubscribe(o =>
                    {
                        if (Session == null)
                        {
                            return;
                        }

                        ServerManager.Instance.FamilyRefresh(familyId);
                    });
                    Observable.Timer(TimeSpan.FromSeconds(10)).SafeSubscribe(o =>
                    {
                        if (Session == null)
                        {
                            return;
                        }

                        ServerManager.Instance.FamilyRefresh(familyId);
                    });
                    Session.SendPacket(Session.Character.GenerateFamilyMember());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
                    inviteSession?.SendPacket(inviteSession?.Character.GenerateGInfo());
                }
            }
        }
    }
}
