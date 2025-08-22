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
using PrimeFixDotNet.Utils;
using QuickFix;
using QuickFix.Fields;
using System.Globalization;

namespace PrimeFixDotNet.Builder
{
    public static class MessageBuilder
    {
        public static void BuildLogon(Message message, string timestamp, string apiKey,
                                    string apiSecret, string passphrase, string targetCompId,
                                    string portfolioId)
        {
            string signature = FixUtils.Sign(timestamp, MsgType.LOGON, ApplicationConstants.LOGON_SEQUENCE_NUMBER,
                                           apiKey, targetCompId, passphrase, apiSecret);

            message.SetField(new Account(portfolioId));
            message.SetField(new StringField(FixConstants.TAG_HMAC, signature));
            message.SetField(new Password(passphrase));
            message.SetField(new StringField(FixConstants.TAG_DROP_COPY_FLAG, "Y"));
            message.SetField(new StringField(FixConstants.TAG_ACCESS_KEY, apiKey));
        }

        public static Message BuildNewOrderSingle(string symbol, string ordType, string side,
                                                string qtyType, string qty,
                                                string portfolioId, string senderCompId,
                                                string targetCompId, string? price = null,
                                                params string[] vwapParams)
        {
            var message = new Message();

            message.Header.SetField(new MsgType(MsgType.ORDER_SINGLE));
            message.Header.SetField(new SenderCompID(senderCompId));
            message.Header.SetField(new TargetCompID(targetCompId));
            message.Header.SetField(new SendingTime());

            string clOrdId = FixUtils.GenerateClOrdId();

            message.SetField(new Account(portfolioId));
            message.SetField(new ClOrdID(clOrdId));
            message.SetField(new Symbol(symbol));

            // Set quantity based on user preference (BASE or QUOTE)
            if (FixConstants.QTY_TYPE_BASE.Equals(qtyType, StringComparison.OrdinalIgnoreCase))
            {
                message.SetField(new OrderQty(decimal.Parse(qty, CultureInfo.InvariantCulture)));
            }
            else // Default to QUOTE
            {
                message.SetField(new CashOrderQty(decimal.Parse(qty, CultureInfo.InvariantCulture)));
            }

            if (FixConstants.ORD_TYPE_LIMIT.Equals(ordType, StringComparison.OrdinalIgnoreCase))
            {
                message.SetField(new OrdType(OrdType.LIMIT));
                message.SetField(new TimeInForce(TimeInForce.GOOD_TILL_CANCEL));
                if (price != null)
                {
                    message.SetField(new Price(decimal.Parse(price, CultureInfo.InvariantCulture)));
                }
                message.SetField(new StringField(Tags.TargetStrategy, FixConstants.TARGET_STRATEGY_LIMIT));
            }
            else if (FixConstants.ORD_TYPE_VWAP.Equals(ordType, StringComparison.OrdinalIgnoreCase))
            {
                message.SetField(new OrdType(OrdType.LIMIT));
                message.SetField(new TimeInForce(TimeInForce.GOOD_TILL_DATE));
                if (price != null)
                {
                    message.SetField(new Price(decimal.Parse(price, CultureInfo.InvariantCulture)));
                }
                message.SetField(new StringField(Tags.TargetStrategy, FixConstants.TARGET_STRATEGY_VWAP));

                // Handle VWAP parameters
                if (vwapParams.Length > 0 && !string.IsNullOrEmpty(vwapParams[0]))
                {
                    message.SetField(new StringField(Tags.EffectiveTime, vwapParams[0]));
                }

                if (vwapParams.Length > 1 && !string.IsNullOrEmpty(vwapParams[1]))
                {
                    message.SetField(new StringField(Tags.ParticipationRate, vwapParams[1]));
                }

                if (vwapParams.Length > 2 && !string.IsNullOrEmpty(vwapParams[2]))
                {
                    message.SetField(new StringField(Tags.ExpireTime, vwapParams[2]));
                }
            }
            else // Market order
            {
                message.SetField(new OrdType(OrdType.MARKET));
                message.SetField(new TimeInForce(TimeInForce.IMMEDIATE_OR_CANCEL));
                message.SetField(new StringField(Tags.TargetStrategy, FixConstants.TARGET_STRATEGY_MARKET));
            }

            if (FixConstants.SIDE_BUY.Equals(side, StringComparison.OrdinalIgnoreCase))
            {
                message.SetField(new Side(Side.BUY));
            }
            else
            {
                message.SetField(new Side(Side.SELL));
            }

            return message;
        }

        public static Message BuildOrderStatusRequest(string clOrdId, string orderId,
                                                    string side, string symbol,
                                                    string senderCompId, string targetCompId)
        {
            var message = new Message();

            message.Header.SetField(new MsgType(MsgType.ORDER_STATUS_REQUEST));
            message.Header.SetField(new SenderCompID(senderCompId));
            message.Header.SetField(new TargetCompID(targetCompId));
            message.Header.SetField(new SendingTime());

            message.SetField(new ClOrdID(clOrdId));
            message.SetField(new OrderID(orderId));
            
            // Convert readable side to FIX protocol codes
            char fixSide = FixConstants.SIDE_BUY.Equals(side, StringComparison.OrdinalIgnoreCase) 
                          ? Side.BUY 
                          : Side.SELL;
            message.SetField(new Side(fixSide));
            message.SetField(new Symbol(symbol));

            return message;
        }

        public static Message BuildOrderCancelRequest(OrderInfo orderInfo, string portfolioId,
                                                    string senderCompId, string targetCompId)
        {
            var message = new Message();

            message.Header.SetField(new MsgType(MsgType.ORDER_CANCEL_REQUEST));
            message.Header.SetField(new SenderCompID(senderCompId));
            message.Header.SetField(new TargetCompID(targetCompId));
            message.Header.SetField(new SendingTime());

            string cancelClOrdId = FixUtils.GenerateCancelClOrdId();

            message.SetField(new Account(portfolioId));
            message.SetField(new ClOrdID(cancelClOrdId));
            message.SetField(new OrigClOrdID(orderInfo.ClOrdId ?? ""));
            message.SetField(new OrderID(orderInfo.OrderId ?? ""));
            
            // Use the same quantity type as the original order
            decimal quantity = decimal.Parse(orderInfo.Quantity ?? ApplicationConstants.DEFAULT_QUANTITY, CultureInfo.InvariantCulture);
            if (FixConstants.QTY_TYPE_QUOTE.Equals(orderInfo.QuantityType, StringComparison.OrdinalIgnoreCase))
            {
                message.SetField(new CashOrderQty(quantity)); // Tag 152
            }
            else
            {
                message.SetField(new OrderQty(quantity)); // Tag 38 (default for BASE)
            }
            
            // Convert readable side to FIX protocol codes
            char fixSide = FixConstants.SIDE_BUY.Equals(orderInfo.Side, StringComparison.OrdinalIgnoreCase) 
                          ? Side.BUY 
                          : Side.SELL;
            message.SetField(new Side(fixSide));
            message.SetField(new Symbol(orderInfo.Symbol ?? ""));

            return message;
        }
    }
}