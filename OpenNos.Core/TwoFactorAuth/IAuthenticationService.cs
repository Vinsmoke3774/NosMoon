using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core.TwoFactorAuth
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Validate or reject the provided code
        /// </summary>
        /// <param name="key"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        bool ValidatePinCode(string key, string code);

        /// <summary>
        /// Generates a setup code and a QrCode if defined
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="additionnalInfos"></param>
        /// <param name="key"></param>
        /// <param name="qrWidth"></param>
        /// <param name="qrHeight"></param>
        /// <returns></returns>
        string GenerateSetupCode(string applicationName, string additionnalInfos, string key);
    }
}
