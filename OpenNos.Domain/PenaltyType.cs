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

namespace OpenNos.Domain
{
    public enum PenaltyType : byte
    {
        Muted = 0,
        Banned = 1,
        BlockExp = 2,
        BlockFExp = 3,
        BlockRep = 4,
        Warning = 5,
        Namechange = 6,
        RainbowBan = 7,
        ChangePasswordWarn = 8,
        ArenaBan = 9,
        BattleRoyaleBan = 10,
        OneVersusOneBan = 11,
        TwoVersusTwoBan = 12,
        DeathMatch = 13,
        TeamElimination = 14,
        KingOfTheHill = 15
    }
}