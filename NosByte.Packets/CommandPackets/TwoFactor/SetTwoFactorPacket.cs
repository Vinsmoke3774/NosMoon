﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace NosByte.Packets.CommandPackets.TwoFactor
{
    [PacketHeader("$Enable2FA", Authority = AuthorityType.User, PassNonParseablePacket = true)]
    public class SetTwoFactorPacket : PacketDefinition
    {

    }
}
