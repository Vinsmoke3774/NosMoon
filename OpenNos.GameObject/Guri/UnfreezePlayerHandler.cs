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
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.UnfreezePlayer)]
    public class UnfreezePlayerHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public UnfreezePlayerHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            return; //not needed anymore/right now
            long? targetId = packet.User;

            if (targetId == null)
            {
                return;
            }

            ClientSession target = ServerManager.Instance.GetSessionByCharacterId(targetId.Value);

            if (target?.Character?.MapInstance == null)
            {
                return;
            }

            if (target.Character.HasBuff(633))
            {
                target?.Character?.RemoveBuff(569);
            }

            else
            {
                target.Character.RemoveBuff(569);
            }
        }
    }
}
