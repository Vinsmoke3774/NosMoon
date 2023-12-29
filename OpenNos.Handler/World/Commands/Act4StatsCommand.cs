using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.HttpClients;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.Commands
{
    public class Act4StatsCommand : IPacketHandler
    {
        private ClientSession Session { get; }

        public Act4StatsCommand(ClientSession session) => Session = session;

        public void Execute(Act4StatsPacket packet)
        {
            var angelPercentage = FrozenCrownClient.Instance.GetPercent()?.AngelPercent / 100 ?? 0;
            var demonPercentage = FrozenCrownClient.Instance.GetPercent()?.DemonPercent / 100 ?? 0;
            Session.SendPacket(Session.Character.GenerateSay($"Angels: {angelPercentage}%", 10));
            Session.SendPacket(Session.Character.GenerateSay($"Demons: {demonPercentage}%", 10));
        }
    }
}
