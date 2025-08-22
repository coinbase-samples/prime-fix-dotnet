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

using System.Security.Cryptography;
using System.Text;
using QuickFix;

namespace PrimeFixDotNet.Utils
{
    public static class FixUtils
    {
        public static string GetString(Message message, int tag)
        {
            try
            {
                return message.GetString(tag);
            }
            catch (FieldNotFoundException)
            {
                return "";
            }
        }

        public static string Sign(string timestamp, string msgType, string sequence,
                                 string apiKey, string targetCompId, string passphrase,
                                 string apiSecret)
        {
            try
            {
                string message = $"{timestamp}{msgType}{sequence}{apiKey}{targetCompId}{passphrase}";

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(hash);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to generate HMAC signature", ex);
            }
        }

        public static string GenerateClOrdId()
        {
            return DateTimeOffset.UtcNow.Ticks.ToString();
        }

        public static string GenerateCancelClOrdId()
        {
            return $"cancel-{DateTimeOffset.UtcNow.Ticks}";
        }

        public static string GetOptional(string[] parts, int index)
        {
            return parts.Length > index ? parts[index] : "";
        }

        public static bool IsValidNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return double.TryParse(value, out _);
        }
    }
}