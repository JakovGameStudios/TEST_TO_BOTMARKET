using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ConnectorBinance;

namespace Testing_Programm
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        public ObservableCollection<CurrencyTableCalculation> Balances { get; set; }  // Таблица 1

        public ObservableCollection<WorkingWebsocketAPI> Output { get; set; }  // Таблица 2

        public ICommand LoadDataCommand { get; set; }

        System.Windows.Threading.DispatcherTimer dispatcherTimer;  // Тамер, чтобы непрерывно вёлся расчёт
        BriefcaseCalculator calculator;  // Класс с функциями для расчёта
        BinanceWebSocket webSocket;  // Класс для работы с API Websocket для Binance
        CancellationTokenSource token;
        private bool _disposed = false;


        // Переменные для расчёта
        private decimal btcbalance;
        private decimal xrpbalance;
        private decimal xmrbalance;
        private decimal dashbalance;
        private decimal usdttotalbalance;
        private decimal btctotalbalance;
        private decimal xrptotalbalance;
        private decimal xmrtotalbalance;
        private decimal dashtotalbalance;


        public MainViewModel()
        {
            Balances = new ObservableCollection<CurrencyTableCalculation>();
            Output = new ObservableCollection<WorkingWebsocketAPI>();
            LoadDataCommand = new RelayCommand(LoadData);
            calculator = new BriefcaseCalculator();
            webSocket = new BinanceWebSocket();
            token = new CancellationTokenSource();

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();


            Task.Run(ReceiveWebSocketMessages);  // Запуск Websocket

        }

        private int i_timer = 0;
        private bool isLoading = false;
        private bool openconnect_flag = false;

        // Данный Task использует методы под Websocket API из созданной библиотеки ConnectorBinance
        private async Task ReceiveWebSocketMessages()  // Получаем сообщение от сервера и заносим в Таблицу 2
        {
            try
            {
                if (!openconnect_flag)
                {
                    await webSocket.ConnectAsync();
                    await webSocket.SubscribeToTrades("BTCUSDT");
                    openconnect_flag = true;
                }

                while (!token.Token.IsCancellationRequested)  // Пока не отменена задача
                {
                    try
                    {
                        string message = await webSocket.ReceiveMessages(token.Token);

                        // Обновляем UI
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Output.Add(new WorkingWebsocketAPI { Output = message }); // Добавляем полученное сообщение в коллекцию для DataGrid
                            if (Output.Count > 1000)
                            {
                                Output.RemoveAt(0); // Удаляем самый старый элемент
                            }
                        }
                        );
                    }
                    catch (Exception ex)
                    {
                        // Обработка других ошибок
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(ex.Message, Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        );
                        break;
                    }
                }
            }
            finally
            {
                // Закрываем соединение при выходе из цикла
                if (openconnect_flag)
                {
                    await webSocket.DisconnectAsync();
                }
            }
        }

        private async void dispatcherTimer_Tick(object sender, EventArgs e)  // Работа таймера для непрерывного расчёта и занесения значений в Таблицу 1
        {
            // Для памяти полезно
            if (!isLoading)
            {
                isLoading = true;
                await LoadData();
                isLoading = false;
            }

            if (i_timer >= 10)
            {
                i_timer = 0;
                dispatcherTimer.Stop();
                dispatcherTimer.Start();
            }
        }

        public async Task LoadData()  // Расчёт данных и занесение значений в Таблицу 1
        {
            try
            {
               // Получение Ticker валюты и расчитываем баланс
                btcbalance = calculator.CalculateBalance(1, await calculator.CalculateTicker("BTCUSDT"));
                xrpbalance = calculator.CalculateBalance(15000, await calculator.CalculateTicker("XRPUSDT"));
                xmrbalance = calculator.CalculateBalance(50, await calculator.CalculateTicker("XMRUSDT"));
                dashbalance = calculator.CalculateBalance(30, await calculator.CalculateTicker("DASHUSDT"));

               // Общий баланс в USDT
                usdttotalbalance = btcbalance + xrpbalance + xmrbalance + dashbalance;

               // Расчёт общего баланса по каждой валюте
                btctotalbalance = calculator.CalculateTotalBalance(usdttotalbalance, await calculator.CalculateTicker("BTCUSDT"));
                xrptotalbalance = calculator.CalculateTotalBalance(usdttotalbalance, await calculator.CalculateTicker("XRPUSDT"));
                xmrtotalbalance = calculator.CalculateTotalBalance(usdttotalbalance, await calculator.CalculateTicker("XMRUSDT"));
                dashtotalbalance = calculator.CalculateTotalBalance(usdttotalbalance, await calculator.CalculateTicker("DASHUSDT"));

                //  Заполнение Таблицы
                Balances.Clear();
                Balances.Add(new CurrencyTableCalculation { Currency = "USDT", Balance = 0, TotalBalanceInCurrency = Math.Round(usdttotalbalance, 5) });
                Balances.Add(new CurrencyTableCalculation { Currency = "BTC", Balance = 1, TotalBalanceInCurrency = Math.Round(btctotalbalance, 5) });
                Balances.Add(new CurrencyTableCalculation { Currency = "XRP", Balance = 15000, TotalBalanceInCurrency = Math.Round(xrptotalbalance, 5) });
                Balances.Add(new CurrencyTableCalculation { Currency = "XMR", Balance = 50, TotalBalanceInCurrency = Math.Round(xmrtotalbalance, 5) });
                Balances.Add(new CurrencyTableCalculation { Currency = "DASH", Balance = 30, TotalBalanceInCurrency = Math.Round(dashtotalbalance, 5) });

            }
            catch (Exception ex)
            {
                dispatcherTimer.Stop();
                MessageBox.Show(ex.Message, Assembly.GetExecutingAssembly().GetName().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                    dispatcherTimer.Stop();
                    token.Cancel();
                    webSocket.Dispose(); 
                }
                _disposed = true;
            }
        }
    }
}
