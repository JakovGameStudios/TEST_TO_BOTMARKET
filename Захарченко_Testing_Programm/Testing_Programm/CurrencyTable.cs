using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing_Programm
{
    public class CurrencyTableCalculation // Столбцы таблицы
    {
        public string Currency { get; set; }  // Валюта
        public decimal Balance { get; set; }  // Баланс
        public decimal TotalBalanceInCurrency { get; set; }  // Общий баланс в валюте

    }

    public class WorkingWebsocketAPI  // Столбцы таблицы
    {
        public string Output { get; set; }  // Вывод
       
    }

}
