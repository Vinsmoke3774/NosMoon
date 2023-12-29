﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.AutoPerfAll)]
    public class AutoPerfAllHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public AutoPerfAllHandler(ClientSession session) : base(session)
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
            var specialist = Session.Character.Inventory.LoadBySlotAndType(0, InventoryType.Equipment);

            if (specialist == null)
            {
                Session.SendPacket(Session.Character.GenerateSay("Please place your specialist in the first slot of your inventory.", 11));
                return;
            }

            specialist.MultiplePerfection(Session, -1);
        }
    }
}
