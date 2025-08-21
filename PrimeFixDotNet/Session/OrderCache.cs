/*
 * Copyright 2025-present Coinbase Global, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Concurrent;
using System.Text.Json;
using PrimeFixDotNet.Model;
using Serilog;

namespace PrimeFixDotNet.Session
{
    public class OrderCache
    {
        private static readonly ILogger Logger = Log.ForContext<OrderCache>();
        private const string ORDER_FILE = "orders.json";

        private readonly JsonSerializerOptions _jsonOptions;

        public OrderCache()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Save orders to JSON file
        /// </summary>
        public void SaveOrders(ConcurrentDictionary<string, OrderInfo> orders)
        {
            try
            {
                var json = JsonSerializer.Serialize(orders, _jsonOptions);
                File.WriteAllText(ORDER_FILE, json);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to save orders to file: {Message}", e.Message);
            }
        }

        /// <summary>
        /// Load orders from JSON file
        /// </summary>
        public void LoadOrders(ConcurrentDictionary<string, OrderInfo> orders)
        {
            if (!File.Exists(ORDER_FILE))
            {
                Logger.Information("Orders file does not exist, starting with empty cache");
                return;
            }

            try
            {
                var json = File.ReadAllText(ORDER_FILE);
                var loadedOrders = JsonSerializer.Deserialize<Dictionary<string, OrderInfo>>(json, _jsonOptions);
                
                if (loadedOrders != null)
                {
                    orders.Clear();
                    foreach (var kvp in loadedOrders)
                    {
                        orders.TryAdd(kvp.Key, kvp.Value);
                    }
                    Logger.Information("Loaded {OrderCount} orders from cache", orders.Count);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to load orders from file: {Message}", e.Message);
                throw;
            }
        }
    }
}