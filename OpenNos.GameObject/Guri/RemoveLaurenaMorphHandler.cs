using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.RemoveLaurenaMorph)]
    public class RemoveLaurenaMorphHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public RemoveLaurenaMorphHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (Session?.Character?.MapInstance == null)
            {
                return;
            }

            if (Session.Character.IsLaurenaMorph())
            {
                Session.Character.MapInstance.Broadcast(Session.Character.GenerateEff(4054));
                Session.Character.ClearLaurena();
            }
        }
    }
}
