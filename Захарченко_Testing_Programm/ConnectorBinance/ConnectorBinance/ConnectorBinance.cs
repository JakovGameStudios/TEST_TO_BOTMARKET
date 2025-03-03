using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ConnectorBinance
{
    public class BinanceRest : IDisposable
    {
        private readonly HttpClient _httpClient;
        private bool _disposed = false;


        public BinanceRest()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.binance.com/api/v3/") };
        }
        
        public async Task<string> GetTradesAsync(string symbol)  // Получение информации о торговле
        {
            var response = await _httpClient.GetStringAsync($"trades?symbol={symbol}");
            return response;
        }

        public async Task<string> GetCandlesAsync(string symbol)  // Получение информации о свечах
        {
            var response = await _httpClient.GetStringAsync($"klines?symbol={symbol}&interval=1h");
            return response;
        }

        public async Task<string> GetTickerAsync(string symbol)  // Получение текущей цены
        {
            var response = await _httpClient.GetStringAsync($"ticker/price?symbol={symbol}");
            return response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _httpClient.Dispose();
                }
                _disposed = true;
            }
        }
    }

    public class BinanceWebSocket : IDisposable
    {
        private readonly ClientWebSocket _webSocket;
        private bool _disposed;

        public BinanceWebSocket()
        {
            _webSocket = new ClientWebSocket();
        }

        public async Task ConnectAsync()  // Установа соединения
        {
            await _webSocket.ConnectAsync(new Uri("wss://stream.binance.com:9443/ws"), CancellationToken.None);
        }

        public async Task SubscribeToTrades(string symbol)  // Сообщение серверу о подписке на Trades
        {
            var message = $"{{\"method\":\"SUBSCRIBE\",\"params\":[\"{symbol.ToLower()}@trade\"],\"id\":1}}";

            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task SubscribeToCandles(string symbol)  // Сообщение серверу о подписке на Candles
        {
            var message = $"{{\"method\":\"SUBSCRIBE\",\"params\":[\"{symbol.ToLower()}@kline_1m\"],\"id\":1}}";
            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task<string> ReceiveMessages(CancellationToken cancellationToken)  // Получение сообщений от сервера
        {
            var buffer = new byte[1024 * 4];
           
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
              //  Console.WriteLine($"Received: {message}");  // MessageBox.Show($"Received: {message}", "Binance");
                return message;
            
        }

        public async Task DisconnectAsync()  // Закрытие соединения
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_webSocket.State == WebSocketState.Open)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }
                    _webSocket.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
