using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets.Family;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.World.Family
{
    public class CmdFamilyDonationPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CmdFamilyDonationPacketHandler(ClientSession session) => Session = session;

        public void FamilyDonation(FamilyGoldDonatePacket donate)
        {
            if (donate == null)
            {
                return;
            }

            if (Session.Character.Family == null)
            {
                Session.SendPacket("info You don't have a family!");
                return;
            }

            long maxFamilyGold = ServerManager.Instance.Configuration.MaxFamilyBankGold;
            var family = (FamilyDTO)Session.Character.Family;

            if (donate?.Amount <= 0)
            {
                return;
            }

            if (donate.Amount > Session.Character.Gold)
            {
                Session.SendPacket("info You can't donate money you don't have!");
                return;
            }
            if (donate.Amount + Session.Character.Family.FamilyGold > maxFamilyGold)
            {
                Session.SendPacket("info Limit of 300.000.000.000 Gold reached!");
                return;
            }

            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = Session.Character.Family.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = UserInterfaceHelper.GenerateMsg($"{Session.Character.Name} just donated {donate.Amount} Gold to our Familybank!", 0),
                Type = MessageType.Family
            });

            Session.ReceivePacket($": I just donated {donate.Amount} to our Familybank!");
            Session.Character.Gold -= donate.Amount;
            Session.SendPacket(Session.Character.GenerateGold());
            family.FamilyGold += donate.Amount;

            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = Session.Character.Family.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Type = MessageType.Family
            });

            DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
            ServerManager.Instance.FamilyRefresh(family.FamilyId);
            Session.SendPacket(UserInterfaceHelper.GenerateInfo($"You just donated {donate.Amount} Gold to your family from your inventory!"));
        }
    }
}
