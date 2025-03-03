using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ConnectorBinance;

namespace Testing_Programm
{
    // Данный класс использует метод под REST API из созданной библиотеки ConnectorBinance
    public class BriefcaseCalculator  
    {
        public async Task<decimal> CalculateTicker(string symbol)  // Расчёт тикера
        {
            using (var client = new BinanceRest())
            {
                string symboltickerinfo = await client.GetTickerAsync(symbol);
                var symbollist = symboltickerinfo.Split('"');
                decimal symbolticker = Convert.ToDecimal(symbollist[7].Replace('.', ','));
                return symbolticker;
            }
        }

        public decimal CalculateBalance(decimal amount, decimal symbolticker)  // Расчёт баланса
        {          
            decimal symbolbalance = amount * symbolticker;

            return symbolbalance;
        }

        public decimal CalculateTotalBalance(decimal usdttotalbalance, decimal symbolticker)  // Расчёт общего баланса, исодя из общего кол-ва USDT
        {
            decimal symboltotalbalance = usdttotalbalance / symbolticker;

            return symboltotalbalance;
        }
    }
}
