using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Family
{
    public class ChangeFamilyAuthorityPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ChangeFamilyAuthorityPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// fauth packet
        /// </summary>
        /// <param name="fAuthPacket"></param>
        public void ChangeAuthority(FAuthPacket fAuthPacket)
        {
            if (Session.Character.Family == null || (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Familydeputy))
            {
                return;
            }

            Session.Character.Family.InsertFamilyLog(FamilyLogType.RightChanged, Session.Character.Name,
                authority: fAuthPacket.MemberType, righttype: fAuthPacket.AuthorityId + 1,
                rightvalue: fAuthPacket.Value);
            switch (fAuthPacket.MemberType)
            {
                case FamilyAuthority.Familykeeper:
                    switch (fAuthPacket.AuthorityId)
                    {
                        case 0:
                            Session.Character.Family.ManagerCanInvite = fAuthPacket.Value == 1;
                            break;

                        case 1:
                            Session.Character.Family.ManagerCanNotice = fAuthPacket.Value == 1;
                            break;

                        case 2:
                            Session.Character.Family.ManagerCanShout = fAuthPacket.Value == 1;
                            break;

                        case 3:
                            Session.Character.Family.ManagerCanGetHistory = fAuthPacket.Value == 1;
                            break;

                        case 4:
                            Session.Character.Family.ManagerAuthorityType = (FamilyAuthorityType)fAuthPacket.Value;
                            break;
                    }

                    break;

                case FamilyAuthority.Member:
                    switch (fAuthPacket.AuthorityId)
                    {
                        case 0:
                            Session.Character.Family.MemberCanGetHistory = fAuthPacket.Value == 1;
                            break;

                        case 1:
                            Session.Character.Family.MemberAuthorityType = (FamilyAuthorityType)fAuthPacket.Value;
                            break;
                    }

                    break;
            }

            FamilyDTO fam = Session.Character.Family;
            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
            ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = fam.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });
            Session.SendPacket(Session.Character.GenerateGInfo());
        }
    }
}
