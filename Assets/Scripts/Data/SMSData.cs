using System;
using System.Collections.Generic;
using UnityEngine;
namespace CubeHole.MM
{
    public enum TransactionType { debit, credit }
    [System.Serializable]
    public class PlayerSettingsInfo
    {
        public bool ReadSMS=true;
        public bool Notification=true;
        public bool Sound = true;
        public bool Vibration = true;
    }
    [System.Serializable]
    public class BankInfo
    {
        public string BankName;
        public int Id;
    }
    [System.Serializable]
    public class Account
    {
        public string AccountNumber;
        public float Balance;
        public bool autoMode = true;
        public BankInfo BankInfo;
    }
    [System.Serializable]
    public class Transaction
    {
        public string TransactionId;
       public Account AccountInfo=new Account();
        public TransactionType Type;
        public float TransactionAmount;
        public float TotalAmount;
        public DateTime TransactionDate;
        public string TransactionDateString;
        public string TransactionSMS;
        public string Address;
        public string CreditedAccountName;
        public bool IsTransfer=false;
        public string Category = "Not Set";
    }
    [System.Serializable]
    public class SMS
    {
        public string id;
        public string address;
        public string body;
        public string date;
    }

    [CreateAssetMenu(fileName = "SMS Data", menuName = "SMS_Data/SMS_lib")]
    public class SMSData : ScriptableObject
    {
        public List<SMS> SMSDirectory = new List<SMS>();
        public List<SMS> BankSMS = new List<SMS>();
        public List<Transaction> Transactions = new List<Transaction>();
        public List<Account> Accounts = new List<Account>();
        public SMS LatestSMS;
        public bool IsInitialised = false;
        public int TotalMessageCount;
        public int BudgetValue = -1;
        public string LastSyncDate = "";
        public PlayerSettingsInfo Settings = new PlayerSettingsInfo();
    }
}
 