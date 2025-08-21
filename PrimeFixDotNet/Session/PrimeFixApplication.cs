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
using PrimeFixDotNet.Utils;
using QuickFix;
using QuickFix.Fields;
using Serilog;

namespace PrimeFixDotNet.Session
{
    public class PrimeFixApplication : IApplication, IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<PrimeFixApplication>();

        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _passphrase;
        private readonly string _senderCompId;
        private readonly string _targetCompId;
        private readonly string _portfolioId;

        private SessionID? _sessionId;
        
        // Order management
        private readonly Dictionary<string, OrderInfo> _orders;
        private readonly ReaderWriterLockSlim _ordersLock;
        private readonly OrderCache _orderCache;

        public PrimeFixApplication(PrimeFixConfig config)
        {
            config.Validate();
            
            _apiKey = config.AccessKey;
            _apiSecret = config.SigningKey;
            _passphrase = config.Passphrase;
            _senderCompId = config.SvcAccountId;
            _targetCompId = config.TargetCompId;
            _portfolioId = config.PortfolioId;
            
            // Initialize order management
            _orders = new Dictionary<string, OrderInfo>();
            _ordersLock = new ReaderWriterLockSlim();
            _orderCache = new OrderCache();
            
            // Load existing orders from cache
            try
            {
                _orderCache.LoadOrders(_orders);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Failed to load order cache, starting with empty cache");
            }
        }

        public void OnCreate(SessionID sessionId)
        {
            _sessionId = sessionId;
            Logger.Information("Session created: {SessionId}", sessionId);
        }

        public void OnLogon(SessionID sessionId)
        {
            _sessionId = sessionId;
            Logger.Information("FIX logon successful: {SessionId}", sessionId);
            Console.WriteLine("FIX logon successful!");
            Console.WriteLine("Commands: new, status, cancel, list, version, exit");
        }

        public void OnLogout(SessionID sessionId)
        {
            Logger.Information("FIX logout: {SessionId}", sessionId);
        }

        public void FromAdmin(Message message, SessionID sessionId)
        {
            try
            {
                string msgType, rawMessage;
                msgType = message.Header.GetString(Tags.MsgType);
                rawMessage = message.ToString().Replace('\u0001', '|');
                Logger.Information("INCOMING ADMIN: MsgType={MsgType}, Raw={RawMessage}", msgType, rawMessage);
                Console.WriteLine($"INCOMING ADMIN: MsgType={msgType}, Raw={rawMessage}");
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error processing FromAdmin message");
            }
        }

        public void ToAdmin(Message message, SessionID sessionId)
        {
            try
            {
                string msgType = message.Header.GetString(Tags.MsgType);
                
                if (MsgType.LOGON.Equals(msgType))
                {
                    // Use the SendingTime that QuickFIX already set in the header
                    string timestamp = message.Header.GetString(Tags.SendingTime);
                    MessageBuilder.BuildLogon(message, timestamp, _apiKey, _apiSecret,
                                            _passphrase, _targetCompId, _portfolioId);
                }
                
                // Log the actual message being sent
                string rawMessage = message.ToString().Replace('\u0001', '|');
                Logger.Information("OUTGOING ADMIN: MsgType={MsgType}, Raw={RawMessage}", msgType, rawMessage);
                Console.WriteLine($"OUTGOING ADMIN: MsgType={msgType}, Raw={rawMessage}");
            }
            catch (FieldNotFoundException e)
            {
                Logger.Error(e, "Error processing ToAdmin message");
            }
        }

