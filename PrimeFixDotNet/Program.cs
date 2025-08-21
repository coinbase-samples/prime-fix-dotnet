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

using PrimeFixDotNet.Constants;
using PrimeFixDotNet.Model;
using PrimeFixDotNet.Repl;
using PrimeFixDotNet.Session;
using PrimeFixDotNet.Utils;
using QuickFix;
using QuickFix.Transport;
using QuickFix.Store;
using QuickFix.Logger;
using Serilog;

namespace PrimeFixDotNet
{
    internal class Program
    {
        private const string DEFAULT_CONFIG_FILE = "fix.cfg";

        static async Task Main(string[] args)
        {
            Console.WriteLine(VersionUtils.GetApplicationNameWithVersion());
            Console.WriteLine();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                // Load configuration from environment variables
                var config = PrimeFixConfig.FromEnvironment();
                string configFile = args.Length > 0 ? args[0] : DEFAULT_CONFIG_FILE;

                // Create application with configuration object
                using var application = new PrimeFixApplication(config);

                var settings = new SessionSettings(configFile);
                var storeFactory = new FileStoreFactory(settings);
                var logFactory = new FileLogFactory(settings);
                var messageFactory = new DefaultMessageFactory();

                var initiator = new SocketInitiator(
                    application, storeFactory, settings, logFactory, messageFactory
                );

                Log.Information("Starting FIX initiator");
                initiator.Start();

                // Create REPL for order management
                var repl = new FixRepl(application);

                // Set up shutdown handler
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Log.Information("Shutting down FIX client");
                    repl.Stop();
                    initiator.Stop();
                    Environment.Exit(ApplicationConstants.EXIT_SUCCESS);
                };

                Console.WriteLine("Waiting for FIX connection...");

                // Verify connection with detailed status
                bool connected = false;
                int attempts = 0;
                while (!connected && attempts < ApplicationConstants.MAX_CONNECTION_ATTEMPTS)
                {
                    await Task.Delay(ApplicationConstants.CONNECTION_RETRY_DELAY_MS);
                    attempts++;
                    
                    if (application.SessionId != null)
                    {
                        var session = QuickFix.Session.LookupSession(application.SessionId);
                        if (session?.IsLoggedOn == true)
                        {
                            Console.WriteLine("Connected and logged on to Coinbase Prime");
                            Console.WriteLine($"Session ID: {application.SessionId}");
                            Console.WriteLine($"Sequence numbers - Sent: {session.NextSenderMsgSeqNum - 1}, Received: {session.NextTargetMsgSeqNum - 1}");
                            connected = true;
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Attempt {attempts}/{ApplicationConstants.MAX_CONNECTION_ATTEMPTS}: Session exists but not logged on yet...");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Attempt {attempts}/{ApplicationConstants.MAX_CONNECTION_ATTEMPTS}: No session yet...");
                    }
                }

                if (!connected)
                {
                    Console.WriteLine($"Connection failed after {ApplicationConstants.CONNECTION_TIMEOUT_SECONDS} seconds");
                    Environment.Exit(ApplicationConstants.EXIT_FAILURE);
                }

                // Start REPL for interactive order management
                Console.WriteLine("Connection verified! Starting interactive REPL...");
                repl.Start();

            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to start FIX client");
                Console.Error.WriteLine($"Error: {e.Message}");
                Environment.Exit(ApplicationConstants.EXIT_FAILURE);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

    }
}