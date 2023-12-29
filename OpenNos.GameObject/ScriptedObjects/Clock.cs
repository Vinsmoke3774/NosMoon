/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NosByte.Shared;

namespace OpenNos.GameObject
{
    public class Clock
    {
        private IDisposable _tickInterval { get; set; }

        #region Instantiation

        public Clock(byte type)
        {
            StopEvents = new List<EventContainer>();
            TimeoutEvents = new List<EventContainer>();
            Type = type;
            SecondsRemaining = 1;
        }

        #endregion

        #region Properties

        public bool Enabled { get; private set; }

        public int SecondsRemaining { get; set; }

        public List<EventContainer> StopEvents { get; set; }

        public List<EventContainer> TimeoutEvents { get; set; }

        public int TotalSecondsAmount { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public void AddTime(int seconds)
        {
            SecondsRemaining += seconds * 10;
            TotalSecondsAmount += seconds * 10;
        }

        public string GetClock() => $"evnt {Type} {(Enabled ? 0 : (Type != 3) ? -1 : 1)} {SecondsRemaining} {TotalSecondsAmount}";

        public void StartClock()
        {
            Enabled = true;
            _tickInterval = Observable.Interval(TimeSpan.FromSeconds(1)).SafeSubscribe(x =>
            {
                Tick();
            });
        }

        public void StopClock(bool runEndEvents = true)
        {
            Enabled = false;
            _tickInterval?.Dispose();

            if (runEndEvents)
            {
                StopEvents.ForEach(e => EventHelper.Instance.RunEvent(e));
            }

            StopEvents.RemoveAll(s => s != null);
        }

        private void Tick()
        {
            if (SecondsRemaining > 0)
            {
                SecondsRemaining -= 10;
                return;
            }

            StopClock();
        }

        #endregion
    }
}