        public void ToApp(Message message, SessionID sessionId)
        {
            try
            {
                string msgType, rawMessage;
                msgType = message.Header.GetString(Tags.MsgType);
                rawMessage = message.ToString().Replace('\u0001', '|');
                Logger.Information("OUTGOING APP: MsgType={MsgType}, Raw={RawMessage}", msgType, rawMessage);
                Console.WriteLine($"OUTGOING APP: MsgType={msgType}, Raw={rawMessage}");

                // Store new orders in cache for tracking
                if (MsgType.ORDER_SINGLE.Equals(msgType))
                {
                    string clOrdId, symbol, sideValue, quantity, quantityType;
                    clOrdId = FixUtils.GetString(message, Tags.ClOrdID);
                    symbol = FixUtils.GetString(message, Tags.Symbol);
                    sideValue = FixUtils.GetString(message, Tags.Side);
                    quantity = "";
                    quantityType = "";
                    
                    // Try to get OrderQty first (BASE), then CashOrderQty (QUOTE)
                    try
                    {
                        quantity = message.GetDecimal(Tags.OrderQty).ToString();
                        quantityType = FixConstants.QTY_TYPE_BASE;
                    }
                    catch
                    {
                        try
                        {
                            quantity = message.GetDecimal(Tags.CashOrderQty).ToString();
                            quantityType = FixConstants.QTY_TYPE_QUOTE;
                        }
                        catch
                        {
                            quantity = ApplicationConstants.DEFAULT_QUANTITY;
                            quantityType = FixConstants.QTY_TYPE_BASE; // default
                        }
                    }

                    string limitPrice = "";
                    try
                    {
                        limitPrice = message.GetDecimal(Tags.Price).ToString();
                    }
                    catch
                    {
                        limitPrice = "";
                    }

                    // Convert FIX side to readable format
                    string side = sideValue == "1" ? FixConstants.SIDE_BUY : FixConstants.SIDE_SELL;

                    var orderInfo = new OrderInfo(clOrdId, "", side, symbol, quantity, limitPrice, quantityType);
                    
                    _ordersLock.EnterWriteLock();
                    try
                    {
                        _orders[clOrdId] = orderInfo;
                        _orderCache.SaveOrders(_orders);
                    }
                    finally
                    {
                        _ordersLock.ExitWriteLock();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error processing ToApp message");
            }
        }

        public void FromApp(Message message, SessionID sessionId)
        {
            try
            {
                string msgType, rawMessage;
                msgType = message.Header.GetString(Tags.MsgType);
                rawMessage = message.ToString().Replace('\u0001', '|');
                Logger.Information("INCOMING APP: MsgType={MsgType}, Raw={RawMessage}", msgType, rawMessage);
                Console.WriteLine($"INCOMING APP: MsgType={msgType}, Raw={rawMessage}");

                // Handle execution reports
                if (MsgType.EXECUTION_REPORT.Equals(msgType))
                {
                    string clOrdId, orderId;
                    clOrdId = FixUtils.GetString(message, Tags.ClOrdID);
                    orderId = FixUtils.GetString(message, Tags.OrderID);
                    
                    if (!string.IsNullOrEmpty(clOrdId) && !string.IsNullOrEmpty(orderId))
                    {
                        _ordersLock.EnterWriteLock();
                        try
                        {
                            if (_orders.TryGetValue(clOrdId, out OrderInfo? existing) && existing != null)
                            {
                                existing.OrderId = orderId;
                                _orderCache.SaveOrders(_orders);
                            }
                        }
                        finally
                        {
                            _ordersLock.ExitWriteLock();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error processing FromApp message");
            }
        }

        public SessionID? SessionId => _sessionId;
        public string SenderCompId => _senderCompId;
        public string TargetCompId => _targetCompId;
        public string PortfolioId => _portfolioId;
        
        // Properties for CommandHandler
        public Dictionary<string, OrderInfo> Orders => _orders;
        public ReaderWriterLockSlim OrdersLock => _ordersLock;

        public void Dispose()
        {
            try
            {
                _orderCache.SaveOrders(_orders);
            }
            catch (Exception e)
            {
                Logger.Warning(e, "Failed to save orders during disposal");
            }
            
            _ordersLock?.Dispose();
        }
    }
}