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
    public static class ApplicationConstants
    {
        // Connection and timeout settings
        public const int CONNECTION_TIMEOUT_SECONDS = 30;
        public const int CONNECTION_RETRY_DELAY_MS = 1000;
        public const int MAX_CONNECTION_ATTEMPTS = 30;

        // Exit codes
        public const int EXIT_SUCCESS = 0;
        public const int EXIT_FAILURE = 1;

        // Command parsing
        public const int MIN_NEW_ORDER_ARGS = 6;
        public const int MIN_LIMIT_ORDER_ARGS = 7;
        public const int MIN_VWAP_ORDER_ARGS = 7;
        public const int MIN_STATUS_REQUEST_ARGS = 2;
        public const int MIN_CANCEL_REQUEST_ARGS = 2;
        
        // Command argument indices
        public const int CMD_INDEX = 0;
        public const int SYMBOL_INDEX = 1;
        public const int ORDER_TYPE_INDEX = 2;
        public const int SIDE_INDEX = 3;
        public const int QTY_TYPE_INDEX = 4;
        public const int QUANTITY_INDEX = 5;
        public const int PRICE_INDEX = 6;
        public const int VWAP_PARAMS_START_INDEX = 7;
        
        // Status request argument indices
        public const int STATUS_CL_ORD_ID_INDEX = 1;
        public const int STATUS_ORDER_ID_INDEX = 2;
        public const int STATUS_SIDE_INDEX = 3;
        public const int STATUS_SYMBOL_INDEX = 4;
        
        // Cancel request argument indices
        public const int CANCEL_CL_ORD_ID_INDEX = 1;

        // FIX protocol defaults
        public const string LOGON_SEQUENCE_NUMBER = "1";
        public const string DEFAULT_QUANTITY = "0";
        
        // File paths
        public const string ORDER_CACHE_FILE = "orders.json";
        
        // Logging configuration
        public const string DEFAULT_LOG_LEVEL = "Information";

        // Display formatting
        public const int ORDER_LIST_ID_COLUMN_WIDTH = -20;
        
        // Version formatting
        public const int VERSION_COMPONENT_COUNT = 3;
    }
}