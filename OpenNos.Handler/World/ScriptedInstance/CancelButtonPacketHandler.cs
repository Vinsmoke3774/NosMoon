using NosByte.Packets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class CancelButtonPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CancelButtonPacketHandler(ClientSession session) => Session = session;

        public void ButtonCancel(BscPacket packet)
        {
            switch (packet.Type)
            {
                case 2:
                    var arenamember = ServerManager.Instance.ArenaMembers.ToList().Find(s => s.Session == Session);
                    if (arenamember?.GroupId != null && packet.Option != 1)
                    {
                        Session.SendPacket($"qna #bsc^2^1 {Language.Instance.GetMessageFromKey("ARENA_PENALTY_NOTICE")}");
                        return;
                    }

                    Session.Character.LeaveTalentArena(false);
                    break;

                case 7:
                    Session.Character.IsWaitingForEvent = false;
                    Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 7, 0, 0));
                    return;

                case 9:
                case 10:
                    Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, packet.Type, 0, 0));
                    Session.Character.DefaultTimer = 120;
                    Session.Character.ArenaDisposable?.Dispose();
                    break;

                case 27:
                    Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(2, 27, 0, 0));
                    var objet = ServerManager.Instance.KingOfTheHillMembers.FirstOrDefault(s => s.Session.Character.Name == Session.Character.Name);
                    if (objet == null) return;
                    ServerManager.Instance.KingOfTheHillMembers.Remove(objet);
                    break;
            }
        }
    }
}
