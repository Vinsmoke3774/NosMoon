using System;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.Basic
{
    public class SendMailPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        public SendMailPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// pst packet
        /// </summary>
        /// <param name="pstPacket"></param>
        public void SendMail(PstPacket pstPacket)
        {
            if (pstPacket?.Data != null)
            {
                CharacterDTO receiver = DAOFactory.CharacterDAO.LoadByName(pstPacket.Receiver);
                if (receiver != null)
                {
                    string[] datasplit = pstPacket.Data.Split(' ');
                    if (datasplit.Length < 2)
                    {
                        return;
                    }

                    if (datasplit[1].Length > 250)
                    {
                        return;
                    }

                    //ServerManager.Instance.ChatLogs.Add(new ChatLogDTO
                    //{
                    //    CharacterId = Session.Character.CharacterId,
                    //    CharacterName = Session.Character.Name,
                    //    DateTime = DateTime.Now,
                    //    DestinationCharacterId = receiver.CharacterId,
                    //    DestinationCharacterName = receiver.Name,
                    //    Message = datasplit[1],
                    //    MessageType = "Note",
                    //    NoteTitle = datasplit[0]
                    //});

                    ItemInstance headWearable =
                        Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);
                    byte color = headWearable?.Item.IsColored == true
                        ? (byte)headWearable.Design
                        : (byte)Session.Character.HairColor;
                    MailDTO mailcopy = new MailDTO
                    {
                        AttachmentAmount = 0,
                        IsOpened = false,
                        Date = DateTime.Now,
                        Title = datasplit[0],
                        Message = datasplit[1],
                        ReceiverId = receiver.CharacterId,
                        SenderId = Session.Character.CharacterId,
                        IsSenderCopy = true,
                        SenderClass = Session.Character.Class,
                        SenderGender = Session.Character.Gender,
                        SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                        SenderHairStyle = Session.Character.HairStyle,
                        EqPacket = Session.Character.GenerateEqListForPacket(),
                        SenderMorphId = Session.Character.Morph == 0
                            ? (short)-1
                            : (short)(Session.Character.Morph > short.MaxValue ? 0 : Session.Character.Morph)
                    };
                    MailDTO mail = new MailDTO
                    {
                        AttachmentAmount = 0,
                        IsOpened = false,
                        Date = DateTime.Now,
                        Title = datasplit[0],
                        Message = datasplit[1],
                        ReceiverId = receiver.CharacterId,
                        SenderId = Session.Character.CharacterId,
                        IsSenderCopy = false,
                        SenderClass = Session.Character.Class,
                        SenderGender = Session.Character.Gender,
                        SenderHairColor = Enum.IsDefined(typeof(HairColorType), color) ? (HairColorType)color : 0,
                        SenderHairStyle = Session.Character.HairStyle,
                        EqPacket = Session.Character.GenerateEqListForPacket(),
                        SenderMorphId = Session.Character.Morph == 0
                            ? (short)-1
                            : (short)(Session.Character.Morph > short.MaxValue ? 0 : Session.Character.Morph)
                    };

                    MailServiceClient.Instance.SendMail(mailcopy);
                    MailServiceClient.Instance.SendMail(mail);

                    //Session.Character.MailList.Add((Session.Character.MailList.Count > 0 ? Session.Character.MailList.OrderBy(s => s.Key).Last().Key : 0) + 1, mailcopy);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAILED"),
                        11));

                    //Session.SendPacket(Session.Character.GeneratePost(mailcopy, 2));
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else if (pstPacket != null && int.TryParse(pstPacket.Id.ToString(), out int id)
                     && byte.TryParse(pstPacket.Type.ToString(), out byte type))
            {
                if (pstPacket.Argument == 3)
                {
                    if (Session.Character.MailList.ContainsKey(id))
                    {
                        if (!Session.Character.MailList[id].IsOpened)
                        {
                            Session.Character.MailList[id].IsOpened = true;
                            MailDTO mailupdate = Session.Character.MailList[id];
                            DAOFactory.MailDAO.InsertOrUpdate(ref mailupdate);
                        }

                        Session.SendPacket(Session.Character.GeneratePostMessage(Session.Character.MailList[id], type));
                    }
                }
                else if (pstPacket.Argument == 2)
                {
                    if (Session.Character.MailList.ContainsKey(id))
                    {
                        MailDTO mail = Session.Character.MailList[id];
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAIL_DELETED"), 11));
                        Session.SendPacket($"post 2 {type} {id}");
                        if (DAOFactory.MailDAO.LoadById(mail.MailId) != null)
                        {
                            DAOFactory.MailDAO.DeleteById(mail.MailId);
                        }

                        if (Session.Character.MailList.ContainsKey(id))
                        {
                            Session.Character.MailList.TryRemove(id, out _);
                        }
                    }
                }
            }
        }
    }
}
