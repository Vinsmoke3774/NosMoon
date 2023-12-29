using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.OpenPetBasket)]
    public class OpenPetBasketHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public OpenPetBasketHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (Session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBasket))
            {
                Session.SendPacket(Session.Character.GenerateStashAll());
            }
        }
    }
}
