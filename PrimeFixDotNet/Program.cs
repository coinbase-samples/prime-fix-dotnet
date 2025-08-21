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

using PrimeFixDotNet.Repl;
using PrimeFixDotNet.Session;
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
            Console.WriteLine("C# FIX Client for Coinbase Prime v1.0.0");
            Console.WriteLine();

            // Configure Serilog (simple console logging like Java)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                // Get credentials from environment variables (same as Java)
                string accessKey = GetRequiredEnv("ACCESS_KEY");
                string signingKey = GetRequiredEnv("SIGNING_KEY");
                string passphrase = GetRequiredEnv("PASSPHRASE");
                string svcAccountId = GetRequiredEnv("SVC_ACCOUNT_ID");
                string targetCompId = GetEnvOrDefault("TARGET_COMP_ID", "COIN");
                string portfolioId = GetRequiredEnv("PORTFOLIO_ID");

                string configFile = args.Length > 0 ? args[0] : DEFAULT_CONFIG_FILE;

                // Create application (direct port of Java)
                using var application = new PrimeFixApplication(
                    accessKey, signingKey, passphrase,
                    svcAccountId, targetCompId, portfolioId
                );

                // Initialize QuickFIX/N components (direct port of Java setup)
                var settings = new SessionSettings(configFile);
                var storeFactory = new FileStoreFactory(settings);
                var logFactory = new FileLogFactory(settings); // Note: Java uses RawLogFactory
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
                    Environment.Exit(0);
                };

                Console.WriteLine("Waiting for FIX connection...");

                // Verify connection with detailed status
                bool connected = false;
                int attempts = 0;
                while (!connected && attempts < 30) // 30 second timeout
                {
                    await Task.Delay(1000);
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
                            Console.WriteLine($"Attempt {attempts}/30: Session exists but not logged on yet...");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Attempt {attempts}/30: No session yet...");
                    }
                }

                if (!connected)
                {
                    Console.WriteLine("Connection failed after 30 seconds");
                    Environment.Exit(1);
                }

                // Start REPL for interactive order management
                Console.WriteLine("Connection verified! Starting interactive REPL...");
                repl.Start();

            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to start FIX client");
                Console.Error.WriteLine($"Error: {e.Message}");
                Environment.Exit(1);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static string GetRequiredEnv(string name)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                Console.Error.WriteLine($"Error: Required environment variable {name} is not set");
                Console.Error.WriteLine("Required environment variables:");
                Console.Error.WriteLine("  ACCESS_KEY - Your API access key");
                Console.Error.WriteLine("  SIGNING_KEY - Your API secret key");
                Console.Error.WriteLine("  PASSPHRASE - Your API passphrase");
                Console.Error.WriteLine("  SVC_ACCOUNT_ID - Your service account ID");
                Console.Error.WriteLine("  PORTFOLIO_ID - Your portfolio ID");
                Console.Error.WriteLine("Optional environment variables:");
                Console.Error.WriteLine("  TARGET_COMP_ID - Target company ID (default: COIN)");
                Environment.Exit(1);
            }
            return value.Trim();
        }

        private static string GetEnvOrDefault(string name, string defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(name);
            return (!string.IsNullOrWhiteSpace(value)) ? value.Trim() : defaultValue;
        }
    }
}