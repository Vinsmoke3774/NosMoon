using System.Linq;
using System.Text.RegularExpressions;

namespace OpenNos.Core.Extensions
{
    public static class StringExtensions
    {
        public static string CleanIpAddress(this string ip)
        {
            if (!ip.Contains(':'))
            {
                return ip;
            }

            string cleanIp = ip.Replace("tcp://", "");
            return cleanIp.Substring(0, cleanIp.LastIndexOf(":") > 0 ? cleanIp.LastIndexOf(":") : cleanIp.Length);
        }

        public static string TrimUntil(this string str, char flag) => str.Substring(str.TakeWhile(c => c != flag).Count());

        public static string SplitCamelCase(this string source)
        {
            return string.Join(" ", Regex.Split(source, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
        }
    }
}
