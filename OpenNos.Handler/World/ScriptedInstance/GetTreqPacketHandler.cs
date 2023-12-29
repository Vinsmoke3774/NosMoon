using NosByte.Packets.ServerPackets;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using System.Linq;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class GetTreqPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public GetTreqPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// treq packet
        /// </summary>
        /// <param name="treqPacket"></param>
        public void GetTreq(TreqPacket treqPacket)
        {
            GameObject.ScriptedInstance timespace = Session.CurrentMapInstance.ScriptedInstances.Find(s => treqPacket.X == s.PositionX && treqPacket.Y == s.PositionY).Copy();

            if (timespace == null)
            {
                var getTimespace = ServerManager.Instance.TimeSpaces.FirstOrDefault(s => s.MapId == Session.Character.MapId && s.Type == ScriptedInstanceType.HiddenTsRaid);
                if (getTimespace == null) return;
                var getSession = ServerManager.Instance.GetSessionByCharacterId(getTimespace.CharacterId);
                timespace = ServerManager.Instance.TimeSpaces.Where(s => s == getTimespace).FirstOrDefault(s => Session.Character.Group.IsMemberOfGroup(getSession) || (getSession.Character.CharacterId == Session.Character.CharacterId));
            }

            if (timespace != null)
            {
                if (treqPacket.StartPress == 1 || treqPacket.RecordPress == 1)
                {
                    Session.Character.EnterInstance(timespace);
                }
                else
                {
                    Session.SendPacket(timespace.GenerateRbr(Session.Character));
                }
            }
        }
    }
}
