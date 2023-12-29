using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using System.Collections.Generic;

namespace NosTale.Parser.Import
{
    public class ImportAccounts : IImport
    {
        public void Import()
        {
            var accounts = new List<AccountDTO>();
            if (!DAOFactory.AccountDAO.ContainsAccounts())
            {
                accounts.Add(new AccountDTO
                {
                    AccountId = 1,
                    Authority = AuthorityType.Administrator,
                    Name = "admin",
                    Password = CryptographyBase.Sha512("test")
                });
                accounts.Add(new AccountDTO
                {
                    AccountId = 2,
                    Authority = AuthorityType.User,
                    Name = "test",
                    Password = CryptographyBase.Sha512("test")
                });
            }

            ;

            DAOFactory.AccountDAO.Insert(accounts);
            Logger.Log.Info($"{accounts.Count} Accounts parsed");
        }
    }
}