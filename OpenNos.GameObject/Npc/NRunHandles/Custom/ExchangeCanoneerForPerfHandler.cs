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
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.ExchangeCanonneerVsPerf)]
    public class ExchangeCanoneerForPerfHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeCanoneerForPerfHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (Session.Character.Inventory.CountItemInInventory(4501, 0) <= 0)
            {
                // No SP
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_SP_TO_TRADE"), 11));
                return;
            }

            if (Session.Character.Inventory.CountItemInInventory(4501, 0) >= 1)
            {
                Session.Character.GiftAdd(2518, (byte)ServerManager.RandomNumber(2, 5));
                Session.Character.Inventory.RemoveItemAmount(4501, 1);
            }
        }
    }
}
