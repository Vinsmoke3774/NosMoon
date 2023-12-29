using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Event.KINGOFTHEHILL;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Reactive.Linq;
using NosByte.Shared;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.KingOfTheHillMatchmaking)]
    public class KingOfTheHillMatchmaking : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public KingOfTheHillMatchmaking(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (ServerManager.Instance.KingOfTheHillMembers.Count >= 50)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg("King of the hill event is full !", 0));
                return;
            }

            Session.SendPacket(UserInterfaceHelper.GenerateBSInfo(0, 27, ServerManager.Instance.KingOfTheHillTimer, 1));

            if (ServerManager.Instance.KingOfTheHillTimer == 300 && ServerManager.Instance.KingOfTheHillMembers.Count == 0)
            {
                IDisposable disp1 = default;
                disp1 = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(s =>
                {
                    ServerManager.Instance.KingOfTheHillTimer--;

                    if (ServerManager.Instance.KingOfTheHillTimer <= 0)
                    {
                        ServerManager.Instance.KingOfTheHillTimer = 300;
                        disp1?.Dispose();
                    }
                });
            }

            var hillMember = new KingOfTheHillMember
            {
                Session = Session
            };

            ServerManager.Instance.KingOfTheHillMembers.Add(hillMember);

            if (ServerManager.Instance.KingOfTheHillMembers.Count == 1)
            {
                Observable.Timer(TimeSpan.FromSeconds(ServerManager.Instance.KingOfTheHillTimer)).SafeSubscribe(s =>
                {
                    KingOfTheHill.Split();
                });
            }
        }
    }
}