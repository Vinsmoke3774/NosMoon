using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.ActVRelic)]
    public class ActVRelicHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public ActVRelicHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            short relictVNum = 0;
            if (packet.Argument == 10000)
            {
                relictVNum = 1878;
            }
            else if (packet.Argument == 30000)
            {
                relictVNum = 1879;
            }
            if (relictVNum > 0 && Session.Character.Inventory.CountItem(relictVNum) > 0)
            {
                IEnumerable<RollGeneratedItemDTO> roll = DAOFactory.RollGeneratedItemDAO.LoadByItemVNum(relictVNum);
                IEnumerable<RollGeneratedItemDTO> rollGeneratedItemDtos = roll as IList<RollGeneratedItemDTO> ?? roll.ToList();
                if (!rollGeneratedItemDtos.Any())
                {
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_RELICT"), GetType(), relictVNum));
                    return;
                }
                int probabilities = rollGeneratedItemDtos.Sum(s => s.Probability);
                int rnd = 0;
                int rnd2 = ServerManager.RandomNumber(0, probabilities);
                int currentrnd = 0;
                foreach (RollGeneratedItemDTO rollitem in rollGeneratedItemDtos.OrderBy(s => ServerManager.RandomNumber()))
                {
                    sbyte rare = 0;
                    if (rollitem.IsRareRandom)
                    {
                        rnd = ServerManager.RandomNumber(0, 100);

                        for (int j = 7; j >= 0; j--)
                        {
                            if (rnd < ItemHelper.RareRate[j])
                            {
                                rare = (sbyte)j;
                                break;
                            }
                        }
                        if (rare < 1)
                        {
                            rare = 1;
                        }
                    }

                    if (rollitem.Probability == 10000)
                    {
                        Session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount, (byte)rare, design: rollitem.ItemGeneratedDesign);
                        continue;
                    }
                    currentrnd += rollitem.Probability;
                    if (currentrnd < rnd2)
                    {
                        continue;
                    }
                    Session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount, (byte)rare, design: rollitem.ItemGeneratedDesign);//, rollitem.ItemGeneratedUpgrade);
                    break;
                }
                Session.Character.Inventory.RemoveItemAmount(relictVNum);
                Session.Character.Gold -= packet.Argument;
                Session.Character.GenerateGold();
                Session.SendPacket("shop_end 1");
            }
        }
    }
}
