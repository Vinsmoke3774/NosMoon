﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.ChristmasEvent
{
    [NRunHandler(NRunType.GetDailyQuestChristmasSorayaStyle)]
    public class SorayaDailyChristmasQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public SorayaDailyChristmasQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet) || !ServerManager.Instance.Configuration.ChristmasEvent)
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.Character.AddQuest(5942);
        }
    }
}
