using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using System.Linq;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class TitleItem : Item
    {
        #region Instantiation

        public TitleItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            if (session.Character.IsVehicled)
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_DO_VEHICLED"), 10));
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance)
            {
                return;
            }

            if (session.Character.Inventory.CountItem(VNum) < 1)
            {
                return;
            }

            if (session.Character.Title.Any(s => s.TitleVnum == VNum))
            {
                return;
            }

            var conditions = ServerManager.Instance.TitleWearConditions.Where(s => s.TitleVNum.Equals(VNum));

            foreach (var condition in conditions)
            {
                if (session.Character.Title.Any(s => s.TitleVnum.Equals(condition.ConditionVNum)))
                {
                    continue;
                }

                session.SendPacket(session.Character.GenerateSay(condition.Message, 10));
                return;
            }

            session.SendPacket($"qna guri^306^{VNum}^{inv.Slot} {Language.Instance.GetMessageFromKey("ASK_TITLE")}");
        }

        #endregion
    }
}