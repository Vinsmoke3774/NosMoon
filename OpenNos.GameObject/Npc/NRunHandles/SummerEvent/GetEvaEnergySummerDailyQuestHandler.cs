﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.SummerEvent
{
    [NRunHandler(NRunType.GetDailyEvaEnergySummerQuest)]
    public class GetEvaEnergySummerDailyQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetEvaEnergySummerDailyQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }
            return;
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.Character.AddQuest(22022);
        }
    }
}
