using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class ExchangeRequestPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ExchangeRequestPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// req_exc packet
        /// </summary>
        /// <param name="exchangeRequestPacket"></param>
        public void ExchangeRequest(ExchangeRequestPacket exchangeRequestPacket)
        {
            if (exchangeRequestPacket != null)
            {
                ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(exchangeRequestPacket.CharacterId);

                if (sess != null && Session.Character.MapInstanceId != sess.Character.MapInstanceId)
                {
                    sess.Character.ExchangeInfo = null;
                    Session.Character.ExchangeInfo = null;
                }
                else
                {
                    switch (exchangeRequestPacket.RequestType)
                    {
                        case RequestExchangeType.Requested:
                            Session.Request(sess);
                            break;

                        case RequestExchangeType.Confirmed: // click Trade button in exchange window
                            Session.Confirm();
                            break;

                        case RequestExchangeType.Cancelled: // cancel trade thru exchange window
                            Session.Close(sess);
                            break;

                        case RequestExchangeType.List:
                            Session.List(sess);
                            break;

                        case RequestExchangeType.Declined:
                            Session.Decline(sess);
                            break;

                        default:
                            Logger.Log.Warn(
                                $"Exchange-Request-Type not implemented. RequestType: {exchangeRequestPacket.RequestType})");
                            break;
                    }
                }
            }
        }
    }
}
