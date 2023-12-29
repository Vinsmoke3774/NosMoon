using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNos.DAL;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Comparers;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;

namespace NosByte.Tests
{
    [TestClass]
    public class DatabaseTests
    {
        private readonly IMapper<MimicRotationDTO, MimicRotation> _mapper = new MimicRotationMapper();

        [TestMethod]
        public void CheckDupedCellons()
        {
            DataAccessHelper.Initialize();

            using var context = DataAccessHelper.CreateContext();
            var entities = context.CellonOption.ToList();
            var cellons = new HashSet<CellonOption>(entities, new CellonOptionComparer());
            context.CellonOption.BulkDelete(entities);
            context.CellonOption.BulkInsert(cellons);
        }

        [TestMethod]
        public void Sexe()
        {
            List<double> doubleList = new List<double>();

            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 100000000; i++)
            {
                var random = ServerManager.RandomDouble();

                doubleList.Add(random);
            }

            watch.Stop();

            var sec = watch.Elapsed.Seconds;
            var avg = doubleList.Average();
        }

        [TestMethod]
        public void WhitelistIps()
        {
            using var context = DataAccessHelper.CreateContext();
            var logs = context.GeneralLog.Where(s => (s.Timestamp.Year == 2021) && s.LogData == "LoginServer" && s.LogType == "Connection")?.ToList();

            var ips = logs.Select(s => s.IpAddress)?.Distinct();

            var rule = FirewallManager.Instance.Rules.FirstOrDefault(s => s.Name == "whitelist1");

            if (rule == null)
            {
                return;
            }

            var ruleContent = rule.RemoteAddresses.ToList(); // Fetch ip addresses
            ruleContent.Clear(); // Clear all to avoid exceptions
            rule.RemoteAddresses = ruleContent.ToArray();

            int i = 0;
            foreach (var ip in ips)
            {
                var success = IPAddress.TryParse(ip, out var addressBytes);

                if (!success)
                {
                    continue;
                }

                ruleContent.Add(new SingleIP(addressBytes.GetAddressBytes()));
                i++;
            }

            rule.RemoteAddresses = ruleContent.ToArray();
        }
    }
}
