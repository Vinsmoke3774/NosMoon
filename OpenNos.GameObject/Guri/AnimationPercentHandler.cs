using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.DanceAnimationPercent)]
    public class AnimationPercentHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public AnimationPercentHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            
        }

        public void Execute(GuriPacket packet)
        {
            
        }
    }
}
