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

using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using System.Threading.Tasks;

namespace OpenNos.Master.Library.Client
{
    internal class CommunicationClient : ICommunicationClient
    {
        #region Methods

        public void CharacterConnected(long characterId) => Task.Run(() => CommunicationServiceClient.Instance.OnCharacterConnected(characterId));

        public void CharacterDisconnected(long characterId) => Task.Run(() => CommunicationServiceClient.Instance.OnCharacterDisconnected(characterId));

        public void KickSession(long? accountId, int? sessionId) => Task.Run(() => CommunicationServiceClient.Instance.OnKickSession(accountId, sessionId));

        public void Restart(int time = 5) => Task.Run(() => CommunicationServiceClient.Instance.OnRestart(time));

        public void RunGlobalEvent(Domain.EventType eventType) => Task.Run(() => CommunicationServiceClient.Instance.OnRunGlobalEvent(eventType));

        public void SendMessageToCharacter(SCSCharacterMessage message) => Task.Run(() => CommunicationServiceClient.Instance.OnSendMessageToCharacter(message));

        public void Shutdown(short time) => Task.Run(() => CommunicationServiceClient.Instance.OnShutdown(time));

        public void UpdateFamily(long familyId, bool changeFaction) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateFamily(familyId, changeFaction));

        public void UpdatePenaltyLog(int penaltyLogId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdatePenaltyLog(penaltyLogId));

        public void UpdateRelation(long relationId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateRelation(relationId));

        public void UpdateStaticBonus(long characterId) => Task.Run(() => CommunicationServiceClient.Instance.OnUpdateStaticBonus(characterId));

        #endregion
    }
}