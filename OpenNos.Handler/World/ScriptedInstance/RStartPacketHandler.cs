using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.Handler.World.Basic;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class RStartPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RStartPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// RstartPacket packet
        /// </summary>
        /// <param name="rStartPacket"></param>
        public void GetRStart(RStartPacket rStartPacket)
        {
            if (Session.Character.Timespace != null)
            {
                if (rStartPacket.Type == 1 && Session.Character.Timespace.InstanceBag != null && Session.Character.Timespace.InstanceBag.Lock == false)
                {
                    if (Session.Character.Timespace.SpNeeded?[(byte)Session.Character.Class] != 0)
                    {
                        ItemInstance specialist = Session.Character.Inventory?.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                        if (specialist == null || specialist.ItemVNum != Session.Character.Timespace.SpNeeded?[(byte)Session.Character.Class])
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TS_SP_NEEDED"), 0));
                            return;
                        }
                    }
                    Session.Character.Timespace.InstanceBag.Lock = true;
                    new PreqPacketHandler(Session).Preq(new PreqPacket());
                    Session.Character.Timespace._mapInstanceDictionary.ToList().SelectMany(s => s.Value.Sessions).Where(s => s.Character?.Timespace != null).ToList().ForEach(s =>
                    {
                        s.Character.GeneralLogs.Add(new GeneralLogDTO
                        {
                            AccountId = s.Account.AccountId,
                            CharacterId = s.Character.CharacterId,
                            IpAddress = s.CleanIpAddress,
                            LogData = s.Character.Timespace.Id.ToString(),
                            LogType = "InstanceEntry",
                            Timestamp = DateTime.Now
                        });
                    });
                }
            }
        }
    }
}
