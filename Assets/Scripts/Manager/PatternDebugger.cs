using MyBox;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace CubeHole.MM
{

    [CopyPasteComponent("Custom Components/My Component")]
    public class PatternDebugger : MonoBehaviour
    {
        public string pattern;
        public string inputDate;
        public SMS body;
        public Transaction transaction;
        public List<Transaction> sortedTransactions = new List<Transaction>();

        [ButtonMethod]
        public void Sort()
        {
            TransactionsManager.Init(AppManager.instance.GetSMSData(), null, null, true);
            Debug.Log(TransactionsManager.GetPattern(body.body, pattern));
            Debug.Log($"is from bank= {TransactionsManager.IsFromBank(body.body, true)}");
            transaction = TransactionsManager.ConvertTransaction(body);
        }

        [ButtonMethod]

        public void TestConversion()
        {
            DateTime date = TransactionsManager.ConvertDate(inputDate);
            Debug.Log(date);
            Debug.Log(date.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
