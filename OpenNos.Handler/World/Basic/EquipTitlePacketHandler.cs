using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Basic
{
    public class EquipTitlePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public EquipTitlePacketHandler(ClientSession session) => Session = session;

        public void TitEqHandler(TitEqPacket e)
        {
            CharacterTitleDTO aa = Session.Character.Title.FirstOrDefault(s => s.TitleVnum == e.ItemVnum);

            if (aa == null)
            {
                return;
            }

            if (Session.Character.LastDefence.AddSeconds(5) > DateTime.Now)
            {
                Session.Character.GenerateSay("You can't equip a new title during a combat.", 10);
                return;
            }

            switch (e.Type)
            {
                case 1:

                    //show
                    foreach (var a in Session.Character.Title.Where(s => s != aa))
                    {
                        switch (a.Stat)
                        {
                            case 7 when a != aa:
                                a.Stat = 5;
                                break;

                            case 3 when a != aa:
                                a.Stat = 1;
                                break;
                                /*case 5 when a != aa:
                                    a.Stat = 7;
                                    break;*/
                        }
                    }

                    aa.Stat = (byte)(aa.Stat == 1 ? 3 : aa.Stat == 5 ? 7 : aa.Stat == 7 ? 5 : 1);
                    break;

                case 2:

                    //effect
                    foreach (var a in Session.Character.Title)
                    {
                        switch (a.Stat)
                        {
                            case 7 when a != aa:
                                a.Stat = 3;
                                break;

                            case 5 when a != aa:
                                a.Stat = 1;
                                break;
                                /*case 3 when a != aa:
                                    a.Stat = 7;
                                    break;*/
                        }
                    }
                    aa.Stat = (byte)(aa.Stat == 1 ? 5 : aa.Stat == 3 ? 7 : aa.Stat == 7 ? 3 : 1);

                    break;

                default:
                    return;
            }

            Session.SendPacket(Session.Character.GenerateTitle());
            Session.CurrentMapInstance.Broadcast(Session.Character.GenerateTitInfo());
            Session.Character.GetEffectFromTitle();
            Session.Character.GetVisualFromTitle();
        }
    }
}
