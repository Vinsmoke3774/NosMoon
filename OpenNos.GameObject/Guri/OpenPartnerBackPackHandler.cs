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
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.OpenPartnerBackPack)]
    public class OpenPartnerBackPackHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public OpenPartnerBackPackHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PARTNER_BACKPACK"), 10));
            Session.SendPacket(Session.Character.GeneratePStashAll());
        }
    }
}
