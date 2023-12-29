using System;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Event.ACT7;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.TeleportActVII)]
    public class TeleportActVIIHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TeleportActVIIHandler(ClientSession session) : base(session)
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
            short goldPrice = 0;
            Tuple<short, short, short> mapTuple = new Tuple<short, short, short>(0, 0, 0);
            switch (packet.Type)
            {
                case 170:
                    goldPrice = 30000;
                    mapTuple = new Tuple<short, short, short>(170, 125, 68);
                    break;

                case 145:
                    goldPrice = 25000;
                    mapTuple = new Tuple<short, short, short>(145, 50, 44);
                    break;

                case 2650:
                    if (Session.Character.QuestLogs.All(s => s.QuestId != 6500))
                    {
                        Session.SendPacket("info Accept 'Cause of the arrival (Main Quest)' and talk to Propga first!");
                        return;
                    }
                    goldPrice = 30000;
                    mapTuple = new Tuple<short, short, short>(2650, 20, 18);
                    Act7Ship.Run(Session);
                    break;

                default:
                    return;
            }



            if (Session.Character.Gold < goldPrice)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                return;
            }

            Session.GoldLess(goldPrice);
            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapTuple.Item1, mapTuple.Item2, mapTuple.Item3);
        }
    }
}
