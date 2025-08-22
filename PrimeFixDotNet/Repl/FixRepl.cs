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

using PrimeFixDotNet.Session;
using Serilog;

namespace PrimeFixDotNet.Repl
{
    public class FixRepl
    {
        private static readonly ILogger Logger = Log.ForContext<FixRepl>();
        private const string PROMPT = "FIX> ";

        private readonly PrimeFixApplication _application;
        private readonly CommandHandler _commandHandler;
        private bool _running;

        public FixRepl(PrimeFixApplication application)
        {
            _application = application;
            _commandHandler = new CommandHandler(application);
            _running = false;
        }

        public void Start()
        {
            _running = true;
            Logger.Information("Starting FIX REPL");

            while (_running)
            {
                try
                {
                    Console.Write(PROMPT);
                    string? line = Console.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    ProcessCommand(line.Trim());
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error processing command: {Message}", e.Message);
                }
            }

            Logger.Information("FIX REPL stopped");
        }

        public void Stop()
        {
            _running = false;
        }

        private void ProcessCommand(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToLower();

            switch (command)
            {
                case "new":
                    _commandHandler.HandleNewOrder(parts);
                    break;

                case "status":
                    _commandHandler.HandleOrderStatus(parts);
                    break;

                case "cancel":
                    _commandHandler.HandleOrderCancel(parts);
                    break;

                case "list":
                    _commandHandler.HandleListOrders();
                    break;

                case "version":
                    _commandHandler.HandleVersion();
                    break;

                case "exit":
                case "quit":
                    _running = false;
                    break;

                case "help":
                    ShowHelp();
                    break;

                default:
                    _commandHandler.HandleUnknownCommand(command);
                    break;
            }
        }

        private void ShowHelp()
        {
            string helpText = @"Available commands:
  new <symbol> <MARKET|LIMIT|VWAP> <BUY|SELL> <BASE|QUOTE> <qty> [price] [start_time] [participation_rate] [expire_time]
      Create a new order
  status <ClOrdId> [OrderId] [Side] [Symbol]
      Request order status
  cancel <ClOrdId>
      Cancel an existing order
  list
      List all cached orders
  version
      Show application version
  help
      Show this help message
  exit
      Exit the application

Order Examples:
  Market buy 0.1 BTC: new BTC-USD MARKET BUY BASE 0.1
  Limit buy $1000 of BTC at $30000: new BTC-USD LIMIT BUY QUOTE 1000 30000
  VWAP buy 1 BTC: new BTC-USD VWAP BUY BASE 1.0 50000 2025-08-01T10:00:00Z 0.1 2025-08-01T16:00:00Z";
            
            Logger.Information("{HelpText}", helpText);
        }
    }
}