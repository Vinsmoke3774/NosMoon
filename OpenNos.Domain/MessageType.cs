﻿/*
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
    public enum MessageType : byte
    {
        Whisper = 0,
        PrivateChat = 1,
        Family = 2,
        Shout = 3,
        FamilyChat = 4,
        WhisperSupport = 5,
        WhisperGM = 6,
        Other = 7,
        Broadcast = 8,
        Speaker = 9,
        ReceivePacket = 10,
        Staff = 11,
        KickChannel = 12
    }
}