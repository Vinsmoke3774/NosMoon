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

using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject.Algorithms;
using OpenNos.GameObject.Algorithms.Geography;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Map : IMapDTO
    {
        //private readonly Random _random;

        #region Members

        //Function to get a random number
        private static readonly Random random = new Random();

        private static readonly object syncLock = new object();

        #endregion

        #region Instantiation

        public Map(short mapId, short gridMapId, byte[] data)
        {
            MapId = mapId;
            GridMapId = gridMapId;
            Data = data;
            LoadZone();
            MapTypes = new List<MapTypeDTO>();
            foreach (MapTypeMapDTO maptypemap in DAOFactory.MapTypeMapDAO.LoadByMapId(mapId).ToList())
            {
                MapTypeDTO maptype = DAOFactory.MapTypeDAO.LoadById(maptypemap.MapTypeId);
                MapTypes.Add(maptype);
            }

            if (MapTypes.Count > 0 && MapTypes[0].RespawnMapTypeId != null)
            {
                long? respawnMapTypeId = MapTypes[0].RespawnMapTypeId;
                long? returnMapTypeId = MapTypes[0].ReturnMapTypeId;
                if (respawnMapTypeId != null)
                {
                    DefaultRespawn = DAOFactory.RespawnMapTypeDAO.LoadById((long)respawnMapTypeId);
                }
                if (returnMapTypeId != null)
                {
                    DefaultReturn = DAOFactory.RespawnMapTypeDAO.LoadById((long)returnMapTypeId);
                }
            }
        }

        #endregion

        #region Properties

        public Tile[,] Tiles;

        public byte[] Data { get; set; }

        public RespawnMapTypeDTO DefaultRespawn { get; }

        public RespawnMapTypeDTO DefaultReturn { get; }

        public short GridMapId { get; set; }

        public short MapId { get; set; }

        public List<MapTypeDTO> MapTypes { get; }

        public int Music { get; set; }

        public string Name { get; set; }

        public bool ShopAllowed { get; set; }

        public byte XpRate { get; set; }

        internal int XLength { get; set; }

        internal int YLength { get; set; }

        private ConcurrentBag<MapCell> Cells { get; set; }

        public byte MonsterKilled { get; set; }

        #endregion

        #region Methods

        public Func<Tile, Tile[]> NeighboursManhattan()
        {
            bool IsValid(Tile tile) => tile.X >= 0 && tile.Y >= 0 && tile.X < XLength && tile.Y < YLength;
            return delegate (Tile tile)
            {
                var neighbours = new List<Tile>()
                {
                    tile.Translate(1, 0),
                    tile.Translate(-1, 0),
                    tile.Translate(0, 1),
                    tile.Translate(0, -1)
                }.Where(IsValid);

                return neighbours.ToArray();
            };
        }

        public Func<Tile, Tile[]> NeighboursManhattanAndDiagonal()
        {
            bool IsValid(Tile tile) => tile.X >= 0 && tile.Y >= 0 && tile.X < XLength && tile.Y < YLength;
            return delegate (Tile tile)
            {
                var neighbours = new List<Tile>()
                {
                    tile.Translate(1, 0),
                    tile.Translate(-1, 0),
                    tile.Translate(0, 1),
                    tile.Translate(0, -1),

                    tile.Translate(1, 1),
                    tile.Translate(-1, -1),
                    tile.Translate(-1, 1),
                    tile.Translate(1, -1),
                }.Where(IsValid);

                return neighbours.ToArray();
            };
        }

        public Func<Tile, int> IndexMap()
        {
            return tile => (tile.X * XLength) + tile.Y;
        }

        public static int GetDistance(Character character1, Character character2) => GetDistance(new MapCell { X = character1.PositionX, Y = character1.PositionY }, new MapCell { X = character2.PositionX, Y = character2.PositionY });

        public static int GetDistance(MapCell p, MapCell q) => (int)Heuristic.GetDistance((p.X, p.Y), (q.X, q.Y));

        public static MapCell GetNextStep(MapCell start, MapCell end, double steps)
        {
            MapCell futurPoint;
            double newX = start.X;
            double newY = start.Y;

            if (start.X < end.X)
            {
                newX = start.X + (steps);
                if (newX > end.X)
                    newX = end.X;
            }
            else if (start.X > end.X)
            {
                newX = start.X - (steps);
                if (newX < end.X)
                    newX = end.X;
            }
            if (start.Y < end.Y)
            {
                newY = start.Y + (steps);
                if (newY > end.Y)
                    newY = end.Y;
            }
            else if (start.Y > end.Y)
            {
                newY = start.Y - (steps);
                if (newY < end.Y)
                    newY = end.Y;
            }

            futurPoint = new MapCell { X = (short)newX, Y = (short)newY };
            return futurPoint;
        }

        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }

        public bool CanWalkAround(int x, int y)
        {
            for (int dX = -1; dX <= 1; dX++)
            {
                for (int dY = -1; dY <= 1; dY++)
                {
                    if (dX == 0 && dY == 0)
                    {
                        continue;
                    }

                    if (!IsBlockedZone((short)(x + dX), (short)(y + dY)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public IEnumerable<MonsterToSummon> GenerateMonsters(short vnum, short amount, bool move, List<EventContainer> deathEvents, bool isBonus = false, bool isHostile = true, bool isBoss = false)
        {
            List<MonsterToSummon> SummonParameters = new List<MonsterToSummon>();
            for (int i = 0; i < amount; i++)
            {
                MapCell cell = GetRandomPosition();
                SummonParameters.Add(new MonsterToSummon(vnum, cell, null, move, isBonus: isBonus, isHostile: isHostile, isBoss: isBoss) { DeathEvents = deathEvents });
            }
            return SummonParameters;
        }

        public List<NpcToSummon> GenerateNpcs(short vnum, short amount, List<EventContainer> deathEvents, bool isMate, bool isProtected, bool move, bool isHostile)
        {
            List<NpcToSummon> SummonParameters = new List<NpcToSummon>();
            for (int i = 0; i < amount; i++)
            {
                MapCell cell = GetRandomPosition();
                SummonParameters.Add(new NpcToSummon(vnum, cell, -1, isProtected, isMate, move, isHostile) { DeathEvents = deathEvents });
            }
            return SummonParameters;
        }

        public MapCell GetRandomPosition()
        {
            if (Cells == null)
            {
                Cells = new ConcurrentBag<MapCell>();
                Parallel.For(0, YLength, y => Parallel.For(0, XLength, x =>
                {
                    if (!IsBlockedZone((short)x, (short)y) && CanWalkAround(x, y))
                    {
                        Cells.Add(new MapCell { X = (short)x, Y = (short)y });
                    }
                }));
            }
            return Cells.OrderBy(s => RandomNumber(0, int.MaxValue)).FirstOrDefault();
        }

        public MapCell GetRandomPositionByDistance(short xPos, short yPos, short distance, bool randomInRange = false)
        {
            if (Cells == null)
            {
                Cells = new ConcurrentBag<MapCell>();
                Parallel.For(0, YLength, y => Parallel.For(0, XLength, x =>
                {
                    if (!IsBlockedZone((short)x, (short)y))
                    {
                        Cells.Add(new MapCell { X = (short)x, Y = (short)y });
                    }
                }));
            }
            if (randomInRange)
            {
                return Cells.Where(s => GetDistance(new MapCell { X = xPos, Y = yPos }, new MapCell { X = s.X, Y = s.Y }) <= distance && !IsBlockedZone(xPos, yPos, s.X, s.Y)).OrderBy(s => RandomNumber(0, int.MaxValue)).FirstOrDefault();
            }
            else
            {
                return Cells.Where(s => GetDistance(new MapCell { X = xPos, Y = yPos }, new MapCell { X = s.X, Y = s.Y }) <= distance && !IsBlockedZone(xPos, yPos, s.X, s.Y)).OrderBy(s => RandomNumber(0, int.MaxValue)).OrderByDescending(s => GetDistance(new MapCell { X = xPos, Y = yPos }, new MapCell { X = s.X, Y = s.Y })).FirstOrDefault();
            }
        }

        public bool IsBlockedZone(short firstX, short firstY, short mapX, short mapY)
        {
            var posX = (short)Math.Abs(mapX - firstX);
            var posY = (short)Math.Abs(mapY - firstY);

            var positiveX = mapX > firstX;
            var positiveY = mapY > firstY;

            for (var i = 0; i <= posX; i++)
            {
                if (!IsWalkable((short)((positiveX ? i : -i) + firstX), firstY))
                {
                    return true;
                }
            }

            for (var i = 0; i <= posY; i++)
            {
                if (!IsWalkable(firstX, (short)((positiveY ? i : -i) + firstY)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBlockedZone(short x, short y)
        {
            try
            {
                if ((MapId == 2552 && y > 38)
                    || x < 0
                    || y < 0
                    || x >= XLength
                    || y >= YLength
                    )
                {
                    return true;
                }

                return !IsWalkable(x, y);
            }
            catch
            {
                return true;
            }
        }

        internal bool GetFreePosition(ref short firstX, ref short firstY, byte xpoint, byte ypoint)
        {
            short MinX = (short)(-xpoint + firstX);
            short MaxX = (short)(xpoint + firstX);

            short MinY = (short)(-ypoint + firstY);
            short MaxY = (short)(ypoint + firstY);

            List<MapCell> cells = new();
            for (short y = MinY; y <= MaxY; y++)
            {
                for (short x = MinX; x <= MaxX; x++)
                {
                    if ((x != firstX) || (y != firstY))
                    {
                        cells.Add(new MapCell { X = x, Y = y });
                    }
                }
            }
            foreach (MapCell cell in cells.OrderBy(_ => RandomNumber(0, int.MaxValue)))
            {
                if (IsBlockedZone(firstX, firstY, cell.X, cell.Y))
                {
                    continue;
                }

                firstX = cell.X;
                firstY = cell.Y;
                return true;
            }
            return false;
        }

        public bool IsWalkable(short mapX, short mapY)
        {
            if ((mapX > XLength) || (mapX < 0) || (mapY > YLength) || (mapY < 0))
            {
                return false;
            }

            return IsWalkable(Tiles[mapX, mapY].Value);
        }

        public bool IsWalkable(byte value) => value == 0 || value == 2 || (value >= 16 && value <= 19);

        private void LoadZone()
        {
            using BinaryReader reader = new(new MemoryStream(Data));
            XLength = reader.ReadInt16();
            YLength = reader.ReadInt16();
            Tiles = new Tile[XLength, YLength];

            for (short i = 0; i < YLength; ++i)
            {
                for (short t = 0; t < XLength; ++t)
                {
                    Tiles[t, i] = new Tile(t, i, reader.ReadByte());
                }
            }
        }

        #endregion
    }
}