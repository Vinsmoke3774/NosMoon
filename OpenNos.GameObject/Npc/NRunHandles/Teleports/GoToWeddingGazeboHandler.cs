﻿using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToWeddingGazebo)]
    public class GoToWeddingGazeboHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToWeddingGazeboHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeTeleporter(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (Session.Character.Gold >= 500)
            {
                Session.Character.Gold -= 500;
                Session.SendPacket(Session.Character.GenerateGold());
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Tp.MapId, Tp.MapX, Tp.MapY);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
            }
        }
    }
}
