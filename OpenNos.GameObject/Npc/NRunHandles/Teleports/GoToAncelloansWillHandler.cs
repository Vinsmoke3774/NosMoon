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

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToShip)]
    public class GoToAncelloansWillHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToAncelloansWillHandler(ClientSession session) : base(session)
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
            return;
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 243, 71, 245);
        }
    }
}
