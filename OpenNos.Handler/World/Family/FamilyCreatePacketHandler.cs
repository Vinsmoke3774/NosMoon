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
    public class FamilyCreatePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyCreatePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// glmk packet
        /// </summary>
        /// <param name="createFamilyPacket"></param>
        public void CreateFamily(CreateFamilyPacket createFamilyPacket)
        {
            if (Session.Character.Group?.GroupType == GroupType.Group && Session.Character.Group.SessionCount == 3)
            {
                foreach (ClientSession session in Session.Character.Group.Sessions.GetAllItems())
                {
                    if (session.Character.Family != null || session.Character.FamilyCharacter != null)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("PARTY_MEMBER_IN_FAMILY")));
                        return;
                    }
                    else if (session.Character.LastFamilyLeave > DateTime.Now.AddDays(-1).Ticks)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("PARTY_MEMBER_HAS_PENALTY")));
                        return;
                    }
                }

                if (Session.Character.Gold < 200000)
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY")));
                    return;
                }

                string name = createFamilyPacket.CharacterName;
                if (DAOFactory.FamilyDAO.LoadByName(name) != null)
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(
                            Language.Instance.GetMessageFromKey("FAMILY_NAME_ALREADY_USED")));
                    return;
                }

                Session.Character.Gold -= 200000;
                Session.SendPacket(Session.Character.GenerateGold());
                FamilyDTO family = new FamilyDTO
                {
                    Name = name,
                    FamilyExperience = 0,
                    FamilyLevel = 1,
                    FamilyMessage = "",
                    FamilyFaction = Session.Character.Faction != FactionType.None ? (byte)Session.Character.Faction : (byte)ServerManager.RandomNumber(1, 2),
                    MaxSize = 100,
                    LastRename = DateTime.Now.AddDays(-14)
                };
                DAOFactory.FamilyDAO.InsertOrUpdate(ref family);

                Logger.Log.LogUserEvent("GUILDCREATE", Session.GenerateIdentity(), $"[FamilyCreate][{family.FamilyId}]");

                ServerManager.Instance.Broadcast(
                    UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("FAMILY_FOUNDED"), name), 0));
                foreach (ClientSession session in Session.Character.Group.Sessions.GetAllItems())
                {
                    session.Character.ChangeFaction(FactionType.None);
                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = session.Character.CharacterId,
                        DailyMessage = "",
                        Experience = 0,
                        Authority = Session.Character.CharacterId == session.Character.CharacterId
                            ? FamilyAuthority.Head
                            : FamilyAuthority.Familydeputy,
                        FamilyId = family.FamilyId,
                        Rank = 0
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                }

                ServerManager.Instance.FamilyRefresh(family.FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                {
                    DestinationCharacterId = family.FamilyId,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                });
                Observable.Timer(TimeSpan.FromSeconds(5)).SafeSubscribe(o =>
                {
                    if (Session == null)
                    {
                        return;
                    }

                    ServerManager.Instance.FamilyRefresh(family.FamilyId);
                });
                Observable.Timer(TimeSpan.FromSeconds(10)).SafeSubscribe(o =>
                {
                    if (Session == null)
                    {
                        return;
                    }

                    ServerManager.Instance.FamilyRefresh(family.FamilyId);
                });
            }
        }
    }
}
