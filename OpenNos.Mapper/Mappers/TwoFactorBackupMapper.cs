using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class TwoFactorBackupMapper : IMapper<TwoFactorBackupDTO, TwoFactorBackup>
    {
        public TwoFactorBackup Map(TwoFactorBackupDTO input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new TwoFactorBackup
            {
                AccountId = input.AccountId,
                Code = input.Code,
                Id = input.Id
            };

            if (result.Id == Guid.Empty || result.Id == new Guid())
            {
                result.Id = Guid.NewGuid();
            }

            return result;
        }

        public TwoFactorBackupDTO Map(TwoFactorBackup input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new TwoFactorBackupDTO
            {
                AccountId = input.AccountId,
                Code = input.Code,
                Id = input.Id
            };

            if (result.Id == Guid.Empty || result.Id == new Guid())
            {
                result.Id = Guid.NewGuid();
            }

            return result;
        }

        public IEnumerable<TwoFactorBackupDTO> Map(IEnumerable<TwoFactorBackup> input)
        {
            var result = new List<TwoFactorBackupDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<TwoFactorBackup> Map(IEnumerable<TwoFactorBackupDTO> input)
        {
            var result = new List<TwoFactorBackup>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
