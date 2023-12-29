using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Domain;

namespace OpenNos.Core.Actions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GuriHandlerAttribute : Attribute
    {
        public GuriHandlerAttribute(GuriType[] guriTypes)
        {
            GuriTypes = guriTypes;
        }

        public GuriHandlerAttribute(GuriType guriType)
        {
            GuriTypes = new[]
            {
                guriType
            };
        }

        public GuriType[] GuriTypes { get; set; }
    }

}
