using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.AddTitle)]
    public class AddTitleHandler : SpecialHandlerBase,IGenericHandler<GuriPacket>
    {
        public AddTitleHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (Session.Character.Inventory.CountItem(packet.Argument) < 1)
            {
                return;
            }

            var item = ServerManager.GetItem((short)packet.Argument);

            if (item == null)
            {
                return;
            }

            if (item.ItemType != ItemType.Title)
            {
                return;
            }

            Session.Character.Title.Add(new CharacterTitleDTO
            {
                CharacterId = Session.Character.CharacterId,
                Stat = 1,
                TitleVnum = packet.Argument
            });

            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEW_TITLE"), 0));

            var conqueror = Session.Character.Title.FirstOrDefault(s => s.TitleVnum.Equals(9388));

            if (conqueror == null)
            {
                var allRaidTitles = Session.Character.Title.Where(s => s.TitleVnum >= 9384 && s.TitleVnum <= 9387);

                if (allRaidTitles.Count() == 4)
                {
                    Session.Character.GiftAdd(9388, 1);
                }
            }

            Session.Character.Inventory.RemoveItemAmount(packet.Argument);
            Session.SendPacket(Session.Character.GenerateTitle());
        }
    }
}
