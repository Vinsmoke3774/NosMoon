using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.HttpClients;
using OpenNos.Master.Library.Client;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.OpenNosBazaar)]
    public class OpenBazaarHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        private static readonly KeepAliveClient _keepAliveClient = KeepAliveClient.Instance;

        public OpenBazaarHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (!Session.Character.CanUseNosBazaar())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
                return;
            }

            if (!_keepAliveClient.IsBazaarOnline())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo($"Uh oh, it looks like the bazaar server is offline ! Please inform a staff member about it as soon as possible !"));
                return;
            }

            MedalType medalType = 0;
            int time = 0;

            StaticBonusDTO medal = Session.Character.StaticBonusList.Find(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

            if (medal != null)
            {
                time = (int)(medal.DateEnd - DateTime.Now).TotalHours;

                switch (medal.StaticBonusType)
                {
                    case StaticBonusType.BazaarMedalGold:
                        medalType = MedalType.Gold;
                        break;

                    case StaticBonusType.BazaarMedalSilver:
                        medalType = MedalType.Silver;
                        break;
                }
            }

            Session.SendPacket($"wopen 32 {(byte)medalType} {time}");
        }
    }
}
