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
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.HalloweenEvent
{
    [NRunHandler(NRunType.GetHalloweenBagOfSweets)]
    public class GetHalloweenBagOfSweetsHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetHalloweenBagOfSweetsHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet) || !ServerManager.Instance.Configuration.HalloweenEvent)
            {
                return;
            }

            if (Session.Character.Level < 20)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_LOW_LVL"), 0));
                return;
            }

            if (Session.Character.GeneralLogs.Any(s => s.LogType == "DailyReward" && short.Parse(s.LogData) == 1917 && s.Timestamp.Date == DateTime.Today))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("QUEST_ALREADY_DONE"), 0));
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.Character.GeneralLogs.Add(new GeneralLogDTO
            {
                AccountId = Session.Account.AccountId,
                CharacterId = Session.Character.CharacterId,
                IpAddress = Session.CleanIpAddress,
                LogData = "1917",
                LogType = "DailyReward",
                Timestamp = DateTime.Now
            });
            short amount = 1;
            if (Session.Character.IsMorphed)
            {
                amount *= 2;
            }

            Session.Character.GiftAdd(1917, amount);
        }
    }
}
