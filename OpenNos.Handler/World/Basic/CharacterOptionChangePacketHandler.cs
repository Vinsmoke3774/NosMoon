﻿using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class CharacterOptionChangePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CharacterOptionChangePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// gop packet
        /// </summary>
        /// <param name="characterOptionPacket"></param>
        public void CharacterOptionChange(CharacterOptionPacket characterOptionPacket)
        {
            if (Session.Character == null)
            {
                return;
            }

            switch (characterOptionPacket.Option)
            {
                case CharacterOption.BuffBlocked:
                    Session.Character.BuffBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.BuffBlocked
                            ? "BUFF_BLOCKED"
                            : "BUFF_UNLOCKED"), 0));
                    break;

                case CharacterOption.EmoticonsBlocked:
                    Session.Character.EmoticonsBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.EmoticonsBlocked
                            ? "EMO_BLOCKED"
                            : "EMO_UNLOCKED"), 0));
                    break;

                case CharacterOption.ExchangeBlocked:
                    Session.Character.ExchangeBlocked = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.ExchangeBlocked
                            ? "EXCHANGE_BLOCKED"
                            : "EXCHANGE_UNLOCKED"), 0));
                    break;

                case CharacterOption.FriendRequestBlocked:
                    Session.Character.FriendRequestBlocked = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.FriendRequestBlocked
                            ? "FRIEND_REQ_BLOCKED"
                            : "FRIEND_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupRequestBlocked:
                    Session.Character.GroupRequestBlocked = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.GroupRequestBlocked
                            ? "GROUP_REQ_BLOCKED"
                            : "GROUP_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.PetAutoRelive:
                    Session.Character.IsPetAutoRelive = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.IsPetAutoRelive
                        ? "PET_AUTO_RELIVE_ENABLED"
                        : "PET_AUTO_RELIVE_DISABLED"), 0));
                    break;

                case CharacterOption.PartnerAutoRelive:
                    Session.Character.IsPartnerAutoRelive = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.IsPartnerAutoRelive
                        ? "PARTNER_AUTO_RELIVE_ENABLED"
                        : "PARTNER_AUTO_RELIVE_DISABLED"), 0));
                    break;

                case CharacterOption.HeroChatBlocked:
                    Session.Character.HeroChatBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.HeroChatBlocked
                            ? "HERO_CHAT_BLOCKED"
                            : "HERO_CHAT_UNLOCKED"), 0));
                    break;

                case CharacterOption.HpBlocked:
                    Session.Character.HpBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.HpBlocked ? "HP_BLOCKED" : "HP_UNLOCKED"),
                        0));
                    break;

                case CharacterOption.MinilandInviteBlocked:
                    Session.Character.MinilandInviteBlocked = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.MinilandInviteBlocked
                            ? "MINI_INV_BLOCKED"
                            : "MINI_INV_UNLOCKED"), 0));
                    break;

                case CharacterOption.MouseAimLock:
                    Session.Character.MouseAimLock = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.MouseAimLock
                            ? "MOUSE_LOCKED"
                            : "MOUSE_UNLOCKED"), 0));
                    break;

                case CharacterOption.QuickGetUp:
                    Session.Character.QuickGetUp = characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.QuickGetUp
                            ? "QUICK_GET_UP_ENABLED"
                            : "QUICK_GET_UP_DISABLED"), 0));
                    break;

                case CharacterOption.WhisperBlocked:
                    Session.Character.WhisperBlocked = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.WhisperBlocked
                            ? "WHISPER_BLOCKED"
                            : "WHISPER_UNLOCKED"), 0));
                    break;

                case CharacterOption.FamilyRequestBlocked:
                    Session.Character.FamilyRequestBlocked = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.FamilyRequestBlocked
                            ? "FAMILY_REQ_LOCKED"
                            : "FAMILY_REQ_UNLOCKED"), 0));
                    break;

                case CharacterOption.HatInVisible:
                    Session.Character.HatInVisible = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.HatInVisible
                            ? "HAT_INVISIBLE"
                            : "HAT_VISIBLE"), 0));
                        Session?.CurrentMapInstance?.Broadcast(Session.Character.GenerateEq());
                    break;
                case CharacterOption.LockInterface:
                    Session.Character.LockInterface = !characterOptionPacket.IsActive;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey(Session.Character.LockInterface
                            ? "INTERFACE_LOCKED"
                            : "INTERFACE_UNLOCKED"), 0));
                    break;

                case CharacterOption.GroupSharing:
                    Group grp = ServerManager.Instance.Groups.Find(
                        g => g != null && g.IsMemberOfGroup(Session.Character.CharacterId));
                    if (grp == null)
                    {
                        return;
                    }

                    if (!grp.IsLeader(Session))
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_MASTER"), 0));
                        return;
                    }

                    if (!characterOptionPacket.IsActive)
                    {
                        grp.SharingMode = 1;

                        Session.CurrentMapInstance?.Broadcast(Session,
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING"), 0),
                            ReceiverType.Group);
                    }
                    else
                    {
                        grp.SharingMode = 0;

                        Session.CurrentMapInstance?.Broadcast(Session,
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHARING_BY_ORDER"), 0),
                            ReceiverType.Group);
                    }
                    break;
            }

            Session.SendPacket(Session.Character.GenerateStat());
        }
    }
}
