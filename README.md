# C# FIX Client for Coinbase Prime

## Introduction
This repository contains a lightweight C#-based FIX client that connects to Coinbase Prime's FIX gateway. It provides an interactive REPL to:
- Create new orders (Market, Limit, VWAP)
- Look up existing orders (using a local `orders.json` cache)  
- Cancel orders

Under the hood, [QuickFIX/N](https://github.com/connamara/quickfixn) is used to handle FIX message encoding/decoding and session management.

## Prerequisites
- **.NET 9.0+** installed (https://dotnet.microsoft.com/download)
- A valid **Coinbase Prime account** with readwrite API key credentials

---

## 1. Build the Application

```bash
dotnet build
```

Or to create a publishable executable:

```bash
dotnet publish -c Release
```

## 2. Configure `fix.cfg` for Native TLS

Coinbase Prime FIX supports **native TLS**, so no stunnel or proxy is required.

The `fix.cfg` file at the project root should contain your service account ID. Replace the `SenderCompID` with your actual service account ID.

This configuration enables QuickFIX/N to connect directly over TLS without relying on external proxies.

## 3. API Credentials

Your C# FIX client requires environment variables to sign the FIX Logon. Set the following in your shell before running:

```bash
export PRIME_ACCESS_KEY="your_api_access_key"
export PRIME_SIGNING_KEY="your_api_secret_key"
export PRIME_PASSPHRASE="your_api_passphrase"
export PRIME_PORTFOLIO_ID="your_portfolio_id"
export PRIME_SVC_ACCOUNT_ID="your_service_account_id"
```

## 4. Run the C# FIX Client

Run the client directly with .NET:
```bash
dotnet run
```

Or with the published executable:
```bash
./bin/Release/net9.0/publish/PrimeFixDotNet
```

On successful FIX Logon, you'll see:
```bash
Commands: new, status, cancel, list, version, exit
```

## 5. REPL Commands

Once the client is running, type one of the following at the `FIX>` prompt:

### Create a New Order

```bash
FIX> new <symbol> <MARKET|LIMIT|VWAP> <BUY|SELL> <BASE|QUOTE> <qty> [price] [start_time] [participation_rate] [expire_time]
```

#### Quantity Types
- **BASE**: Quantity specified in base currency (e.g., BTC for BTC-USD)
- **QUOTE**: Quantity specified in quote currency (e.g., USD for BTC-USD)

#### Examples

**Market Orders:**
```bash
# Buy 0.1 BTC (base currency)
FIX> new BTC-USD MARKET BUY BASE 0.1

# Buy $1000 worth of BTC (quote currency)
FIX> new BTC-USD MARKET BUY QUOTE 1000
```

**Limit Orders:**
```bash
# Buy 0.1 BTC at $30000 (base currency)
FIX> new BTC-USD LIMIT BUY BASE 0.1 30000

# Buy $3000 worth of BTC at $30000 (quote currency)
FIX> new BTC-USD LIMIT BUY QUOTE 3000 30000
```

**VWAP Orders:**
You can specify VWAP orders with various combinations of optional parameters:

```bash
# Basic VWAP with just price (base currency)
FIX> new BTC-USD VWAP BUY BASE 1.0 50000

# VWAP with start time (quote currency)
FIX> new BTC-USD VWAP BUY QUOTE 50000 50000 2025-08-01T10:00:00Z

# VWAP with start time and participation rate (10%)
FIX> new BTC-USD VWAP BUY BASE 1.0 50000 2025-08-01T10:00:00Z 0.1

# VWAP with all parameters (start and expire time)
FIX> new BTC-USD VWAP BUY BASE 1.0 50000 2025-08-01T10:00:00Z 2025-08-01T16:00:00Z
```

**VWAP Parameters:**
- `start_time`: When execution should begin (ISO 8601 format)
- `participation_rate`: Execution aggressiveness (0.0-1.0, e.g., 0.1 = 10%)
- `expire_time`: When the order should expire (ISO 8601 format)

The order is sent, and the ExecReport (fill/cancel information) will be stored in `orders.json`.

### Look Up an Existing Order

```bash
FIX> status <ClOrdId> [OrderId] [Side] [Symbol]
```

This application automatically generates a unique `ClOrdId` (Client Order ID) using `DateTimeOffset.UtcNow.Ticks`. This value can be collected from `orders.json`, or from FIX responses sent by the server. `OrderId`, `Side`, and `Symbol` are required, however this app will automatically import these to the request based on the provided `ClOrdId`.

Example:
```bash
FIX> status 638123456789012345
```
If `orders.json` contains that `ClOrdId`, its `OrderId`, `Side`, and `Symbol` are filled in automatically.

### Cancel an Order

```bash
FIX> cancel <ClOrdID>
```

This request looks up an order by `ClOrdId` and attempts to cancel it.

### List All Cached Orders

```bash
FIX> list
```

This command lists out all stored orders from `orders.json`.

### Other Commands

```bash
FIX> version    # Show application version
FIX> help       # Show command help
FIX> exit       # Exit the application
```

## Dependencies

This project uses the following NuGet packages:
- **QuickFIXn.Core** (1.13.0) - Core QuickFIX/N library
- **QuickFIXn.FIX4.2** (1.13.0) - FIX 4.2 protocol support
- **Serilog** (4.1.0) - Structured logging
- **Serilog.Sinks.Console** (6.0.0) - Console logging sink
