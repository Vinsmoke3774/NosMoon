using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.Basic
{
    public class SetQuickListPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SetQuickListPacketHandler(ClientSession session) => Session = session;


        /// <summary>
        /// qset packet
        /// </summary>
        /// <param name="qSetPacket"></param>
        public void SetQuicklist(QSetPacket qSetPacket)
        {
            short data1 = 0, data2 = 0, type = qSetPacket.Type, q1 = qSetPacket.Q1, q2 = qSetPacket.Q2;
            if (qSetPacket.Data1.HasValue)
            {
                data1 = qSetPacket.Data1.Value;
            }

            if (qSetPacket.Data2.HasValue)
            {
                data2 = qSetPacket.Data2.Value;
            }

            switch (type)
            {
                case 0:
                case 1:

                    // client says qset 0 1 3 2 6 answer -> qset 1 3 0.2.6.0
                    Session.Character.QuicklistEntries.RemoveAll(n =>
                        n.Q1 == q1 && n.Q2 == q2
                        && (Session.Character.UseSp ? n.Morph == Session.Character.Morph : n.Morph == 0));

                    var morph = Session.Character.Morph;
                    if (Session.Character.Class == ClassType.MartialArtist && Session.Character.Morph == (byte)BrawlerMorphType.Dragon || Session.Character.Morph == (byte)BrawlerMorphType.Normal)
                    {
                        morph = 30;
                    }

                    Session.Character.QuicklistEntries.Add(new QuicklistEntryDTO
                    {
                        CharacterId = Session.Character.CharacterId,
                        Type = type,
                        Q1 = q1,
                        Q2 = q2,
                        Slot = data1,
                        Pos = data2,
                        Morph = (short)(Session.Character.UseSp ? morph : 0)
                    });
                    Session.SendPacket($"qset {q1} {q2} {type}.{data1}.{data2}.0");
                    break;

                case 2:

                    morph = Session.Character.Morph;
                    if (Session.Character.Class == ClassType.MartialArtist && Session.Character.Morph == (byte)BrawlerMorphType.Dragon || Session.Character.Morph == (byte)BrawlerMorphType.Normal)
                    {
                        morph = 30;
                    }

                    // DragDrop / Reorder qset type to1 to2 from1 from2 vars -> q1 q2 data1 data2
                    QuicklistEntryDTO qlFrom = Session.Character.QuicklistEntries.SingleOrDefault(n =>
                        n.Q1 == data1 && n.Q2 == data2
                        && (Session.Character.UseSp ? n.Morph == morph : n.Morph == 0));
                    if (qlFrom != null)
                    {
                        QuicklistEntryDTO qlTo = Session.Character.QuicklistEntries.SingleOrDefault(n =>
                            n.Q1 == q1 && n.Q2 == q2 && (Session.Character.UseSp
                                ? n.Morph == morph
                                : n.Morph == 0));
                        qlFrom.Q1 = q1;
                        qlFrom.Q2 = q2;
                        if (qlTo == null)
                        {
                            // Put 'from' to new position (datax)
                            Session.SendPacket(
                                $"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // old 'from' is now empty.
                            Session.SendPacket($"qset {data1} {data2} 7.7.-1.0");
                        }
                        else
                        {
                            // Put 'from' to new position (datax)
                            Session.SendPacket(
                                $"qset {qlFrom.Q1} {qlFrom.Q2} {qlFrom.Type}.{qlFrom.Slot}.{qlFrom.Pos}.0");

                            // 'from' is now 'to' because they exchanged
                            qlTo.Q1 = data1;
                            qlTo.Q2 = data2;
                            Session.SendPacket($"qset {qlTo.Q1} {qlTo.Q2} {qlTo.Type}.{qlTo.Slot}.{qlTo.Pos}.0");
                        }
                    }

                    break;

                case 3:
                    morph = Session.Character.Morph;
                    if (Session.Character.Class == ClassType.MartialArtist && Session.Character.Morph == (byte)BrawlerMorphType.Dragon || Session.Character.Morph == (byte)BrawlerMorphType.Normal)
                    {
                        morph = 30;
                    }

                    // Remove from Quicklist
                    Session.Character.QuicklistEntries.RemoveAll(n =>
                        n.Q1 == q1 && n.Q2 == q2
                        && (Session.Character.UseSp ? n.Morph == morph : n.Morph == 0));
                    Session.SendPacket($"qset {q1} {q2} 7.7.-1.0");
                    break;

                default:
                    return;
            }
        }
    }
}
