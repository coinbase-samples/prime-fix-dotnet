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

namespace PrimeFixDotNet.Model
{
    /// <summary>
    /// Configuration object for Coinbase Prime FIX client
    /// </summary>
    public class PrimeFixConfig
    {
        public required string AccessKey { get; init; }
        public required string SigningKey { get; init; }
        public required string Passphrase { get; init; }
        public required string SvcAccountId { get; init; }
        public required string PortfolioId { get; init; }
        public string TargetCompId { get; init; } = FixConstants.DEFAULT_TARGET_COMP_ID;

        /// <summary>
        /// Creates configuration from environment variables
        /// </summary>
        public static PrimeFixConfig FromEnvironment()
        {
            return new PrimeFixConfig
            {
                AccessKey = GetRequiredEnv("ACCESS_KEY"),
                SigningKey = GetRequiredEnv("SIGNING_KEY"),
                Passphrase = GetRequiredEnv("PASSPHRASE"),
                SvcAccountId = GetRequiredEnv("SVC_ACCOUNT_ID"),
                PortfolioId = GetRequiredEnv("PORTFOLIO_ID"),
                TargetCompId = GetEnvOrDefault("TARGET_COMP_ID", FixConstants.DEFAULT_TARGET_COMP_ID)
            };
        }

        /// <summary>
        /// Validates that all required configuration is present
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(AccessKey))
                throw new InvalidOperationException("ACCESS_KEY is required");
            if (string.IsNullOrWhiteSpace(SigningKey))
                throw new InvalidOperationException("SIGNING_KEY is required");
            if (string.IsNullOrWhiteSpace(Passphrase))
                throw new InvalidOperationException("PASSPHRASE is required");
            if (string.IsNullOrWhiteSpace(SvcAccountId))
                throw new InvalidOperationException("SVC_ACCOUNT_ID is required");
            if (string.IsNullOrWhiteSpace(PortfolioId))
                throw new InvalidOperationException("PORTFOLIO_ID is required");
            if (string.IsNullOrWhiteSpace(TargetCompId))
                throw new InvalidOperationException("TARGET_COMP_ID cannot be empty");
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
                Environment.Exit(ApplicationConstants.EXIT_FAILURE);
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