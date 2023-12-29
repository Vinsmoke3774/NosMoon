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
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.MysteryBoxInfo)]
    public class MysteryBoxInfoHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public MysteryBoxInfoHandler(ClientSession session) : base(session)
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
            if (ServerManager.Instance.MimicItems == null || !ServerManager.Instance.MimicItems.Any())
            {
                ServerManager.Instance.LoadMimicRewards();
            }

            var rewards = ServerManager.Instance.MimicItems;
            Session.SendPacket(Session.Character.GenerateSay("----------[REWARDS OF THE WEEK]---------- \n", 12));
            foreach (var reward in rewards)
            {
                var item = ServerManager.GetItem((short)reward.ItemVnum);
                Session.SendPacket(Session.Character.GenerateSay($"{(reward.IsSuperReward ? "SuperReward : " : "")}" + item.Name + $" x{reward.ItemAmount}" + "\n", 11));
            }
            Session.SendPacket(Session.Character.GenerateSay("----------[END]---------- \n", 12));
        }
    }
}
