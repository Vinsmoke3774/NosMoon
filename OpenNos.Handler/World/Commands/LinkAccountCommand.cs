using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using WindowsFirewallHelper.Helpers;

namespace OpenNos.Handler.World.Commands
{
    public class LinkAccountCommand : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public LinkAccountCommand(ClientSession session) => Session = session;

        public void Execute(LinkCommandPacket packet)
        {
            var alreadyLinked = DAOFactory.WhitelistedCharacterDao.LoadByIpAddress(Session.CleanIpAddress);

            if (alreadyLinked != null)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Your ip is already linked !"));
                return;
            }

            var dto = new WhitelistedPlayerDTO
            {
                IpAddress = Session.CleanIpAddress
            };

            var result = DAOFactory.WhitelistedCharacterDao.InsertOrUpdate(dto);

            var rule = FirewallManager.Instance.Rules.FirstOrDefault(s => s.Name == "whitelist");

            if (rule != null)
            {
                var ruleContent = rule.RemoteAddresses.ToList();
                var addressBytes = IPAddress.Parse(Session.CleanIpAddress).GetAddressBytes();
                
                ruleContent.Add(new SingleIP(addressBytes));
                rule.RemoteAddresses = ruleContent.ToArray();
            }

            if (result == SaveResult.Inserted || result == SaveResult.Updated)
            {
                Session.SendPacket(Session.Character.GenerateSay("Success !", 11));
                return;
            }

            Session.SendPacket(Session.Character.GenerateSay("Oopsie, something went wrong !", 11));
        }
    }
}
