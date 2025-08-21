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

namespace PrimeFixDotNet.Model
{
    public class OrderInfo
    {
        public string? ClOrdId { get; set; }

        public string? OrderId { get; set; }

        public string? Side { get; set; }

        public string? Symbol { get; set; }

        public string? Quantity { get; set; }

        public string? LimitPrice { get; set; }

        public string? StartTime { get; set; }

        public string? ExpireTime { get; set; }

        public string? ParticipationRate { get; set; }

        public string? QuantityType { get; set; } // "BASE" or "QUOTE"

        public OrderInfo() { }

        public OrderInfo(string clOrdId, string orderId, string side, string symbol,
                        string quantity, string limitPrice)
        {
            ClOrdId = clOrdId;
            OrderId = orderId;
            Side = side;
            Symbol = symbol;
            Quantity = quantity;
            LimitPrice = limitPrice;
        }

        public OrderInfo(string clOrdId, string orderId, string side, string symbol,
                        string quantity, string limitPrice, string quantityType)
        {
            ClOrdId = clOrdId;
            OrderId = orderId;
            Side = side;
            Symbol = symbol;
            Quantity = quantity;
            LimitPrice = limitPrice;
            QuantityType = quantityType;
        }

        public override string ToString()
        {
            return $"OrderInfo{{clOrdId='{ClOrdId}', orderId='{OrderId}', side='{Side}', symbol='{Symbol}', quantity='{Quantity}'}}";
        }
    }
}