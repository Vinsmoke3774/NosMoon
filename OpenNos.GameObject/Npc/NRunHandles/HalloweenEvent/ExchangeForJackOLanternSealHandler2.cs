﻿using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.HalloweenEvent
{
    [NRunHandler(NRunType.ExchangeForJackOLanternSeal2)]
    public class ExchangeForJackOLanternSealHandler2 : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeForJackOLanternSealHandler2(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet) || !ServerManager.Instance.Configuration.HalloweenEvent)
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (packet.Type == 0)
            {
                Session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("ASK_TRADE")}");
            }
            else
            {
                TradeItems(2324, 30, 1916, 1);
            }
        }
    }
}
