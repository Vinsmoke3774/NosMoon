using System;
using System.Collections.Concurrent;
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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class SearchNamePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SearchNamePacketHandler(ClientSession session) => Session = session;

        public void SearchName(TawPacket packet)
        {
            ConcurrentBag<ArenaTeamMember> at = ServerManager.Instance.ArenaTeams.ToList().FirstOrDefault(s => s.Any(o => o.Session?.Character?.Name == packet.Username && Session.CurrentMapInstance != null));
            if (at != null)
            {
                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, at.FirstOrDefault(s => s.Session != null).Session.CurrentMapInstance.MapInstanceId, 69, 100);

                var zenas = at.OrderBy(s => s.Order).FirstOrDefault(s => s.Session != null && !s.Dead && s.ArenaTeamType == ArenaTeamType.Zenas);
                var erenia = at.OrderBy(s => s.Order).FirstOrDefault(s => s.Session != null && !s.Dead && s.ArenaTeamType == ArenaTeamType.Erenia);
                Session.SendPacket(Session.Character.GenerateTaM(0));
                Session.SendPacket(Session.Character.GenerateTaM(3));
                Session.SendPacket("taw_sv 0");
                //Session.SendPacket(zenas?.Session.Character.GenerateTaP(0, true));
                //Session.SendPacket(erenia?.Session.Character.GenerateTaP(2, true));
                Session.SendPacket(zenas?.Session.Character.GenerateTaFc(0));
                Session.SendPacket(erenia?.Session.Character.GenerateTaFc(1));
            }
            else
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_FOUND_IN_ARENA")));
            }
        }
    }
}
