using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.Bazaar
{
    public class RefreshPersonalListPacketHandler : IPacketHandler
    {
        private static readonly KeepAliveClient _keepAliveClient = KeepAliveClient.Instance;

        private ClientSession Session { get; set; }

        public RefreshPersonalListPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// c_slist packet
        /// </summary>
        /// <param name="csListPacket"></param>
        public void RefreshPersonalBazarList(CSListPacket csListPacket)
        {
            if (!_keepAliveClient.IsBazaarOnline())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo($"Uh oh, it looks like the bazaar server is offline ! Please inform a staff member about it as soon as possible !"));
                return;
            }
            if (!Session.Character.CanUseNosBazaar())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
                return;
            }

            Session.SendPacket(Session.Character.GenerateRCSList(csListPacket));
        }
    }
}
