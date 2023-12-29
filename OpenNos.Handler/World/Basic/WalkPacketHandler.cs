using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Basic
{
    public class WalkPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public WalkPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// walk packet
        /// </summary>
        /// <param name="walkPacket"></param>
        public void Walk(WalkPacket walkPacket)
        {
            if (Session.Character.CanMove())
            {
                if (Session.Character.MeditationDictionary.Count != 0)
                {
                    Session.Character.MeditationDictionary.Clear();
                }

                double currentRunningSeconds =
                    (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                double timeSpanSinceLastPortal = currentRunningSeconds - Session.Character.LastPortal;
                int distance =
                    Map.GetDistance(new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY },
                        new MapCell { X = walkPacket.XCoordinate, Y = walkPacket.YCoordinate });

                if (Session.HasCurrentMapInstance
                    && !Session.CurrentMapInstance.Map.IsBlockedZone(walkPacket.XCoordinate, walkPacket.YCoordinate)
                    && !Session.Character.IsChangingMapInstance && !Session.Character.HasShopOpened)
                {
                    Session.Character.PyjamaDead = false;
                    if (!Session.Character.InvisibleGm)
                    {
                        Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.Move(UserType.Player,
                            Session.Character.CharacterId, walkPacket.XCoordinate, walkPacket.YCoordinate,
                            Session.Character.Speed));
                    }
                    Session.SendPacket(Session.Character.GenerateCond());
                    Session.Character.WalkDisposable?.Dispose();
                    walk();
                    int interval = 100 - Session.Character.Speed * 5 + 100 > 0 ? 100 - Session.Character.Speed * 5 + 100 : 0;
                    Session.Character.WalkDisposable = Observable.Interval(TimeSpan.FromMilliseconds(interval)).SafeSubscribe(obs =>
                    {
                        walk();
                    });
                    void walk()
                    {
                        MapCell nextCell = Map.GetNextStep(new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY }, new MapCell { X = walkPacket.XCoordinate, Y = walkPacket.YCoordinate }, 1);

                        Session.Character.GetDir(Session.Character.PositionX, Session.Character.PositionY, nextCell.X, nextCell.Y);

                        if (Session.Character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                        {
                            Session.Character.MapX = nextCell.X;
                            Session.Character.MapY = nextCell.Y;
                        }

                        Session.Character.PositionX = nextCell.X;
                        Session.Character.PositionY = nextCell.Y;

                        Session.Character.LastMove = DateTime.Now;

                        if (Session.Character.IsVehicled)
                        {
                            Session.Character.Mates.Where(s => s.IsTeamMember || s.IsTemporalMate).ToList().ForEach(s =>
                            {
                                s.PositionX = Session.Character.PositionX;
                                s.PositionY = Session.Character.PositionY;
                            });
                        }

                        Session.CurrentMapInstance?.OnAreaEntryEvents
                            ?.Where(s => s.InZone(Session.Character.PositionX, Session.Character.PositionY)).ToList()
                            .ForEach(e => e.Events.ForEach(evt => EventHelper.Instance.RunEvent(evt)));
                        Session.CurrentMapInstance?.OnAreaEntryEvents?.RemoveAll(s =>
                            s.InZone(Session.Character.PositionX, Session.Character.PositionY));

                        Session.CurrentMapInstance?.OnMoveOnMapEvents?.ForEach(e => EventHelper.Instance.RunEvent(e));
                        Session.CurrentMapInstance?.OnMoveOnMapEvents?.RemoveAll(s => s != null);

                        if (Session.Character.PositionX == walkPacket.XCoordinate && Session.Character.PositionY == walkPacket.YCoordinate)
                        {
                            Session.Character.WalkDisposable?.Dispose();
                        }
                    }
                }
            }
        }
    }
}
