using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Authenticator;

namespace OpenNos.Core.TwoFactorAuth
{
    public class GoogleAuthService : IAuthenticationService
    {
        public string GenerateSetupCode(string applicationName, string additionnalInfos, string key)
        {
            var tfa = new TwoFactorAuthenticator();
            return tfa.GenerateSetupCode(applicationName, additionnalInfos, key, false, 3).ManualEntryKey;
        }

        public bool ValidatePinCode(string key, string code)
        {
            var tfa = new TwoFactorAuthenticator();
            return tfa.ValidateTwoFactorPIN(key, code);
        }

    }
}
