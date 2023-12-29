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
    [NRunHandler(NRunType.MysteryBox)]
    public class MysteryBoxHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public MysteryBoxHandler(ClientSession session) : base(session)
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

            if (10000000 > Session.Character.Gold)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 11));
                return;
            }

            var rewards = ServerManager.Instance.MimicItems;
            var random = ServerManager.RandomDouble();

            var rewardsPercentage = rewards.GroupBy(s => s.Percentage).OrderBy(s => s.Key);

            MimicRotationDTO selectedReward = null;
            foreach (var grouping in rewardsPercentage)
            {
                if (random > grouping.Key)
                {
                    continue;
                }

                selectedReward = grouping.ElementAt(ServerManager.RandomNumber(0, grouping.Count()));
                if (selectedReward.IsSuperReward)
                {
                    ServerManager.Shout($"{Session.Character.Name} just won {selectedReward.ItemAmount}x {ServerManager.GetItem((short)selectedReward.ItemVnum).Name} from the Mystery Box!");
                }

                break;
            }

            if (selectedReward == null)
            {
                var items = rewards.Where(s => s.Percentage == 100);
                selectedReward = rewards[new Random().Next(0, items.Count())];
            }

            Session.Character.GiftAdd((short)selectedReward.ItemVnum, selectedReward.ItemAmount);
            Session.Character.RemoveGold(10000000, true);
        }
    }
}
