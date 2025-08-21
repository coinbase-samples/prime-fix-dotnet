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

using System.Collections.Concurrent;
using PrimeFixDotNet.Builder;
using PrimeFixDotNet.Constants;
using PrimeFixDotNet.Model;
using PrimeFixDotNet.Session;
using PrimeFixDotNet.Utils;
using QuickFix;

namespace PrimeFixDotNet.Repl
{
    public class CommandHandler
    {
        private readonly PrimeFixApplication _application;

        public CommandHandler(PrimeFixApplication application)
        {
            _application = application;
        }

        public void HandleNewOrder(string[] parts)
        {
            if (parts.Length < ApplicationConstants.MIN_NEW_ORDER_ARGS)
            {
                Console.WriteLine("error: insufficient arguments");
                Console.WriteLine("usage: new <symbol> <MARKET|LIMIT|VWAP> <BUY|SELL> <BASE|QUOTE> <qty> [price] [start_time] [participation_rate] [expire_time]");
                return;
            }

            string symbol = parts[ApplicationConstants.SYMBOL_INDEX];
            string ordType = parts[ApplicationConstants.ORDER_TYPE_INDEX].ToUpper();
            string side = parts[ApplicationConstants.SIDE_INDEX].ToUpper();
            string qtyType = parts[ApplicationConstants.QTY_TYPE_INDEX].ToUpper();
            string qty = parts[ApplicationConstants.QUANTITY_INDEX];
            string? price = null;

            if (!FixConstants.SIDE_BUY.Equals(side) && !FixConstants.SIDE_SELL.Equals(side))
            {
                Console.WriteLine("error: side must be BUY or SELL");
                return;
            }

            if (!FixConstants.QTY_TYPE_BASE.Equals(qtyType) && !FixConstants.QTY_TYPE_QUOTE.Equals(qtyType))
            {
                Console.WriteLine("error: quantity type must be BASE or QUOTE");
                return;
            }

            if (!FixUtils.IsValidNumber(qty))
            {
                Console.WriteLine("error: qty must be a valid number");
                return;
            }

            switch (ordType)
            {
                case FixConstants.ORD_TYPE_MARKET:
                    if (parts.Length > ApplicationConstants.MIN_LIMIT_ORDER_ARGS)
                    {
                        Console.WriteLine("error: MARKET orders should not include a price");
                        return;
                    }
                    break;

                case FixConstants.ORD_TYPE_LIMIT:
                    if (parts.Length < ApplicationConstants.MIN_LIMIT_ORDER_ARGS)
                    {
                        Console.WriteLine("error: price must be specified for LIMIT orders");
                        return;
                    }
                    price = parts[ApplicationConstants.PRICE_INDEX];
                    if (!FixUtils.IsValidNumber(price))
                    {
                        Console.WriteLine("error: price must be a valid number");
                        return;
                    }
                    break;

                case FixConstants.ORD_TYPE_VWAP:
                    if (parts.Length < ApplicationConstants.MIN_VWAP_ORDER_ARGS)
                    {
                        Console.WriteLine("error: price must be specified for VWAP orders");
                        return;
                    }
                    price = parts[ApplicationConstants.PRICE_INDEX];
                    if (!FixUtils.IsValidNumber(price))
                    {
                        Console.WriteLine("error: price must be a valid number");
                        return;
                    }
                    break;

                default:
                    Console.WriteLine("error: order type must be MARKET, LIMIT, or VWAP");
                    return;
            }

            string[] vwapParams = Array.Empty<string>();
            if (FixConstants.ORD_TYPE_VWAP.Equals(ordType) && parts.Length > ApplicationConstants.VWAP_PARAMS_START_INDEX)
            {
                vwapParams = parts[ApplicationConstants.VWAP_PARAMS_START_INDEX..];
            }

            try
            {
                Message message = MessageBuilder.BuildNewOrderSingle(
                    symbol, ordType, side, qtyType, qty, price,
                    _application.PortfolioId,
                    _application.SenderCompId,
                    _application.TargetCompId,
                    vwapParams
                );

                if (_application.SessionId != null)
                {
                    QuickFix.Session.SendToTarget(message, _application.SessionId);
                }
                else
                {
                    Console.WriteLine("error: no active session");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building order: {e.Message}");
            }
        }

        public void HandleOrderStatus(string[] parts)
        {
            if (parts.Length < ApplicationConstants.MIN_STATUS_REQUEST_ARGS)
            {
                Console.WriteLine("usage: status <ClOrdId> [OrderId] [Side] [Symbol]");
                return;
            }

            string clOrdId = parts[ApplicationConstants.STATUS_CL_ORD_ID_INDEX];
            string orderId = parts.Length > ApplicationConstants.STATUS_ORDER_ID_INDEX ? parts[ApplicationConstants.STATUS_ORDER_ID_INDEX] : "";
            string side = parts.Length > ApplicationConstants.STATUS_SIDE_INDEX ? parts[ApplicationConstants.STATUS_SIDE_INDEX] : "";
            string symbol = parts.Length > ApplicationConstants.STATUS_SYMBOL_INDEX ? parts[ApplicationConstants.STATUS_SYMBOL_INDEX] : "";

            ConcurrentDictionary<string, OrderInfo> orders = _application.Orders;
            _application.OrdersLock.EnterReadLock();
            try
            {
                if (orders.TryGetValue(clOrdId, out OrderInfo? cached) && cached != null)
                {
                    if (string.IsNullOrEmpty(orderId))
                    {
                        orderId = cached.OrderId ?? "";
                    }
                    if (string.IsNullOrEmpty(side))
                    {
                        side = cached.Side ?? "";
                    }
                    if (string.IsNullOrEmpty(symbol))
                    {
                        symbol = cached.Symbol ?? "";
                    }
                }
            }
            finally
            {
                _application.OrdersLock.ExitReadLock();
            }

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(side) || string.IsNullOrEmpty(symbol))
            {
                Console.WriteLine("need OrderId, Side, and Symbol (not cached)");
                return;
            }

            try
            {
                Message message = MessageBuilder.BuildOrderStatusRequest(
                    clOrdId, orderId, side, symbol,
                    _application.SenderCompId,
                    _application.TargetCompId
                );

                if (_application.SessionId != null)
                {
                    QuickFix.Session.SendToTarget(message, _application.SessionId);
                }
                else
                {
                    Console.WriteLine("error: no active session");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building status request: {e.Message}");
            }
        }

        public void HandleOrderCancel(string[] parts)
        {
            if (parts.Length < ApplicationConstants.MIN_CANCEL_REQUEST_ARGS)
            {
                Console.WriteLine("usage: cancel <ClOrdId>");
                return;
            }

            string clOrdId = parts[ApplicationConstants.CANCEL_CL_ORD_ID_INDEX];

            ConcurrentDictionary<string, OrderInfo> orders = _application.Orders;
            _application.OrdersLock.EnterReadLock();
            OrderInfo? orderInfo;
            try
            {
                orders.TryGetValue(clOrdId, out orderInfo);
            }
            finally
            {
                _application.OrdersLock.ExitReadLock();
            }

            if (orderInfo == null)
            {
                Console.WriteLine("unknown ClOrdId (not in cache)");
                return;
            }

            try
            {
                Message message = MessageBuilder.BuildOrderCancelRequest(
                    orderInfo,
                    _application.PortfolioId,
                    _application.SenderCompId,
                    _application.TargetCompId
                );

                if (_application.SessionId != null)
                {
                    QuickFix.Session.SendToTarget(message, _application.SessionId);
                }
                else
                {
                    Console.WriteLine("error: no active session");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error building cancel request: {e.Message}");
            }
        }

        public void HandleListOrders()
        {
            ConcurrentDictionary<string, OrderInfo> orders = _application.Orders;
            _application.OrdersLock.EnterReadLock();
            try
            {
                if (orders.IsEmpty)
                {
                    Console.WriteLine("(no cached orders)");
                    return;
                }

                foreach (var order in orders.Values)
                {
                    Console.WriteLine($"{order.ClOrdId,ApplicationConstants.ORDER_LIST_ID_COLUMN_WIDTH} → {order.OrderId} ({order.Side} {order.Symbol} {order.Quantity})");
                }
            }
            finally
            {
                _application.OrdersLock.ExitReadLock();
            }
        }

        public void HandleVersion()
        {
            Console.WriteLine(VersionUtils.GetApplicationNameWithVersion());
        }

        public void HandleUnknownCommand(string command)
        {
            Console.WriteLine($"unknown command: {command}");
            Console.WriteLine("Commands: new, status, cancel, list, version, exit");
        }
    }
}