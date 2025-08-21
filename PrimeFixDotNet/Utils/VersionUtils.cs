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

using System.Reflection;
using PrimeFixDotNet.Constants;

namespace PrimeFixDotNet.Utils
{
    public static class VersionUtils
    {
        /// <summary>
        /// Gets the application version from the assembly
        /// </summary>
        public static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString(ApplicationConstants.VERSION_COMPONENT_COUNT) ?? "Unknown";
        }

        /// <summary>
        /// Gets the full application name with version
        /// </summary>
        public static string GetApplicationNameWithVersion()
        {
            return $"C# FIX Client for Coinbase Prime v{GetVersion()}";
        }
    }
}