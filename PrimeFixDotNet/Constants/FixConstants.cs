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

using QuickFix;
using QuickFix.Fields;

namespace PrimeFixDotNet.Constants
{
    public static class FixConstants
    {

        // Time Format
        public const string FIX_TIME_FORMAT = "yyyyMMdd-HH:mm:ss.fff";

        // Default Values
        public const string DEFAULT_TARGET_COMP_ID = "COIN";

        // Order Types (User-facing)
        public const string ORD_TYPE_LIMIT = "LIMIT";
        public const string ORD_TYPE_MARKET = "MARKET";
        public const string ORD_TYPE_VWAP = "VWAP";

        // Sides (User-facing)
        public const string SIDE_BUY = "BUY";
        public const string SIDE_SELL = "SELL";

        // Target Strategy
        public const string TARGET_STRATEGY_LIMIT = "L";   // Limit strategy
        public const string TARGET_STRATEGY_MARKET = "M";  // Market strategy
        public const string TARGET_STRATEGY_VWAP = "V";    // VWAP strategy

        // Quantity Types
        public const string QTY_TYPE_BASE = "BASE";
        public const string QTY_TYPE_QUOTE = "QUOTE";

        public const int TAG_HMAC = 96;                      // Custom HMAC field (non-standard)
        public const int TAG_DROP_COPY_FLAG = 9406;          // Custom drop copy flag (non-standard)
        public const int TAG_ACCESS_KEY = 9407;              // Custom access key (non-standard)
    }
}