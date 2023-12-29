using NosByte.Packets.ClientPackets.Family;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using System;
using System.Linq;

namespace OpenNos.Handler.World.Family
{
    public class CmdTodayMessagePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CmdTodayMessagePacketHandler(ClientSession session) => Session = session;

        public void TodayMessage(TodayMessagePacket packet)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null)
            {
                var msg = packet.Data;

                Logger.Log.LogUserEvent("GUILDCOMMAND", Session.GenerateIdentity(),
                    $"[Today][{Session.Character.Family.FamilyId}]CharacterName: {Session.Character.Name} Title: {msg}");

                bool islog = Session.Character.Family.FamilyLogs.Any(s =>
                    s.FamilyLogType == FamilyLogType.DailyMessage
                    && s.FamilyLogData.StartsWith(Session.Character.Name, StringComparison.CurrentCulture)
                    && s.Timestamp.AddDays(1) > DateTime.Now);
                if (!islog)
                {
                    Session.Character.FamilyCharacter.DailyMessage = msg;
                    FamilyCharacterDTO fchar = Session.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref fchar);
                    Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                    Session.Character.Family.InsertFamilyLog(FamilyLogType.DailyMessage, Session.Character.Name,
                        message: msg);
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_CHANGE_MESSAGE")));
                }
            }
        }
    }
}
