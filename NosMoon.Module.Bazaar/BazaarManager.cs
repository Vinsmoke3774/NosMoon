﻿using Microsoft.Extensions.Hosting;
using OpenNos.Core.Threading;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NosMoon.Module.Bazaar
{
    public class BazaarManager
    {
        public BazaarManager()
        {
            BazaarItems = new ThreadSafeLockedDictionary<long, BazaarItemDTO>();
            BazaarItemLinks = new ThreadSafeLockedDictionary<long, BazaarItemLink>();
            BazaarItemStates = new ConcurrentBag<long>();
        }

        public ThreadSafeLockedDictionary<long, BazaarItemDTO> BazaarItems { get; set; }

        public ThreadSafeLockedDictionary<long, BazaarItemLink> BazaarItemLinks { get; set; }

        public ConcurrentBag<long> BazaarItemStates { get; set; }

        public async Task Initialize()
        {
            await LoadBazaarItemsAsync();
        }

        public async Task LoadBazaarItemsAsync()
        {
            ServerManager.Instance.LoadItems();

            var bazaarItems = DAOFactory.BazaarItemDAO.LoadAll();

            if (bazaarItems?.Any() != true)
            {
                Console.WriteLine("No bazaar items loaded.");
                return;
            }

            var dictionary = bazaarItems.ToDictionary(x => x.BazaarItemId, y => y);
            BazaarItems = new ThreadSafeLockedDictionary<long, BazaarItemDTO>(dictionary);
            Console.WriteLine($"{BazaarItems.Count} Bazaar Items loaded.");

            var partitioner = Partitioner.Create(bazaarItems, EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(partitioner, new ParallelOptions { MaxDegreeOfParallelism = 8 }, bz =>
            {
                BazaarItemLinks.TryAdd(bz.BazaarItemId, new BazaarItemLink
                {
                    BazaarItem = bz,
                    Item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bz.ItemInstanceId)),
                    Owner = DAOFactory.CharacterDAO.LoadById(bz.SellerId)?.Name
                });
            });

            Console.WriteLine($"{BazaarItemLinks.Count} Bazaar item links created.");
        }
    }
}
