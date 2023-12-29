using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class GitPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public GitPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// GitPacket packet
        /// </summary>
        /// <param name="packet"></param>
        public void Git(GitPacket packet)
        {
            MapButton button = Session.CurrentMapInstance.Buttons.Find(s => s.MapButtonId == packet.ButtonId);
            if (button != null)
            {
                if (Session.Character.IsVehicled)
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_DO_VEHICLED"), 10));
                    return;
                }

                Session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Object, button.MapButtonId));
                button.RunAction();
                Session.CurrentMapInstance.Broadcast(button.GenerateIn());
            }
        }
    }
}
