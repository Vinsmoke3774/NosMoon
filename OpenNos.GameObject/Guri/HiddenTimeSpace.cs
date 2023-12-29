using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.SearchHiddenTimeSpace)]
    public class HiddenTimeSpaceHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        #region Instantiation

        public HiddenTimeSpaceHandler(ClientSession session) : base(session)
        {
        }

        #endregion

        #region Methods

        public void Execute(GuriPacket packet)
        {
            short dowsingRod = 2155;

            if (Session.Character.Inventory.CountItem(dowsingRod) < 0)
            {
                return;
            }

            Session.Character.CanCreateTimeSpace = false;

            var energyField = Session.Character.EnergyFields.FirstOrDefault(s => s.MapId == Session.Character.MapId);
            Session.Character.GenerateEff(820);

            Session.Character.Inventory.RemoveItemAmount(dowsingRod);

            if (energyField == null || !Session.Character.IsInRange(energyField.MapX, energyField.MapY, 30))
            {
                Session.SendPacket(Session.Character.GenerateEff(6009));
                Session.SendPacket(UserInterfaceHelper.GenerateSay("Cannot receive any signal because it's too far away.", 10));
                return;
            }

            if (Session.Character.IsInRange(energyField.MapX, energyField.MapY, 5))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateSay("You have discovered an energy field.", 10));
                Session.SendPacket(UserInterfaceHelper.GenerateMsg("You have discovered an energy field.", 0));
                Session.SendPacket(Session.Character.GenerateEff(6008));
                Session.SendPacket($"eff_g 821 9996 {energyField.MapX} {energyField.MapY} 0");
                Session.Character.CanCreateTimeSpace = true;
                return;
            }

            Session.SendPacket(Session.Character.GenerateHidn(energyField.MapX, energyField.MapY));
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        #endregion
    }
}