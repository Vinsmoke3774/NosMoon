using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.Family
{
    public class FamilyDismissPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyDismissPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// glrm packet
        /// </summary>
        /// <param name="familyDissmissPacket"></param>
        public void FamilyDismiss(FamilyDismissPacket familyDissmissPacket)
        {
            if (Session.Character.Family == null || Session.Character.FamilyCharacter == null
                                                 || Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
            {
                return;
            }

            GameObject.Family fam = Session.Character.Family;

            fam.FamilyCharacters.ForEach(s =>
            {
                var chara =  DAOFactory.CharacterDAO.LoadById(s.Character.CharacterId);

                chara.LastFamilyLeave = DateTime.Now.Ticks;
                DAOFactory.CharacterDAO.InsertOrUpdate(ref chara);

                DAOFactory.FamilyCharacterDAO.Delete(s.Character.CharacterId);
            });
            fam.FamilyLogs.ForEach(s => DAOFactory.FamilyLogDAO.Delete(s.FamilyLogId));
            DAOFactory.FamilyDAO.Delete(fam.FamilyId);
            ServerManager.Instance.FamilyRefresh(fam.FamilyId);

            Logger.Log.LogUserEvent("GUILDDISMISS", Session.GenerateIdentity(), $"[FamilyDismiss][{fam.FamilyId}]");

            List<ClientSession> sessions = ServerManager.Instance.Sessions
                .Where(s => s.Character?.Family != null && s.Character.Family.FamilyId == fam.FamilyId).ToList();

            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = fam.FamilyId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });

            Observable.Timer(TimeSpan.FromSeconds(3)).SafeSubscribe(observer =>
            {
                sessions.ForEach(s =>
                {
                    if (s?.Character != null)
                    {
                        s.CurrentMapInstance?.Broadcast(s.Character.GenerateGidx());
                    }
                });
            });
        }
    }
}
