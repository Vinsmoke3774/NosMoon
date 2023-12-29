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
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.FactionEgg)]
    public class FactionEggHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public FactionEggHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            const short baseVnum = 1623;
            if (ServerManager.Instance.ChannelId == 51)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED_ACT4"),
                        0));
                return;
            }
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4ShipAngel
                || Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4ShipDemon)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED_ACT4SHIP"),
                        0));
                return;
            }
            if (Enum.TryParse(packet.Argument.ToString(), out FactionType faction)
            && Session.Character.Inventory.CountItem(baseVnum + (byte)faction) > 0)
            {
                if ((byte)faction < 3) // Single family change
                {
                    if (Session.Character.Faction == (FactionType)faction)
                    {
                        return;
                    }
                    if (Session.Character.Family != null)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("IN_FAMILY"),
                                0));
                        return;
                    }
                    Session.Character.Inventory.RemoveItemAmount(baseVnum + (byte)faction);
                    Session.Character.ChangeFaction((FactionType)faction);
                }
                else // Family faction change
                {
                    faction -= 2;
                    if ((FactionType)Session.Character.Family.FamilyFaction == faction)
                    {
                        return;
                    }
                    if (Session.Character.Family == null)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAMILY"),
                                0));
                        return;
                    }
                    if (Session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAMILY_HEAD"),
                                0));
                        return;
                    }
                    if (Session.Character.Family.LastFactionChange > DateTime.Now.AddDays(-1).Ticks)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                            Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED"), 0));
                        return;
                    }

                    Session.Character.Inventory.RemoveItemAmount(baseVnum + (byte)faction + 2);
                    Session.Character.Family.ChangeFaction((byte)faction, Session);
                }
            }
        }
    }
}
