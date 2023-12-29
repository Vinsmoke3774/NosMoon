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

using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class ProduceItem : Item
    {
        #region Instantiation

        public ProduceItem(ItemDTO item) : base(item)
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

            if (session.CurrentMapInstance.MapInstanceType == Domain.MapInstanceType.TalentArenaMapInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.ArenaInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance)
            {
                return;
            }

            switch (Effect)
            {
                case 100:
                    session.Character.LastNRunId = 0;
                    session.Character.LastItemVNum = inv.ItemVNum;
                    session.SendPacket("wopen 28 0 0");
                    List<Recipe> recipeList = ServerManager.Instance.GetRecipesByItemVNum(VNum);
                    string list = recipeList.Where(s => s.Amount > 0).Aggregate("m_list 2", (current, s) => current + $" {s.ItemVNum}");
                    session.SendPacket(list + (EffectValue <= 110 && EffectValue >= 108 ? " 999" : ""));
                    var energyField = session.Character.EnergyFields.FirstOrDefault(s => s.MapId == session.Character.MapId); // He shouldn't be able to create a ts if he's going outside of the zone after getting the "CanCreateTimespace" from using the dowsing rod

                    if (EffectValue.IsBetween(80, 84) && energyField == null)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg("You can't create a timespace on this map !", 0));
                        return;
                    }

                    if (EffectValue.IsBetween(80, 84) && (!session.Character.CanCreateTimeSpace || !session.Character.IsInRange(energyField.MapX, energyField.MapY, 5)))
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg("You can't create a timespace here !", 0));
                        return;
                    }

                    switch (EffectValue)
                    {
                        // Group Time-Space Piece
                        case 80:
                            if (!session.Character.Group.IsMemberOfGroup(session))
                            {
                                session.SendPacket(session.Character.GenerateSay("You should have a group to do this timespace !", 10));
                                return;
                            }
                            break;

                        // Individual Time-Space Piece
                        case 81:
                            if (session.Character.Group != null)
                            {
                                session.SendPacket(session.Character.GenerateSay("You shouldn't have a group to do this timespace !", 10));
                                return;
                            }

                            session.SendPacket("wopen 28 0 0");
                            session.SendPacket(list + (EffectValue <= 110 && EffectValue >= 108 ? " 999" : ""));
                            break;

                        // Hunting Time-Space Piece
                        case 82:

                            break;

                        // Raid Time-Space Piece
                        case 83:
                            session.SendPacket("wopen 28 0 0");
                            session.SendPacket(list + (EffectValue <= 110 && EffectValue >= 108 ? " 999" : ""));
                            break;

                        // Laboratory Time-Space Piece
                        case 84:

                            break;

                        default:
                            session.SendPacket("wopen 28 0 0");
                            session.SendPacket(list + (EffectValue <= 110 && EffectValue >= 108 ? " 999" : ""));
                            break;
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(), VNum, Effect, EffectValue));
                    break;
            }
        }

        #endregion
    }
}