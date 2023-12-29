using System;
using System.Collections.Generic;
using System.Linq;
using NosByte.Packets.CommandPackets.TwoFactor;
using NosByte.Packets.Lock;
using OpenNos.Core;
using OpenNos.Core.TwoFactorAuth;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler
{
    public class LockPacketHandler : IPacketHandler
    {
        #region Instantiation

        public LockPacketHandler(ClientSession session) => Session = session;

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        public void Execute(RemoveTwoFactorPacket removeLockPacket)
        {
            if (!Session.Account.TwoFactorEnabled || !string.IsNullOrEmpty(Session.Account.LockCode))
            {
                return;
            }

            var googleAuthentificator = new GoogleAuthService();
            bool isCorrectPin = googleAuthentificator.ValidatePinCode(Session.Account.GeneratedAuthKey, removeLockPacket.Code);
            var backupCodes = DAOFactory.TwoFactorBackupsDAO.LoadByAccountId(Session.Account.AccountId);

            var isBackupCode = backupCodes.Any(s => s.Code == removeLockPacket.Code);

            if (isBackupCode)
            {
                Session.Account.TwoFactorEnabled = false;
                Session.Account.GeneratedAuthKey = string.Empty;
                DAOFactory.TwoFactorBackupsDAO.DeleteByAccountId(Session.Account.AccountId);
                Session.SendPacket(Session.Character.GenerateSay("TwoFactor authentication disabled.", 10));
                return;
            }

            if (!isCorrectPin)
            {
                Session.SendPacket(Session.Character.GenerateSay("The code you entered is invalid.", 10));
                return;
            }

            Session.Account.TwoFactorEnabled = false;
            Session.Account.GeneratedAuthKey = string.Empty;
            DAOFactory.TwoFactorBackupsDAO.DeleteByAccountId(Session.Account.AccountId);
            Session.SendPacket(Session.Character.GenerateSay("Two factor is now disabled.", 10));

        }

        public void Execute(SetTwoFactorPacket packet)
        {
            if (!string.IsNullOrEmpty(Session.Account.LockCode))
            {
                Session.SendPacket(Session.Character.GenerateSay($"Please remove your lock code before adding 2FA.", 11));
                return;
            }

            if (Session.Character.IsLocked)
            {
                return;
            }

            if (Session.Account.TwoFactorEnabled)
            {
                return;
            }

            var googleAuthentification = new GoogleAuthService();
            var generatedKey = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 10);

            var backupCodes = new List<TwoFactorBackupDTO>();

            Session.SendPacket(Session.Character.GenerateSay("---------- BACKUP CODES ----------", 11));

            for (int i = 0; i < 5; i++)
            {
                var newCode = new TwoFactorBackupDTO
                {
                    AccountId = Session.Account.AccountId,
                    Code = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 5)
                };

                Session.SendPacket(Session.Character.GenerateSay($"{newCode.Code}", 10));
                backupCodes.Add(newCode);
            }

            Session.SendPacket(Session.Character.GenerateSay("Write those backup codes somewhere.", 11));
            Session.SendPacket(Session.Character.GenerateSay("They will never be provided again.", 11));
            DAOFactory.TwoFactorBackupsDAO.InsertOrUpdate(backupCodes);
            Session.SendPacket(Session.Character.GenerateSay("----------------------------------", 11));

            Session.Account.GeneratedAuthKey = generatedKey;
            string manualEntryKey = googleAuthentification.GenerateSetupCode(ServerManager.Instance.Configuration.GoogleIssuer, ServerManager.Instance.Configuration.GoogleTitleNoSpace, generatedKey);
            Session.SendPacket(Session.Character.GenerateSay($"Download google authenticator and click on \" + \" and add your key : {manualEntryKey}", 12));
            Session.Account.TwoFactorEnabled = true;

        }

        public void Execute(UnlockTwoFactorPacket unlockPacket)
        {
            if (string.IsNullOrEmpty(unlockPacket.Code) || !Session.Character.IsLocked)
            {
                return;
            }

            var tfa = new GoogleAuthService();
            var backupCodes = DAOFactory.TwoFactorBackupsDAO.LoadByAccountId(Session.Account.AccountId);
            bool isCorrectPin = tfa.ValidatePinCode(Session.Account.GeneratedAuthKey, unlockPacket.Code);

            var isBackupCode = backupCodes.Any(s => s.Code == unlockPacket.Code);

            if (isBackupCode)
            {
                Session.Character.RemoveLock();
                Session.SendPacket(Session.Character.GenerateSay("Character unlocked.", 10));
                Session.Account.TwoFactorEnabled = false;
                Session.Account.GeneratedAuthKey = string.Empty;
                Session.SendPacket(Session.Character.GenerateSay("TwoFactor authentication disabled.", 10));
                return;
            }

            if (!isCorrectPin)
            {
                Session.SendPacket(Session.Character.GenerateSay("The code entered is invalid.", 10));
                return;
            }

            Session.Character.RemoveLock();
            Session.SendPacket(Session.Character.GenerateSay("Character unlocked.", 10));

        }

        public void ChangeLock(ChangeLockPacket e)
        {
            if (Session.Character.IsLocked)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Unlock your character first"));
                return;
            }

            if (e == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeLockPacket.ReturnHelp(), 10));
                return;
            }

            if (!CanRun(e.Code))
            {
                return;
            }

            if (Session.Account.LockCode != e.Code)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Code is not correct"));
                return;
            }

            Session.Account.LockCode = e.NewCode;
            Session.SendPacket(UserInterfaceHelper.GenerateInfo("Code has been changed to: " + $"{e.NewCode}"));
        }

        public void Lock(LockPacket e)
        {
            if (Session.Character.IsLocked)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Your character is already locked."));
                return;
            }

            if (Session.Account.TwoFactorEnabled)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Please remove 2FA before adding a lock code."));
                return;
            }

            if (e == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(LockPacket.ReturnHelp(), 10));
                return;
            }

            if (!CanRun(e.Code))
            {
                return;
            }

            if (Session.Account.LockCode != e.Code)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Code is not correct"));
                return;
            }

            Session.Character.Lock();
            Session.SendPacket(UserInterfaceHelper.GenerateInfo("Character locked succesfully"));
        }

        public void LockCode(UnlockPacket e)
        {
            if (!Session.Character.IsLocked)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Your character is already unlocked."));
                return;
            }

            if (e == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(UnlockPacket.ReturnHelp(), 10));
                return;
            }

            if (!CanRun(e.Code))
            {
                return;
            }

            if (Session.Account.LockCode != e.Code)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Code is not correct"));
                return;
            }

            Session.Character.RemoveLock();
            Session.SendPacket(UserInterfaceHelper.GenerateInfo("Character unlocked succesfully"));
        }

        public void RemoveLock(RemoveLockPacket e)
        {
            if (Session.Character.IsLocked)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Unlock your character first."));
                return;
            }

            if (e == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(RemoveLockPacket.ReturnHelp(), 10));
                return;
            }

            if (!CanRun(e.Code))
            {
                return;
            }

            if (Session.Account.LockCode != e.Code)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Code is not correct"));
                return;
            }

            Session.Account.LockCode = string.Empty;
            Session.SendPacket(UserInterfaceHelper.GenerateInfo("Lock has been removed"));
        }

        public void SetLockCode(SetLockCodePacket e)
        {
            if (!string.IsNullOrEmpty(Session.Account.LockCode))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("You can't set another code use $ChangeLock <Code> <NewCode>"));
                return;
            }

            if (e == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(SetLockCodePacket.ReturnHelp(), 10));
                return;
            }

            if (e.Code == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(SetLockCodePacket.ReturnHelp(), 10));
                return;
            }

            Session.Account.LockCode = e.Code;
            Session.Character.RemoveLock();
            Session.SendPacket(UserInterfaceHelper.GenerateInfo("LOCK_CODE_SET"));
        }

        private bool CanRun(string e)
        {
            if (string.IsNullOrEmpty(Session.Account.LockCode))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("You don't have any code set"));
                return false;
            }

            if (e == null)
            {
                Session.SendPacket(Session.Character.GenerateSay(UnlockPacket.ReturnHelp(), 10));
                return false;
            }

            return true;
        }

        #endregion

        // review ReturnHelp issue shows "$Unlock <Code>" for every command if null
    }
}