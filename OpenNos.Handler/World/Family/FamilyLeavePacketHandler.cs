using NosByte.Packets.ClientPackets.Family;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.Family
{
    public class FamilyLeavePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyLeavePacketHandler(ClientSession session) => Session = session;

        public void FamilyLeave(FamilyLeavePacket packet)
        {
            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null)
            {
                return;
            }

            if (Session.CurrentMapInstance.MapInstanceType.Equals(MapInstanceType.LodInstance))
            {
                ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.Character.MapInstanceId, Session.Character.PositionX, Session.Character.PositionY, true);
            }

            if (Session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANNOT_LEAVE_FAMILY")));
                return;
            }

            long familyId = Session.Character.Family.FamilyId;

            DAOFactory.FamilyCharacterDAO.Delete(Session.Character.CharacterId);

            Logger.Log.LogUserEvent("GUILDCOMMAND", Session.GenerateIdentity(),
                $"[FamilyLeave][{Session.Character.Family.FamilyId}]");
            Logger.Log.LogUserEvent("GUILDLEAVE", Session.GenerateIdentity(),
                $"[FamilyLeave][{Session.Character.Family.FamilyId}]");

            Session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, Session.Character.Name);
            Session.Character.LastFamilyLeave = DateTime.Now.Ticks;
            Session.Character.Family = null;
            Observable.Timer(TimeSpan.FromSeconds(5)).SafeSubscribe(o =>
            {
                if (Session == null)
                {
                    return;
                }

                ServerManager.Instance.FamilyRefresh(familyId);
            });
            Observable.Timer(TimeSpan.FromSeconds(10)).SafeSubscribe(o =>
            {
                if (Session == null)
                {
                    return;
                }

                ServerManager.Instance.FamilyRefresh(familyId);
            });
            ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.Character.MapInstanceId, Session.Character.PositionX, Session.Character.PositionY, true);
            Session.SendPacket(StaticPacketHelper.Cancel(2));
        }
    }
}
