using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;
using System;
using System.Linq;

namespace OpenNos.Handler.World.BattlePass
{
    public class BpPSelPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public BpPSelPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// bp_psel packet
        /// </summary>
        /// <param name="bpPsel"></param>
        public void BattlePassOpen(BpPsel bpPsel)
        {
            bpPsel.Palier++;
            if (bpPsel.Type != GetBattlePassItemType.GetAll)
            {
                bool alreadyTaken = (bool)Session.Character.BattlePassItemLogs?.Any(s => s.IsPremium == (bpPsel.Type == GetBattlePassItemType.Premium) && s.Palier == bpPsel.Palier);

                if (alreadyTaken) return;

                var checkIfExist = ServerManager.Instance.BattlePassItems.Find(s => s.Palier == bpPsel.Palier && s.IsPremium == (bpPsel.Type == GetBattlePassItemType.Premium));

                if (checkIfExist == null) return;

                var getPalier = ServerManager.Instance.BattlePassPaliers.FirstOrDefault(s => s.Id == checkIfExist.Palier);

                if (getPalier == null) return;

                if (getPalier.MaximumBattlePassPoint > Session.Character.BattlePassPoints) return;
            }

            switch (bpPsel.Type)
            {
                case GetBattlePassItemType.Free:
                case GetBattlePassItemType.Premium:
                    var item = ServerManager.Instance.BattlePassItems.Find(s => s.Palier == bpPsel.Palier && s.IsPremium == (bpPsel.Type == GetBattlePassItemType.Premium));
                    if (item == null) return;
                    var canGetItem = ServerManager.Instance.BattlePassPaliers.Find(s => s.Id == item.Palier);
                    if (canGetItem == null) return;
                    if (canGetItem.MaximumBattlePassPoint > Session.Character.BattlePassPoints) return;
                    Session.Character.GiftAdd(item.ItemVNum, item.Amount);

                    Session.Character.BattlePassItemLogs.Add(new BattlePassItemLogsDTO
                    {
                        Id = Guid.NewGuid(),
                        CharacterId = Session.Character.CharacterId,
                        IsPremium = bpPsel.Type == GetBattlePassItemType.Premium,
                        Palier = item.Palier
                    });
                    break;

                case GetBattlePassItemType.GetAll:
                    foreach (var itemBp in ServerManager.Instance.BattlePassItems)
                    {
                        bool alreadyTaken = Session.Character.BattlePassItemLogs.Any(s => s.IsPremium == itemBp.IsPremium && s.Palier == itemBp.Palier);
                        if (alreadyTaken) continue;
                        var canGetItem2 = ServerManager.Instance.BattlePassPaliers.Find(s => s.Id == itemBp.Palier);
                        if (canGetItem2 == null) return;
                        if (canGetItem2.MaximumBattlePassPoint > Session.Character.BattlePassPoints) return;
                        Session.Character.GiftAdd(itemBp.ItemVNum, itemBp.Amount);

                        Session.Character.BattlePassItemLogs.Add(new BattlePassItemLogsDTO
                        {
                            Id = Guid.NewGuid(),
                            CharacterId = Session.Character.CharacterId,
                            IsPremium = itemBp.IsPremium,
                            Palier = itemBp.Palier
                        });
                    }
                    break;
            }

            Session.SendPacket(Session.Character.GenerateBpp());
        }
    }
}