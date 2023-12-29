using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.ScriptedInstance
{
    [NRunHandler(new [] { NRunType.FinishedTs, NRunType.FinishedTS2 } )]
    public class TimeSpaceEndHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TimeSpaceEndHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (Session.Character.Timespace == null || Session.Character.MapInstance.InstanceBag.EndState != 10)
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            EventHelper.Instance.RunEvent(new EventContainer(Session.Character.MapInstance, EventActionType.SCRIPTEND, (byte)5));
        }
    }
}
