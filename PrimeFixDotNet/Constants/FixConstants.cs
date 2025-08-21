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

namespace PrimeFixDotNet.Constants
{
    public static class FixConstants
    {
        // Message Types
        public const string MSG_TYPE_NEW = "D";        // New Order
        public const string MSG_TYPE_STATUS = "H";     // Status
        public const string MSG_TYPE_CANCEL = "F";     // Cancel
        public const string MSG_TYPE_LOGON = "A";      // Logon
        public const string MSG_TYPE_EXEC_REPORT = "8"; // Execution Report

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

        // Order Types (FIX Protocol)
        public const char ORD_TYPE_LIMIT_FIX = '2';      // Limit
        public const char ORD_TYPE_MARKET_FIX = '1';     // Market
        public const char ORD_TYPE_VWAP_FIX = '2';       // VWAP uses limit type

        // Time In Force
        public const char TIME_IN_FORCE_DAY = '0';                    // Day
        public const char TIME_IN_FORCE_GTC = '1';                   // Good Till Cancel
        public const char TIME_IN_FORCE_IOC = '3';                   // Immediate or Cancel
        public const char TIME_IN_FORCE_GTD = '6';                   // Good Till Date

        // Target Strategy
        public const string TARGET_STRATEGY_LIMIT = "L";   // Limit strategy
        public const string TARGET_STRATEGY_MARKET = "M";  // Market strategy
        public const string TARGET_STRATEGY_VWAP = "V";    // VWAP strategy

        // Sides (FIX Protocol)
        public const char SIDE_BUY_FIX = '1';   // Buy
        public const char SIDE_SELL_FIX = '2';  // Sell

        // Quantity Types
        public const string QTY_TYPE_BASE = "BASE";
        public const string QTY_TYPE_QUOTE = "QUOTE";

        // FIX Tags
        public const int TAG_ACCOUNT = 1;                    // Account
        public const int TAG_CL_ORD_ID = 11;                 // ClOrdID
        public const int TAG_ORDER_ID = 37;                  // OrderID
        public const int TAG_ORDER_QTY = 38;                 // OrderQty
        public const int TAG_ORD_TYPE = 40;                  // OrdType
        public const int TAG_ORIG_CL_ORD_ID = 41;            // OrigClOrdID
        public const int TAG_PRICE = 44;                     // Price
        public const int TAG_SENDER_COMP_ID = 49;            // SenderCompID
        public const int TAG_SENDING_TIME = 52;              // SendingTime
        public const int TAG_SIDE = 54;                      // Side
        public const int TAG_SYMBOL = 55;                    // Symbol
        public const int TAG_TARGET_COMP_ID = 56;            // TargetCompID
        public const int TAG_TIME_IN_FORCE = 59;             // TimeInForce
        public const int TAG_HMAC = 96;                      // Custom HMAC field
        public const int TAG_MSG_TYPE = 35;                  // MsgType
        public const int TAG_EXEC_TYPE = 150;                // ExecType
        public const int TAG_CASH_ORDER_QTY = 152;           // CashOrderQty
        public const int TAG_PASSWORD = 554;                 // Password
        public const int TAG_DROP_COPY_FLAG = 9406;          // Custom drop copy flag
        public const int TAG_ACCESS_KEY = 9407;              // Custom access key
        public const int TAG_START_TIME = 168;               // EffectiveTime
        public const int TAG_EXPIRE_TIME = 126;              // ExpireTime
        public const int TAG_PARTICIPATION_RATE = 849;       // ParticipationRate
        public const int TAG_TARGET_STRATEGY = 847;          // TargetStrategy
    }
}