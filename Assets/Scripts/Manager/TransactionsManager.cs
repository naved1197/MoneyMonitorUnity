using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CubeHole.MM
{
    public enum FilteredData { TotalDebit,TotalCredit}
 
    public class TransactionsManager
    {
        public static string ruppeSymbol = "₹";
        private static string accountPattern = @"\b(?:ac|a/c|paytm|amazon pay|upi|CARD|account)\b";
        private static string creditPattern = @"\b(?:credited|received|repayment)\b";
        private static string debitPattern = @"\b(?:debited|paid|sent|transaction|transferred from)\b";
        private static string amountPattern = "rs|USD|INR";
        private static string blackListPattern = @"\b(?:HURRRY|SOON|EXPIRES|will be|can be|eligible|hi|attachments|due|IPO|Fantasy|win|declined|off|cashback|choose|loan|Bonus|Prize|working|Instant|Free|recharge|Shop|Install|save|claim|Register)\b";
        private static string amountPatternExtract = @"(?<=\bRs|INR |Rs.\s?)\d+(?:,\d+)*(\.\d+)?";
        private static string debitAccountPatternExtract = @"(?<=(credited to|at|paid.*? to|sent to)\s).*?(?=\s(with|-Ref|against|from|on))";
        private static string creditAccountPatternExtract = @"(?<=(credited to|at|paid.*? to|sent to)\s).*?(?=\s(with|-Ref|against|from|on))";
        private static string accountNumberPatternExtract = @"(?<=(?i)\b(?:a\/c|credit card|ac-|a\/c no. |Card no. |account|card ending |Card no |(?:ac(?:count)?)(?:\sno\.)?))\s*([*|X\d]+)(?!\S*a\/c)";
        private static string totalAmountExtract = @"(?<=Avl Bal\s|Avl Bal:\s*Rs\.|Paytm Wallet- Rs |balance is Rs. |credit limit is Rs.|Avl Bal is Rs.|Total Bal:\s*Rs\.\s*)\d+(?:,\d+)*(\.\d+)?";
        private static string totalAmountExtract2 = @"(?<=available limit is Rs[.,])\d+(?:,\d+)*\.\d+";
        public static string cashAccountName = "Cash";
        public static StringResourceLibrary categoriesPatterns = new StringResourceLibrary();

        private static SMSData SMSDirectory;
        private static Action OnProgressCompleted;
        public static void Init(SMSData Data, List<SMS> newSms,Action OnProgressComplete,bool test=false)
        {
            OnProgressCompleted = OnProgressComplete;
            SMSDirectory = Data;
            accountPattern = AppResources.GetStringsLibrary(R_Strings.CheckPatterns).GetStringResource(R_CheckPatterns.accountPattern.ToString());
            creditPattern = AppResources.GetStringsLibrary(R_Strings.CheckPatterns).GetStringResource(R_CheckPatterns.creditPattern.ToString());
            debitPattern = AppResources.GetStringsLibrary(R_Strings.CheckPatterns).GetStringResource(R_CheckPatterns.debitPattern.ToString());
            amountPattern = AppResources.GetStringsLibrary(R_Strings.CheckPatterns).GetStringResource(R_CheckPatterns.amountPattern.ToString());
            blackListPattern = AppResources.GetStringsLibrary(R_Strings.CheckPatterns).GetStringResource(R_CheckPatterns.blackListPattern.ToString());
            amountPatternExtract = AppResources.GetStringsLibrary(R_Strings.ExtractPatterns).GetStringResource(R_ExtractPatterns.amountPatternExtract.ToString());
            debitAccountPatternExtract = AppResources.GetStringsLibrary(R_Strings.ExtractPatterns).GetStringResource(R_ExtractPatterns.debitAccountPatternExtract.ToString());
            creditAccountPatternExtract = AppResources.GetStringsLibrary(R_Strings.ExtractPatterns).GetStringResource(R_ExtractPatterns.creditAccountPatternExtract.ToString());
            accountNumberPatternExtract = AppResources.GetStringsLibrary(R_Strings.ExtractPatterns).GetStringResource(R_ExtractPatterns.accountNumberPatternExtract.ToString());
            totalAmountExtract = AppResources.GetStringsLibrary(R_Strings.ExtractPatterns).GetStringResource(R_ExtractPatterns.totalAmountExtract.ToString());
            totalAmountExtract2 = AppResources.GetStringsLibrary(R_Strings.ExtractPatterns).GetStringResource(R_ExtractPatterns.totalAmountExtract2.ToString());
            categoriesPatterns = AppResources.GetStringsLibrary(R_Strings.CategoriesPattern);
            if (!test)
                AddNewSMSCoroutine(newSms);
        }
        static void AddNewSMSCoroutine(List<SMS> newSms = null)
        {
            if (SMSDirectory.IsInitialised)
            {
                Logger.Log($"Added {SMSDirectory.SMSDirectory.Count} SMS");
                SMSDirectory.SMSDirectory.InsertRange(0, newSms);
            }
            else
                SMSDirectory.SMSDirectory = newSms;
            Logger.Log($"Got List of {SMSDirectory.SMSDirectory.Count} SMS");
            FilterBankSMS(newSms);
        }
        static void FilterBankSMS(List<SMS> newSMS)
        {
            SMSDirectory.BankSMS = new List<SMS>();
            SMSDirectory.BankSMS.Clear();
            foreach (var item in newSMS)
            {
                if (item.body == "" || item == null)
                    continue;
                if (IsFromBank(item.body))
                {
                    SMSDirectory.BankSMS.Add(item);
                }
            }
            Logger.Log($"Found {SMSDirectory.BankSMS.Count} Bank SMS");
            ConvertSMSToTransaction();
        }
        private static List<Account> FindAllAccounts(List<Transaction> transactions)
        {
            //Find the account number with highest count and set it as the account number for all the transactions
            List<Account> accounts = new List<Account>();
            var AllAccounts = transactions.GroupBy(x => x.AccountInfo.AccountNumber, t => t).ToDictionary(k => k.Key, v => v.ToList());
            foreach (var item in AllAccounts)
            {
                var bankGrp = item.Value.GroupBy(x => x.AccountInfo.BankInfo.BankName, t => t).ToDictionary(k => k.Key, v => v.ToList());
                //get the bank name with highest count
                var bankName = bankGrp.OrderByDescending(x => x.Value.Count).First().Value[0].AccountInfo;
                foreach (var item2 in item.Value)
                {
                    item2.AccountInfo.BankInfo = bankName.BankInfo;
                }
                Account account = new Account();
                account.AccountNumber = item.Key;
                account.BankInfo = item.Value[0].AccountInfo.BankInfo;
                accounts.Add(account);
            }
            //Add new cash account

            accounts.Add(GetCashAccount());
            Logger.Log($"Found {accounts.Count} Accounts");
            return accounts;
        }
        public static Account GetCashAccount()
        {
            Account cashAccount = new Account();
            cashAccount.AccountNumber = cashAccountName;
            cashAccount.BankInfo = new BankInfo { BankName = cashAccountName, Id = -1 };
            return cashAccount;
        }
        static void MergeBanks(List<Transaction> transactions)
        {
            var PPBLAccountNumberT = transactions.Where(x => CheckKeyword("PPBL", x.TransactionSMS) && x.AccountInfo.AccountNumber != "PPBL").FirstOrDefault();
            var PPBLAccountNumber = PPBLAccountNumberT != null ? PPBLAccountNumberT.AccountInfo.AccountNumber : "";
            var PPBLTrasactions = transactions.Where(x => x.AccountInfo.AccountNumber == "PPBL").ToList();
            if (PPBLAccountNumber == "")
                PPBLAccountNumber = "OTHERS";
            foreach (var item in PPBLTrasactions)
            {
                item.AccountInfo.AccountNumber = PPBLAccountNumber;
            }
            var SBICardAccountNumberT = transactions.Where(x => CheckKeyword("SBI", x.TransactionSMS) && int.TryParse(x.AccountInfo.AccountNumber.Substring(x.AccountInfo.AccountNumber.Length - 4), out int r)).FirstOrDefault();
            var SBICardAccountNumber = SBICardAccountNumberT != null ? SBICardAccountNumberT.AccountInfo.AccountNumber : "";
            var SBITrasactions = transactions.Where(x => CheckKeyword(@"\bSBI(?: Credit)? Card\b", x.TransactionSMS)).ToList();
            if (SBICardAccountNumber == "")
                SBICardAccountNumber = "OTHERS";
            foreach (var item in SBITrasactions)
            {
                item.AccountInfo.AccountNumber = SBICardAccountNumber;
            }
            if (SMSDirectory.IsInitialised)
            {
                Logger.Log($"Added {transactions.Count} transactions");
                SMSDirectory.Transactions.InsertRange(0, transactions);
            }
            else
                SMSDirectory.Transactions = transactions;
            Logger.Log("Merged Banks");
        }
        static void ConvertSMSToTransaction()
        {
            List<Transaction> transactions = new List<Transaction>();
            string lastTransactionID = "";
            if(SMSDirectory.Transactions.Count> 0)
            {
                lastTransactionID = SMSDirectory.Transactions[0].TransactionId;
                Logger.Log($"Found previous transactions id {lastTransactionID}");
            }
            foreach (var item in SMSDirectory.BankSMS)
            {
                Transaction transaction = ConvertTransaction(item);
                transaction.IsTransfer = CheckForTransfers(transactions, transaction);
                if (!string.IsNullOrEmpty(lastTransactionID) && lastTransactionID == transaction.TransactionId)
                    continue;
                else
                    transactions.Add(transaction);
            }
            MergeBanks(transactions);
            List<Account> accounts = FindAllAccounts(transactions);
            //Check for new accounts and add them to the list
            foreach (var item in accounts)
            {
                if (!SMSDirectory.Accounts.Any(x => x.AccountNumber == item.AccountNumber))
                {
                    SMSDirectory.Accounts.Add(item);
                }
            }
            Logger.Log($"Converted {SMSDirectory.BankSMS.Count} SMS to {transactions.Count} Transaction");
            SMSDirectory.BankSMS.Clear();
            SMSDirectory.IsInitialised = true;
            OnProgressCompleted?.Invoke();
        }
        static bool CheckForTransfers(List<Transaction> transactions,Transaction currentTransaction)
        {
            List<Transaction> sortedTransactions=new List<Transaction>();
            if (transactions.Count == 0)
                return false;
            for (int i = transactions.Count - 1; i >= 0; i--)
            {
                if (
                    transactions[i].TransactionDate.Year == currentTransaction.TransactionDate.Year &&
                    transactions[i].TransactionDate.Month == currentTransaction.TransactionDate.Month &&
                    transactions[i].TransactionDate.Day == currentTransaction.TransactionDate.Day &&
                    transactions[i].TransactionAmount == currentTransaction.TransactionAmount &&
                   transactions[i].Type != currentTransaction.Type &&
                    transactions[i].AccountInfo.AccountNumber != currentTransaction.AccountInfo.AccountNumber)
                {
                    sortedTransactions.Add(transactions[i]);
                }
                else
                    break;
            }
            if (sortedTransactions.Count == 0)
                return false;
            else
            {
                foreach (var item in sortedTransactions)
                {
                    item.IsTransfer = true;
                }
               // Logger.Log("Transfer Found");
                return true;
            }

        }

        #region Core Functions
        public static List<Transaction> AssignCategrories(List<Transaction> transactions)
        {
            foreach (var item in transactions)
            {
                item.Category = FindCategory(item.TransactionSMS);
            }
            return transactions;
        }
        public static string FindCategory(string transactionSms)
        {
            if(categoriesPatterns==null||categoriesPatterns.stringLibraries.Count==0)
            { 
                Logger.Log("Categories not loaded");
                return "Not Set";
            }

            string body = transactionSms;
            string category = "Not Set";
            foreach (var item in categoriesPatterns.stringLibraries)
            {
               // Logger.Log($"Checking {item.name} with {item.value}");
                if(CheckKeyword(item.value,body))
                {
                  //  Logger.Log($"Found {item.name} in {transaction.TransactionSMS}");
                    category = item.name;
                    break;
                }
            }
            return category;
        }
        public static float GetAvailableBalance(List<Transaction> transactions)
        {
            var lastCreditTransactions = transactions.Where(x => x.TotalAmount != 0).ToList();
            Transaction lastCreditT = null;
            float balanceAmount = 0;
            if (lastCreditTransactions.Count > 0)
            {
                lastCreditT = lastCreditTransactions.First();
                var total = lastCreditT != null ? lastCreditT.TotalAmount : 0;
                if(total==0)
                {
                    total = lastCreditT.AccountInfo.Balance;
                }    
                var creditAfterLast = transactions.Where(x => x.Type == TransactionType.credit && x.TransactionDate > lastCreditT.TransactionDate).ToList();

                foreach (var item2 in creditAfterLast)
                {
                    total += item2.TransactionAmount;
                }

                float DebitAfterLastCredit = transactions.Where(x => x.Type == TransactionType.debit && x.TransactionDate > lastCreditT.TransactionDate).Sum(x => x.TransactionAmount);
                balanceAmount = total - DebitAfterLastCredit;
            }
            else
            {
                balanceAmount = 0;   
            }
            if (balanceAmount < 0)
            {
                balanceAmount = lastCreditT != null ? lastCreditT.TotalAmount : 0;
            }
            if (balanceAmount == 0)
            {
                balanceAmount = transactions.Where(x => x.Type == TransactionType.credit).Sum(x => x.TransactionAmount);
            }
            return balanceAmount;
        }
        public static Dictionary<string,List<Transaction>> GetAllAccounts(List<Transaction> AllTransactions)
        {
            var AccountsGrp = AllTransactions.GroupBy(x => x.AccountInfo.AccountNumber, v => v).ToDictionary(k => k.Key, v => v.ToList());
            var sortedAccounts = AccountsGrp.OrderByDescending(kv => kv.Value.Count).ToDictionary(kv => kv.Key, kv => kv.Value);
            return sortedAccounts;
        }
        public static string GetAmountString(float amount)
        {
            return ruppeSymbol + amount.ToString();
        }

        public static string GetCompressedAmountString(float value)
        {
            string amount = "";
            if (value > 1000)
            {
                amount = (value / 1000).ToString("0.0") + "K";
            }
            else
            {
                amount = value.ToString();
            }
            return ruppeSymbol+ amount.ToString();
        }
        public static List<Transaction> FilterTransactionByDate(DateTime fromDate,DateTime toDate,List<Transaction> transactions)
        {
            if(fromDate>toDate)
            {
                Logger.Log("From Date is greater than To Date");
                return null;
            }
            var filteredTransactions = transactions.Where(x => x.TransactionDate.Month >= fromDate.Month&& x.TransactionDate.Month <= toDate.Month 
            && x.TransactionDate.Year >= fromDate.Year && x.TransactionDate.Year <= toDate.Year).ToList();
            return filteredTransactions;
        }
        public static Dictionary<FilteredData, float> ExtractDataFromTransactions(List<Transaction> filteredTransaction)
        {
           var totalDebitAmount = filteredTransaction.Where(x => x.Type == TransactionType.debit && x.IsTransfer == false).Sum(x => x.TransactionAmount);
            var totalCredit = filteredTransaction.Where(x => x.Type == TransactionType.credit && x.IsTransfer == false).Sum(x => x.TransactionAmount);
            Dictionary<FilteredData, float> monthData = new Dictionary<FilteredData, float>();
            monthData.Add(FilteredData.TotalDebit, totalDebitAmount);
            monthData.Add(FilteredData.TotalCredit, totalCredit);
            return monthData;
        }
        public static Dictionary<DateTime,List<Transaction>> GetCurrentWeekTransactions(DateTime todaysDate,List<Transaction> AllTransactions)
        {
            DateTime lastSevenDate = todaysDate.Subtract(TimeSpan.FromDays(7));
            var LastSevenDaysTransactions = AllTransactions.Where(x => x.TransactionDate >= lastSevenDate);
            Dictionary<DateTime, List<Transaction>> LastSevenDaysGrp = new Dictionary<DateTime, List<Transaction>>();
            for (int i = 0; i < 7; i++)
            {
                DateTime dateTime = todaysDate.Subtract(TimeSpan.FromDays(i));
                LastSevenDaysGrp.Add(dateTime, new List<Transaction>());
            }
            LastSevenDaysGrp = LastSevenDaysGrp.Reverse().ToDictionary(x => x.Key, v => v.Value);
            var AllWeeksTransactions = LastSevenDaysTransactions.GroupBy(x => x.TransactionDate.Day).ToDictionary(x => x.Key.ToString(), v => v.Where(x=>x.IsTransfer == false).ToList());
            for (int i = 0; i < LastSevenDaysGrp.Count; i++)
            {
                DateTime keyDate = LastSevenDaysGrp.ElementAt(i).Key;
                string key = keyDate.Day.ToString();
                List<Transaction> transactions = new List<Transaction>();
                AllWeeksTransactions.TryGetValue(key, out transactions);
                if (transactions != null)
                    LastSevenDaysGrp[keyDate] = transactions;
                else
                    LastSevenDaysGrp[keyDate] = new List<Transaction>();
            }
            return LastSevenDaysGrp;
        }
        public static bool IsFromBank(string message,bool debug=false)
        {
            if (message == null)
            {
                Logger.Log($"Found message is null");
                return false;
            }
            bool hasAccount = false;
            bool hasTransactionType = false;
            bool hasAmount = false;
            if (CheckKeyword(blackListPattern, message))
            {
                if (debug)
                    Logger.Log("bank sms rejected from blacklist pattern");
                return false;
            }
            if (CheckKeyword(accountPattern, message))
            {
                if (debug)
                    Logger.Log("bank sms has account pattern");
                hasAccount = true;
            }
            if (CheckKeyword(creditPattern, message) || CheckKeyword(debitPattern, message))
            {
                if (debug)
                    Logger.Log("bank sms has credit and debit pattern");
                hasTransactionType = true;
            }
            if (CheckKeyword(amountPattern, message))
            {
                if (debug)
                    Logger.Log("bank sms has amount pattern");
                hasAmount = true;
            }
            if (hasAccount && hasTransactionType && hasAmount)
            {
                if (debug)
                    Logger.Log("bank sms has all patterns");
                return true;
            }
            return false;
        }
        static BankInfo GetBankName(string message)
        {
            Dictionary<string, string> BankNames = new Dictionary<string, string>();
            StringResourceLibrary banksAddress = AppResources.GetStringsLibrary(R_Strings.BanksSMSAdress);
            foreach (var item in banksAddress.stringLibraries)
            {
                BankNames.Add(item.name, item.value);
            }
            int setBankType = -1;
            for (int i = 0; i < BankNames.Count; i++)
            {
                string key = BankNames.Keys.ElementAt(i);
                string value = BankNames[key];
                if (CheckKeyword(value, message))
                {
                    setBankType = i;
                    break;
                }
            }
            if (setBankType >= 0)
            {
                BankInfo bankInfo = new BankInfo();
                bankInfo.BankName = BankNames.Keys.ElementAt(setBankType);
                bankInfo.Id = setBankType;
                return bankInfo;
            }

            return null;
        }
        static float GetAmount(string message)
        {
            string amount = GetPattern(message, amountPatternExtract);
            float result = 0;
            float.TryParse(amount, out result);
            return result;
        }

        static string GetAccountName(string message,TransactionType type)
        {
            string account =type==TransactionType.debit? GetPattern(message, debitAccountPatternExtract):GetPattern(message,creditAccountPatternExtract);
            return account;
        }
        static string FindAccountNumber(string message)
        {
            string account = GetPattern(message, accountNumberPatternExtract);
            string accountNumber = "";
            if (account.Length >= 4)
            {
                string lastFourDigit = account.Substring(account.Length - 4);
                if (lastFourDigit.Any(c => char.IsDigit(c)))
                {
                    accountNumber = "XXXX" + lastFourDigit;
                    return accountNumber;
                }

            }
            else if (account.Length == 2 && int.TryParse(account, out int b))
            {
                accountNumber = "XXXXXX" + account;
                return accountNumber;
            }
            Dictionary<string, string> backAccountPatterns = new Dictionary<string, string>();
            StringResourceLibrary backAccountPatternsStrings = AppResources.GetStringsLibrary(R_Strings.BankAccountsPattern);
            foreach (var item in backAccountPatternsStrings.stringLibraries)
            {
                backAccountPatterns.Add(item.name, item.value);
            }
            //  string[] otherAccounts = { @"\b(?:paytm)\b", @"\bSBI(?: Credit)? Card\b", @"\b(?:SLICE)\b", @"\b(?:PPBL)\b" };
            foreach (var item in backAccountPatterns)
            {
                account = GetPattern(message, item.Value);
                if (!string.IsNullOrEmpty(account))
                {
                    accountNumber = item.Key;
                    break;
                }
            }
            if (string.IsNullOrEmpty(accountNumber))
                return "OTHERS";
            else
                return accountNumber;
        }
        static TransactionType GetType(string sms)
        {
            if (CheckKeyword(debitPattern, sms))
            {
                return TransactionType.debit;
            }
            else
            {
                return TransactionType.credit;
            }
        }
        static float GetTotal(string message)
        {
            string totalPattern2 = totalAmountExtract2;
            string pattern = GetPattern(message, totalAmountExtract);
            if (string.IsNullOrEmpty(pattern))
            {
                pattern = GetPattern(message, totalPattern2);
            }
            float result = -1;
            float.TryParse(pattern, out result);
            return result;
        }
      public static Transaction ConvertTransaction(SMS sms)
        {
            BankInfo bankInfo = GetBankName(sms.address);
            Transaction transaction = new Transaction();
            transaction.TransactionId = sms.id;
            transaction.AccountInfo = new Account();
            transaction.AccountInfo.AccountNumber = FindAccountNumber(sms.body).ToUpper();
            transaction.AccountInfo.BankInfo = bankInfo != null ? bankInfo : new BankInfo { BankName = "NONE", Id = -1 };
            transaction.Type = GetType(sms.body);
            transaction.TransactionDate = ConvertDate(sms.date);
            transaction.TransactionAmount = GetAmount(sms.body);
            transaction.TotalAmount = GetTotal(sms.body);
            transaction.TransactionSMS = sms.body;
            transaction.Address = sms.address;
            transaction.TransactionDateString = sms.date;
            transaction.Category = FindCategory(sms.body);
            string creditedAccountName = GetAccountName(sms.body,transaction.Type);
            transaction.CreditedAccountName = creditedAccountName != "" ? creditedAccountName : transaction.Type.ToString();
            return transaction;
        }
        public static string GetPattern(string message, string pattern)
        {
            return Regex.Match(message, pattern, RegexOptions.IgnoreCase).Value;
        }
      public static bool CheckKeyword(string pattern, string message, bool softcheck = false)
        {
            if (!softcheck)
            {
                if (Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase))
                    return true;
                else
                    return false;

            }

            if (message.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            else
                return false;
        }
        #endregion
        #region Utility Functions
        public static string GetDate(string smsDate)
        {
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime date;
            try
            {
                if (DateTime.TryParseExact(smsDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    return (DateTime.Now.Date == date.Date && DateTime.Now.Year == date.Year && DateTime.Now.Month == date.Month) ? "Today " + date.ToString("hh:mm tt") : date.ToString("dd MMM");
            }
            catch (Exception)
            {
                Logger.LogError("Cannot convert " + smsDate);
            }
            return "_";
        }
        public static DateTime ConvertDate(string smsDate)
        {
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime date = DateTime.Now;
            try
            {
                if (DateTime.TryParseExact(smsDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                    return date;
                else
                {
                      Logger.LogError("Cannot convert " + smsDate);

                }    
            }
            catch (Exception)
            {
                Logger.LogError("Cannot convert " + smsDate);
            }
            return date;
        }

        public static Dictionary<DateTime,List<Transaction>> GroupTransactionsByDate(List<Transaction> filteredTransactions)
        {
           Dictionary<DateTime, List<Transaction>> groupedTransactions = new Dictionary<DateTime, List<Transaction>>();
          groupedTransactions=  filteredTransactions.GroupBy(x => x.TransactionDate.Date).ToDictionary(x => x.Key, x => x.OrderBy(x=>x.TransactionDate).ToList());
            return groupedTransactions;
        }

        public static Dictionary<string,List<Transaction>> GroupTransactionsByCategory(List<Transaction> filteredTransactions)
        {
           Dictionary<string,List<Transaction>> keyValuePairs = new Dictionary<string, List<Transaction>>();
            keyValuePairs = filteredTransactions.GroupBy(x => x.Category).ToDictionary(x => x.Key, x => x.ToList());
            return keyValuePairs;
        }

        public static List<Transaction> FilterTransactionsByAccount(List<string> selectedAccounts, List<Transaction> filteredTransactions)
        {
            var filtered = filteredTransactions.Where(x => selectedAccounts.Contains(x.AccountInfo.AccountNumber)).ToList();
            return filtered;
        }
        #endregion
    }
}
