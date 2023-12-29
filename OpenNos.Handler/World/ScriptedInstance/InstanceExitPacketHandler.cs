using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class InstanceExitPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public InstanceExitPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// rxitPacket packet
        /// </summary>
        /// <param name="rxitPacket"></param>
        public void InstanceExit(RaidExitPacket rxitPacket)
        {
            if (rxitPacket?.State == 1)
            {
                if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.TimeSpaceInstance && Session.Character.Timespace != null || Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.SkyTowerInstance && Session.Character.SkyTower != null)
                {
                    if (Session.CurrentMapInstance.InstanceBag.Lock)
                    {
                        //5seed
                        if (Session.Character.Inventory.CountItem(1012) >= 5)
                        {
                            Session.CurrentMapInstance.InstanceBag.DeadList.Add(Session.Character.CharacterId);
                            Session.Character.Dignity -= 20;
                            if (Session.Character.Dignity < -1000)
                            {
                                Session.Character.Dignity = -1000;
                            }
                            Session.SendPacket(
                                Session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("DIGNITY_LOST"), 20), 11));
                            Session.Character.Inventory.RemoveItemAmount(1012, 5);
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                string.Format(Language.Instance.GetMessageFromKey("NO_ITEM_REQUIRED"),
                                    ServerManager.GetItem(1012).Name), 0));
                            return;
                        }
                    }
                    else
                    {
                        //1seed
                    }
                    ServerManager.Instance.TimespaceSessions.Remove(Session);
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, Session.Character.MapId, Session.Character.MapX, Session.Character.MapY);
                }
                if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.RaidInstance)
                {
                    if (Session.CurrentMapInstance.InstanceBag.Lock)
                    {
                        //5seed
                        if (Session.Character.Inventory.CountItem(1012) >= 5)
                        {
                            Session.CurrentMapInstance.InstanceBag.DeadList.Add(Session.Character.CharacterId);
                            Session.Character.Dignity -= 20;
                            if (Session.Character.Dignity < -1000)
                            {
                                Session.Character.Dignity = -1000;
                            }
                            Session.SendPacket(
                                Session.Character.GenerateSay(
                                    string.Format(Language.Instance.GetMessageFromKey("DIGNITY_LOST"), 20), 11));
                            Session.Character.Inventory.RemoveItemAmount(1012, 5);
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                string.Format(Language.Instance.GetMessageFromKey("NO_ITEM_REQUIRED"),
                                    ServerManager.GetItem(1012).Name), 0));
                            return;
                        }
                    }
                    else
                    {
                        //1seed
                    }

                    if (Session.Account.Authority < AuthorityType.GM)
                    {
                        return;
                    }

                    ServerManager.Instance.GroupLeave(Session);
                }
                else if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                {
                    Session.Character.LeaveTalentArena(true);
                    var mapInstance = ServerManager.GetMapInstanceByMapId(Session.Character.LastMapId);
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, mapInstance.MapInstanceId, Session.Character.LastMapX, Session.Character.LastMapY);
                }
            }
        }
    }
}
