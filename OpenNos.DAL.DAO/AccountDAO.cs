﻿using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Domain;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class AccountDAO : IAccountDAO
    {
        #region Methods

        public bool ContainsAccounts()
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    if (context.Account.FirstOrDefault() != null)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                throw;
            }
        }

        public void Insert(List<AccountDTO> account)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    context.Configuration.AutoDetectChangesEnabled = false;
                    foreach (var Account in account)
                    {
                        var entity = new Account();
                        AccountMapper.ToAccount(Account, entity);
                        context.Account.Add(entity);
                    }

                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        public DeleteResult Delete(long accountId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var account = context.Account.FirstOrDefault(c => c.AccountId.Equals(accountId));

                    if (account != null)
                    {
                        context.Account.Remove(account);
                        context.SaveChanges();
                    }

                    return DeleteResult.Deleted;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(
                    string.Format(Language.Instance.GetMessageFromKey("DELETE_ACCOUNT_ERROR"), accountId, e.Message),
                    e);
                return DeleteResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(ref AccountDTO account)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var accountId = account.AccountId;
                var entity = context.Account.FirstOrDefault(c => c.AccountId.Equals(accountId));

                if (entity == null)
                {
                    account = insert(account, context);
                    return SaveResult.Inserted;
                }

                account = update(entity, account, context);
                return SaveResult.Updated;
            }
            catch (Exception e)
            {
                Logger.Log.Error(
                    string.Format(Language.Instance.GetMessageFromKey("UPDATE_ACCOUNT_ERROR"), account.AccountId,
                        e.Message), e);
                return SaveResult.Error;
            }
        }

        public AccountDTO LoadById(long accountId)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var account = context.Account.FirstOrDefault(a => a.AccountId.Equals(accountId));
                    if (account != null)
                    {
                        var accountDTO = new AccountDTO();
                        if (AccountMapper.ToAccountDTO(account, accountDTO))
                        {
                            return accountDTO;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            return null;
        }

        public AccountDTO LoadByName(string name)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var account = context.Account.FirstOrDefault(a => a.Name.Equals(name));
                    if (account != null)
                    {
                        var accountDTO = new AccountDTO();
                        if (AccountMapper.ToAccountDTO(account, accountDTO))
                        {
                            return accountDTO;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            return null;
        }

        // public int AmountDailyRewardsGiven(string registrationIP)
        // {
        //     try
        //     {
        //         using (var context = DataAccessHelper.CreateContext())
        //         {
        //             var account = context.Account.Count(a => a.RegistrationIP.Equals(registrationIP));
        //             if (account != null)
        //             {
        //                 var accountDTO = new AccountDTO();
        //                 if (AccountMapper.ToAccountDTO(account, accountDTO))
        //                 {
        //                     return accountDTO;
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Logger.Error(e);
        //     }
        //
        //     return 0;
        // }

        public void WriteGeneralLog(long accountId, string ipAddress, long? characterId, GeneralLogType logType,
            string logData)
        {
            try
            {
                using (var context = DataAccessHelper.CreateContext())
                {
                    var log = new GeneralLog
                    {
                        AccountId = accountId,
                        IpAddress = ipAddress,
                        Timestamp = DateTime.Now,
                        LogType = logType.ToString(),
                        LogData = logData,
                        CharacterId = characterId
                    };

                    context.GeneralLog.Add(log);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }
        }

        private static AccountDTO insert(AccountDTO account, OpenNosContext context)
        {
            var entity = new Account();
            AccountMapper.ToAccount(account, entity);
            context.Account.Add(entity);
            context.SaveChanges();
            AccountMapper.ToAccountDTO(entity, account);
            return account;
        }

        private static AccountDTO update(Account entity, AccountDTO account, OpenNosContext context)
        {
            if (entity != null)
            {
                AccountMapper.ToAccount(account, entity);
                context.Entry(entity).State = EntityState.Modified;
                context.SaveChanges();
            }

            if (AccountMapper.ToAccountDTO(entity, account))
            {
                return account;
            }

            return null;
        }


        #endregion
    }
}