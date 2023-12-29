﻿using System.Linq;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.PetManagement)]
    public class PetManagementHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public PetManagementHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Mate mate = Session.Character.Mates.Find(s => s.MateTransportId == packet.NpcId);
            switch (packet.Type)
            {
                case 2:
                    if (mate != null)
                    {
                        if (Session.Character.Miniland == Session.Character.MapInstance)
                        {
                            if (Session.Character.Level >= mate.Level)
                            {
                                Mate teammate = Session.Character.Mates.Where(s => s.IsTeamMember).FirstOrDefault(s => s.MateType == mate.MateType);
                                if (teammate != null)
                                {
                                    teammate.RemoveTeamMember();
                                }
                                mate.AddTeamMember();
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                            }
                        }
                    }
                    break;

                case 3:
                    if (mate != null && Session.Character.Miniland == Session.Character.MapInstance)
                    {
                        mate.RemoveTeamMember();
                    }
                    break;

                case 4:
                    if (mate != null)
                    {
                        if (Session.Character.Miniland == Session.Character.MapInstance)
                        {
                            mate.RemoveTeamMember(false);
                            mate.MapX = mate.PositionX;
                            mate.MapY = mate.PositionY;
                        }
                        else
                        {
                            Session.SendPacket($"qna #n_run^4^5^3^{mate.MateTransportId} {Language.Instance.GetMessageFromKey("ASK_KICK_PET")}");
                        }
                        break;
                    }
                    break;

                case 5:
                    if (mate != null)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 10, $"#n_run^4^6^3^{mate.MateTransportId}"));
                    }
                    break;

                case 6:
                    if (mate != null && Session.Character.Miniland != Session.Character.MapInstance)
                    {
                        mate.BackToMiniland();
                    }
                    break;

                case 7:
                    if (mate != null)
                    {
                        if (Session.Character.Mates.Any(s => s.MateType == mate.MateType && s.IsTeamMember))
                        {
                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 11));
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 0));
                        }
                        else
                        {
                            mate.RemoveTeamMember();
                            Session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 10, $"#n_run^4^9^3^{mate.MateTransportId}"));
                        }
                    }
                    break;

                case 9:
                    if (mate != null && mate.IsSummonable && Session.Character.MapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance)
                    {
                        if (Session.Character.Level >= mate.Level)
                        {
                            mate.PositionX = (short)(Session.Character.PositionX + (mate.MateType == MateType.Partner ? -1 : 1));
                            mate.PositionY = (short)(Session.Character.PositionY + 1);
                            mate.AddTeamMember();
                            Parallel.ForEach(Session.CurrentMapInstance.Sessions.Where(s => s.Character != null), s =>
                            {
                                if (ServerManager.Instance.ChannelId != 51 || Session.Character.Faction == s.Character.Faction)
                                {
                                    s.SendPacket(mate.GenerateIn(false, ServerManager.Instance.ChannelId == 51));
                                }
                                else
                                {
                                    s.SendPacket(mate.GenerateIn(true, ServerManager.Instance.ChannelId == 51, s.Account.Authority));
                                }
                            });
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                        }
                    }
                    break;
            }
            Session.SendPacket(Session.Character.GeneratePinit());
            Session.SendPackets(Session.Character.GeneratePst());
        }
    }
}
