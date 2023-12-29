using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Linq;

namespace OpenNos.Handler.World.Basic
{
    public class WhisperPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public WhisperPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// / packet
        /// </summary>
        /// <param name="whisperPacket"></param>
        public void Whisper(WhisperPacket whisperPacket)
        {
            string[] filter = new string[] { "nig", "fuck", "bitch", "ass", "bastard", "cunt", "milf", "shit", "fag" };
            try
            {
                // TODO: Implement WhisperSupport
                if (string.IsNullOrEmpty(whisperPacket.Message))
                {
                    return;
                }

                if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RainbowBattleInstance ||
                    Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.CaligorInstance)
                {
                    Session.SendPacket(Session.Character.GenerateSay("You cannot speak to your friends on this map.", 12));
                    return;
                }

                string characterName =
                    whisperPacket.Message.Split(' ')[
                            whisperPacket.Message.StartsWith("GM ", StringComparison.CurrentCulture) ? 1 : 0].Replace("[Angel]", "").Replace("[Demon]", "");

                Enum.GetNames(typeof(AuthorityType)).ToList().ForEach(at => characterName = characterName.Replace($"[{at}]", ""));

                string message = "";
                string[] packetsplit = whisperPacket.Message.Split(' ');
                for (int i = packetsplit[0] == "GM" ? 2 : 1; i < packetsplit.Length; i++)
                {
                    if (filter.Contains(packetsplit[i])) message += "$@_éè&à$^ù" + " ";
                    else message += packetsplit[i] + " ";
                }

                if (message.Length > 60)
                {
                    message = message.Substring(0, 60);
                }

                message = message.Trim();
                Session.SendPacket(Session.Character.GenerateSpk(message, 5));
                CharacterDTO receiver = DAOFactory.CharacterDAO.LoadByName(characterName);

                int? sentChannelId = null;
                if (receiver != null)
                {
                    //ServerManager.Instance.ChatLogs.Add(new ChatLogDTO
                    //{
                    //    CharacterId = Session.Character.CharacterId,
                    //    CharacterName = Session.Character.Name,
                    //    DateTime = DateTime.Now,
                    //    DestinationCharacterId = receiver.CharacterId,
                    //    DestinationCharacterName = receiver.Name,
                    //    MessageType = "Whisper",
                    //    Message = message
                    //});

                    if (receiver.CharacterId == Session.Character.CharacterId)
                    {
                        return;
                    }

                    if (Session.Character.IsBlockedByCharacter(receiver.CharacterId))
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                        return;
                    }

                    var targetName = string.Empty;
                    ClientSession receiverSession =
                        ServerManager.Instance.GetSessionByCharacterId(receiver.CharacterId);
                    if (receiverSession?.CurrentMapInstance?.Map.MapId == Session.CurrentMapInstance?.Map.MapId
                        && Session.Account.Authority >= AuthorityType.TMOD)
                    {
                        receiverSession?.SendPacket(Session.Character.GenerateSay(message, 2));
                    }

                    sentChannelId = CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                    {
                        DestinationCharacterId = receiver.CharacterId,
                        SourceCharacterId = Session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = Session.Character.Authority >= AuthorityType.TGS
                            ? Session.Character.GenerateSay(
                                $"(whisper)(From {Session.Character.Authority} {Session.Character.Name}):{message}", 11)
                            : Session.Character.GenerateSpk(message,
                                Session.Account.Authority >= AuthorityType.TMOD ? 15 : 5),
                        Type = Enum.GetNames(typeof(AuthorityType)).Any(a =>
                        {
                            if (a.Equals(packetsplit[0]))
                            {
                                Enum.TryParse(a, out AuthorityType auth);
                                if (auth >= AuthorityType.TMOD)
                                {
                                    return true;
                                }
                            }
                            return false;
                        })
                        || Session.Account.Authority >= AuthorityType.TMOD
                        ? MessageType.WhisperGM : MessageType.Whisper
                    });
                }

                if (sentChannelId == null)
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED")));
                }
                else
                {
                    
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Whisper failed.", e);
            }
        }
    }
}
