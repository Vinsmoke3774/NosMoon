using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class SpTransformPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SpTransformPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// sl packet
        /// </summary>
        /// <param name="spTransformPacket"></param>
        public void SpTransform(SpTransformPacket spTransformPacket)
        {
            if (spTransformPacket != null && !Session.Character.IsSeal && !Session.Character.IsMorphed)
            {
                ItemInstance specialistInstance =
                    Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);

                if (spTransformPacket.Type == 10)
                {
                    short specialistDamage = spTransformPacket.SpecialistDamage,
                        specialistDefense = spTransformPacket.SpecialistDefense,
                        specialistElement = spTransformPacket.SpecialistElement,
                        specialistHealpoints = spTransformPacket.SpecialistHP;
                    int transportId = spTransformPacket.TransportId;
                    if (!Session.Character.UseSp || specialistInstance == null
                        || transportId != specialistInstance.TransportId)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SPUSE_NEEDED"), 0));
                        return;
                    }

                    if (CharacterHelper.SPPoint(specialistInstance.SpLevel, specialistInstance.Upgrade)
                        - specialistInstance.SlDamage - specialistInstance.SlHP - specialistInstance.SlElement
                        - specialistInstance.SlDefence - specialistDamage - specialistDefense - specialistElement
                        - specialistHealpoints < 0)
                    {
                        return;
                    }

                    if (specialistDamage < 0 || specialistDefense < 0 || specialistElement < 0
                        || specialistHealpoints < 0)
                    {
                        return;
                    }

                    specialistInstance.SlDamage += specialistDamage;
                    specialistInstance.SlDefence += specialistDefense;
                    specialistInstance.SlElement += specialistElement;
                    specialistInstance.SlHP += specialistHealpoints;

                    Session.LoadSPStats();

                    Session.SendPackets(Session.Character.GenerateStatChar());
                    Session.SendPacket(Session.Character.GenerateStat());
                    Session.SendPacket(specialistInstance.GenerateSlInfo(Session));
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_SET"), 0));
                }
                else if (!Session.Character.IsSitting)
                {
                    if (Session.Character.Buff.Any(s => s.Card.BuffType == BuffType.Bad))
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_TRASFORM_WITH_DEBUFFS"),
                            0));
                        return;
                    }

                    if (Session.Character.Skills.Any(s => !s.CanBeUsed(true)))
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SKILLS_IN_LOADING"),
                                0));
                        return;
                    }

                    if (specialistInstance == null)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"),
                            0));
                        return;
                    }

                    if (Session.Character.IsVehicled)
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                        return;
                    }

                    double currentRunningSeconds =
                        (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;

                    if (Session.Character.UseSp)
                    {
                        if (Session.Character.Timespace != null && Session.Character.Timespace.SpNeeded?[(byte)Session.Character.Class] != 0 && Session.Character.Timespace.InstanceBag.Lock)
                        {
                            return;
                        }
                        Session.Character.LastSp = currentRunningSeconds;
                        Session.Character.RemoveSp(specialistInstance.ItemVNum, false);
                    }
                    else
                    {
                        if (Session.Character.LastMove.AddSeconds(1) >= DateTime.Now
                            || Session.Character.LastSkillUse.AddSeconds(2) >= DateTime.Now)
                        {
                            return;
                        }

                        if (Session.Character.SpPoint == 0 && Session.Character.SpAdditionPoint == 0)
                        {
                            Session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SP_NOPOINTS"), 0));
                        }

                        double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                        if (timeSpanSinceLastSpUsage >= Session.Character.SpCooldown)
                        {
                            if (spTransformPacket.Type == 1)
                            {
                                DateTime delay = DateTime.Now.AddSeconds(-6);
                                if (Session.Character.LastDelay > delay
                                    && Session.Character.LastDelay < delay.AddSeconds(2))
                                {
                                    Session.ChangeSp();
                                    Session.LoadSPStats();
                                }
                            }
                            else
                            {
                                Session.Character.LastDelay = DateTime.Now;
                                Session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 3, "#sl^1"));
                                Session.CurrentMapInstance?.Broadcast(
                                    UserInterfaceHelper.GenerateGuri(2, 1, Session.Character.CharacterId),
                                    Session.Character.PositionX, Session.Character.PositionY);
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"),
                                    Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)), 0));
                        }
                    }
                }
            }
        }
    }
}
