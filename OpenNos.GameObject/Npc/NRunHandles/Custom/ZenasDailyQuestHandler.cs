﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.ZenasDailyQuest)]
    public class ZenasDailyQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ZenasDailyQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.Character.AddQuest(22030);
        }
    }
}
