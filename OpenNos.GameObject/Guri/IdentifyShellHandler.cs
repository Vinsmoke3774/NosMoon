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
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.IdentifyShell)]
    public class IdentifyShellHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public IdentifyShellHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (packet.Argument == 0 && short.TryParse(packet.User.ToString(), out short slot))
            {
                ItemInstance shell =
                    Session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                if (shell?.ShellEffects.Count == 0 && shell.Upgrade > 0 && shell.Rare > 0
                    && Session.Character.Inventory.CountItem(1429) >= ((shell.Upgrade / 10) + shell.Rare))
                {
                    if (!ShellGeneratorHelper.Instance.ShellTypes.TryGetValue(shell.ItemVNum, out var shellType))
                    {
                        // SHELL TYPE NOT IMPLEMENTED
                        return;
                    }

                    List<ShellEffectDTO> shellOptions = ShellGeneratorHelper.Instance.GenerateShell(shellType, shell.Rare == 8 ? 7 : shell.Rare, shell.Item.IsHeroic ? 106 : shell.Upgrade);

                    shell.ShellEffects.AddRange(shellOptions);

                    DAOFactory.ShellEffectDAO.InsertOrUpdateFromList(shell.ShellEffects, shell.EquipmentSerialId);

                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("OPTION_IDENTIFIED"), 0));
                    Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 3006));
                    Session.Character.Inventory.RemoveItemAmount(1429, (shell.Upgrade / 10) + shell.Rare);
                }
            }
        }
    }
}
