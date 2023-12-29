using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.Family
{
    [NRunHandler(NRunType.FamilyStartOrDisband)]
    public class StartOrDisbandFamilyHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public StartOrDisbandFamilyHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (packet.Type == 0)
            {
                if (Session.Character.Group?.SessionCount == 3)
                {
                    foreach (ClientSession s in Session.Character.Group.Sessions.GetAllItems())
                    {
                        if (s.Character.Family != null)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_MEMBER_ALREADY_IN_FAMILY")));
                            return;
                        }
                    }
                }
                if (Session.Character.Group == null || Session.Character.Group.SessionCount != 3)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
                    return;
                }
                Session.SendPacket(UserInterfaceHelper.GenerateInbox($"#glmk^ {14} 1 {Language.Instance.GetMessageFromKey("CREATE_FAMILY").Replace(' ', '^')}"));
            }
            else
            {
                if (Session.Character.Family == null)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_IN_FAMILY")));
                    return;
                }
                if (Session.Character.Family != null && Session.Character.FamilyCharacter != null && Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                    return;
                }
                Session.SendPacket($"qna #glrm^1 {Language.Instance.GetMessageFromKey("DISSOLVE_FAMILY")}");
            }
        }
    }
}
