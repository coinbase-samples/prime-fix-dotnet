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

using PrimeFixDotNet.Builder;
using PrimeFixDotNet.Constants;
using PrimeFixDotNet.Model;
using PrimeFixDotNet.Session;
using PrimeFixDotNet.Utils;
using QuickFix;
using Serilog;

namespace PrimeFixDotNet.Repl
{
    public class CommandHandler
    {
        private static readonly ILogger Logger = Log.ForContext<CommandHandler>();
        private readonly PrimeFixApplication _application;

        public CommandHandler(PrimeFixApplication application)
        {
            _application = application;
        }

        public void HandleNewOrder(string[] parts)
        {
            if (parts.Length < ApplicationConstants.MIN_NEW_ORDER_ARGS)
            {
                Logger.Error("Insufficient arguments for new order command");
                Logger.Information("Usage: new <symbol> <MARKET|LIMIT|VWAP> <BUY|SELL> <BASE|QUOTE> <qty> [price] [start_time] [participation_rate] [expire_time]");
                return;
            }

            string symbol, ordType, side, qtyType, qty;
            symbol = parts[ApplicationConstants.SYMBOL_INDEX];
            ordType = parts[ApplicationConstants.ORDER_TYPE_INDEX].ToUpper();
            side = parts[ApplicationConstants.SIDE_INDEX].ToUpper();
            qtyType = parts[ApplicationConstants.QTY_TYPE_INDEX].ToUpper();
            qty = parts[ApplicationConstants.QUANTITY_INDEX];
            string? price = null;

            if (!FixConstants.SIDE_BUY.Equals(side) && !FixConstants.SIDE_SELL.Equals(side))
            {
                Logger.Error(" side must be BUY or SELL");
                return;
            }

            if (!FixConstants.QTY_TYPE_BASE.Equals(qtyType) && !FixConstants.QTY_TYPE_QUOTE.Equals(qtyType))
            {
                Logger.Error(" quantity type must be BASE or QUOTE");
                return;
            }

            if (!FixUtils.IsValidNumber(qty))
            {
                Logger.Error(" qty must be a valid number");
                return;
            }

            switch (ordType)
            {
                case FixConstants.ORD_TYPE_MARKET:
                    if (parts.Length > ApplicationConstants.MIN_LIMIT_ORDER_ARGS)
                    {
                        Logger.Error(" MARKET orders should not include a price");
                        return;
                    }
                    break;

                case FixConstants.ORD_TYPE_LIMIT:
                    if (parts.Length < ApplicationConstants.MIN_LIMIT_ORDER_ARGS)
                    {
                        Logger.Error(" price must be specified for LIMIT orders");
                        return;
                    }
                    price = parts[ApplicationConstants.PRICE_INDEX];
                    if (!FixUtils.IsValidNumber(price))
                    {
                        Logger.Error(" price must be a valid number");
                        return;
                    }
                    break;

                case FixConstants.ORD_TYPE_VWAP:
                    if (parts.Length < ApplicationConstants.MIN_VWAP_ORDER_ARGS)
                    {
                        Logger.Error(" price must be specified for VWAP orders");
                        return;
                    }
                    price = parts[ApplicationConstants.PRICE_INDEX];
                    if (!FixUtils.IsValidNumber(price))
                    {
                        Logger.Error(" price must be a valid number");
                        return;
                    }
                    break;

                default:
                    Logger.Error(" order type must be MARKET, LIMIT, or VWAP");
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
                    Logger.Error(" no active session");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error building order: {Message}", e.Message);
            }
        }

        public void HandleOrderStatus(string[] parts)
        {
            if (parts.Length < ApplicationConstants.MIN_STATUS_REQUEST_ARGS)
            {
                Logger.Information("Usage: status <ClOrdId> [OrderId] [Side] [Symbol]");
                return;
            }

            string clOrdId, orderId, side, symbol;
            clOrdId = parts[ApplicationConstants.STATUS_CL_ORD_ID_INDEX];
            orderId = parts.Length > ApplicationConstants.STATUS_ORDER_ID_INDEX ? parts[ApplicationConstants.STATUS_ORDER_ID_INDEX] : "";
            side = parts.Length > ApplicationConstants.STATUS_SIDE_INDEX ? parts[ApplicationConstants.STATUS_SIDE_INDEX] : "";
            symbol = parts.Length > ApplicationConstants.STATUS_SYMBOL_INDEX ? parts[ApplicationConstants.STATUS_SYMBOL_INDEX] : "";

            Dictionary<string, OrderInfo> orders = _application.Orders;
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
                Logger.Warning("Need OrderId, Side, and Symbol (not cached)");
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
                    Logger.Error(" no active session");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error building status request: {Message}", e.Message);
            }
        }

        public void HandleOrderCancel(string[] parts)
        {
            if (parts.Length < ApplicationConstants.MIN_CANCEL_REQUEST_ARGS)
            {
                Logger.Information("Usage: cancel <ClOrdId>");
                return;
            }

            string clOrdId = parts[ApplicationConstants.CANCEL_CL_ORD_ID_INDEX];

            Dictionary<string, OrderInfo> orders = _application.Orders;
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
                Logger.Warning("Unknown ClOrdId (not in cache)");
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
                    Logger.Error(" no active session");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error building cancel request: {Message}", e.Message);
            }
        }

        public void HandleListOrders()
        {
            Dictionary<string, OrderInfo> orders = _application.Orders;
            _application.OrdersLock.EnterReadLock();
            try
            {
                if (orders.Count == 0)
                {
                    Logger.Information("(No cached orders)");
                    return;
                }

                foreach (var order in orders.Values)
                {
                    Logger.Information("{ClOrdId} → {OrderId} ({Side} {Symbol} {Quantity})", 
                        order.ClOrdId?.PadRight(-ApplicationConstants.ORDER_LIST_ID_COLUMN_WIDTH), 
                        order.OrderId, order.Side, order.Symbol, order.Quantity);
                }
            }
            finally
            {
                _application.OrdersLock.ExitReadLock();
            }
        }

        public void HandleVersion()
        {
            Logger.Information("{ApplicationVersion}", VersionUtils.GetApplicationNameWithVersion());
        }

        public void HandleUnknownCommand(string command)
        {
            Logger.Warning("Unknown command: {Command}", command);
            Logger.Information("Commands: new, status, cancel, list, version, exit");
        }
    }
}