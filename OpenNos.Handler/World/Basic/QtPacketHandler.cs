using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.Basic
{
    public class QtPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public QtPacketHandler(ClientSession session) => Session = session;

        public void QtPacket(QtPacket qtPacket)
        {
            switch (qtPacket.Type)
            {
                // On Target Dest
                case 1:
                    Session.Character.IncrementQuests(QuestType.GoTo, Session.CurrentMapInstance.Map.MapId, Session.Character.PositionX, Session.Character.PositionY);
                    break;

                // Give Up Quest
                case 3:
                    CharacterQuest charQuest = Session.Character.Quests?.FirstOrDefault(q => q.QuestNumber == qtPacket.Data);
                    if (charQuest == null)
                    {
                        return;
                    }

                    if (charQuest.IsMainQuest && charQuest.QuestId < 5478)
                    {
                        return;
                    }

                    Session.Character.RemoveQuest(charQuest.QuestId, true);
                    break;

                // Ask for rewards
                case 4:
                    break;
            }
        }
    }
}
