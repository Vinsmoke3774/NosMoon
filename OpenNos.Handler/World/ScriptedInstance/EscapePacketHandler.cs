using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class EscapePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public EscapePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// RSelPacket packet
        /// </summary>
        /// <param name="escapePacket"></param>
        public void Escape(EscapePacket escapePacket)
        {
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId,
                    Session.Character.MapX, Session.Character.MapY);
                Session.Character.Timespace = null;
            }
            else if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
            {
                if (Session.Account.Authority < AuthorityType.GM)
                {
                    return;
                }

                ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId,
                    Session.Character.MapX, Session.Character.MapY);
                ServerManager.Instance.GroupLeave(Session);
            }
        }
    }
}
