using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;

namespace OpenNos.GameObject.ActionHandles
{
    public interface IGenericHandler<in T> where T : PacketDefinition
    {
        /// <summary>
        /// Verifies the integrity of the packet
        /// </summary>
        /// <param name="packet"></param>
        void ValidateData(T packet);

        /// <summary>
        /// Executes the logic for the provided packet
        /// </summary>
        /// <param name="packet"></param>
        void Execute(T packet);

    }
}
