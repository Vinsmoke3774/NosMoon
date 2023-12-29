using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.StarterBuffs)]
    public class StarterBuffsHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public StarterBuffsHandler(ClientSession session) : base(session)
        {
        }

        private DateTime LastBuff { get; set; }

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
            if (ServerManager.Instance.ChannelId != 51)
            {
                if (Session.Character.Level > 93)
                {
                    Session?.SendPacket(Session.Character?.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("LEVEL_TOO_HIGH")), 11));
                    return;
                }

                if (LastBuff > DateTime.Now.AddSeconds(-30))
                {
                    Session?.SendPacket(Session.Character.GenerateSay($"Please wait 30 seconds!", 11));
                    return;
                }

                LastBuff = DateTime.Now;
                Session.Character?.AddBuff(new Buff(155, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(153, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(72, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(89, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(91, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(139, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(152, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(157, 99), Npc?.BattleEntity);
                return;
            }

            else if (ServerManager.Instance.ChannelId == 51 )
            {
                if (LastBuff > DateTime.Now.AddSeconds(-180))
                {
                    Session?.SendPacket(Session.Character.GenerateSay($"Please wait 3 minutes!", 11));
                    return;
                }

                LastBuff = DateTime.Now;
                Session.Character?.AddBuff(new Buff(155, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(153, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(157, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(72, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(89, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(91, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(139, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(152, 99), Npc?.BattleEntity);
                Session.Character?.AddBuff(new Buff(74, 99), Npc?.BattleEntity);
                return;
            }

        }
    }
}
