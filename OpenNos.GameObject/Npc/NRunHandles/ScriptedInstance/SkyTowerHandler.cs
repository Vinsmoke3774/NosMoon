using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;
using System.Linq;

namespace OpenNos.GameObject.Npc.NRunHandles.ScriptedInstance
{
    [NRunHandler(NRunType.SkyTower)]
    public class SkyTowerHanler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public SkyTowerHanler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (Session.Character.Level <= 90)
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            var skytower = ServerManager.Instance.SkyTowers.FirstOrDefault(s => s.MapId == 2628 && s.Type == Domain.ScriptedInstanceType.SkyTower && s.Round == 1);
            Session.SendPacket(skytower.GenerateRbr2());
        }
    }
}
