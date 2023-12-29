using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.Bazaar
{
    public class OpenBazaarPacketHandler : IPacketHandler
    {
        private static readonly KeepAliveClient _keepAliveClient = KeepAliveClient.Instance;

        private ClientSession Session { get; set; }

        public OpenBazaarPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// c_skill packet
        /// </summary>
        /// <param name="cSkillPacket"></param>
        public void OpenBazaar(CSkillPacket cSkillPacket)
        {
            if (!_keepAliveClient.IsBazaarOnline())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo($"Uh oh, it looks like the bazaar server is offline ! Please inform a staff member about it as soon as possible !"));
                return;
            }

            StaticBonusDTO medal = Session.Character.StaticBonusList.Find(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

            if (medal != null)
            {
                MedalType medalType = medal.StaticBonusType == StaticBonusType.BazaarMedalGold ? MedalType.Gold : MedalType.Silver;

                int time = (int)(medal.DateEnd - DateTime.Now).TotalHours;

                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOTICE_BAZAAR"), 0));
                Session.SendPacket($"wopen 32 {(byte)medalType} {time}");
            }
            else
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
            }
        }
    }
}
