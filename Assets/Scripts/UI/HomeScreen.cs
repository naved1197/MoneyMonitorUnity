using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace CubeHole.MM
{
    public class HomeScreen : MonoBehaviour
    {
        [SerializeField] private TransactionHolder[] transactionHolders;
        [SerializeField] private TextMeshProUGUI currentMonthTxt;
        [SerializeField] private TextMeshProUGUI totalDebitTxt;
        [SerializeField] private TextMeshProUGUI totalCreditTxt;
        [SerializeField] private TextMeshProUGUI budgetPercentTxt;
        [SerializeField] private TextMeshProUGUI budgetPerDayTxt;
        [SerializeField] private TextMeshProUGUI lastSyncedDateTxt;
        [SerializeField] private Slider budgetslider;
        [SerializeField] private TMP_InputField budgetSetInput;
        [SerializeField] private GameObject setBudgetHolder;
        [SerializeField] private GameObject BudgetSetHolder;
        [SerializeField] private Button budgetBackButton;
        [SerializeField] private CharacterAnimator budgetCharacter;
        [SerializeField] private InitilisationHandler initilisationHandler;
        [SerializeField] private Transform accountHolder;
        [SerializeField] private AccountHolder accountHolderPrefab;
        [SerializeField] private KeyBoardTrigger keyBoardTrigger;
        [SerializeField] private TransactionViewer transactionViewer;
        [SerializeField] private BarGraphHandler weeksGraph;
        [SerializeField] private GameObject noDataInfoGraphic;
        [SerializeField] private GameObject accountsHolder;
        [SerializeField] private Button allTransactionBtn;
        private List<Transaction> AllTransactions = new List<Transaction>();
        public List<Transaction> currentMonthTransactions = new List<Transaction>();
        public static DateTime todaysDate;
        private float totalDebitAmount;
        private SMSData data;
        private List<AccountHolder> accountsHolders=new List<AccountHolder>();

        public void Init(SMSData data)
        {
            Logger.Log("HomeScreen Init");
            this.data = data;
            AllTransactions = data.Transactions;
            transactionViewer.Initialise(data.Accounts);
            StartCoroutine(ShowData());
        }

        IEnumerator ShowData()
        {
            DateTime lastTransactionDate = TransactionsManager.ConvertDate(data.LatestSMS.date);
            lastSyncedDateTxt.text = "Last Synced: " + lastTransactionDate.ToString("yyyy-MM-dd");
            todaysDate = DateTime.Now;
            Debug.Log("Last SMS Date " + data.LatestSMS.date);
//#if UNITY_EDITOR
//            todaysDate = new DateTime(year: 2023, month: (todaysDate.Month-1), day: 7);
//#endif
            Logger.Log("todays Date Date " + todaysDate);
            currentMonthTxt.text = todaysDate.ToString("MMMM");
            StartCoroutine(IntialiseTransactionsRountine());
            AppManager.instance.LoadRemoteConfig();
            yield return null;
        }
        public AccountHolder GetAccountHolder(int count)
        {
            if (count <= (accountsHolders.Count - 1))
            {
                return accountsHolders[count];
            }
            else
            {
                AccountHolder accountHolder = Instantiate(accountHolderPrefab, this.accountHolder);
                accountsHolders.Add(accountHolder);
                return accountHolder;
            }
        }
        public void IntialiseTransactions()
        {
            StartCoroutine(IntialiseTransactionsRountine());
        }
        IEnumerator IntialiseTransactionsRountine()
        {
            var LastSevenDaysGrp = TransactionsManager.GetCurrentWeekTransactions(todaysDate, AllTransactions);
            List<BarGraphInfo> weekData = new List<BarGraphInfo>();
            Dictionary<string, float> totalWeeksTransaction = new Dictionary<string, float>();
            Color barColor = AppResources.GetColorGroup(R_Colors.AppColors).GetColorResource(R_AppColors.Orange.ToString());
            foreach (var item in LastSevenDaysGrp)
            {
                totalWeeksTransaction.Add(item.Key.DayOfWeek.ToString(), item.Value.Where(x => x.Type == TransactionType.debit).Sum(x => x.TransactionAmount));
            }
            for (int i = 0; i < LastSevenDaysGrp.Count; i++)
            {
                float totalAmount = totalWeeksTransaction.ElementAt(i).Value;
              
                BarGraphInfo barGraphInfo = new BarGraphInfo();
                barGraphInfo.label = totalWeeksTransaction.ElementAt(i).Key.Substring(0, 2);
                barGraphInfo.data = new List<BarGraphData>();
                barGraphInfo.data.Add(new BarGraphData(totalWeeksTransaction.ElementAt(i).Value,
                    TransactionsManager.GetCompressedAmountString(totalAmount),
                    barColor, 
                    BarType.Vertical));
                weekData.Add(barGraphInfo);
            }
            weeksGraph.Init(weekData);

           var currentMonthGrp = TransactionsManager.ExtractDataFromTransactions(TransactionsManager.FilterTransactionByDate(todaysDate,todaysDate, AllTransactions));
            totalDebitTxt.text = TransactionsManager.GetAmountString(currentMonthGrp[FilteredData.TotalDebit]);
            totalDebitAmount = currentMonthGrp[FilteredData.TotalDebit];
            totalCreditTxt.text = TransactionsManager.GetAmountString(currentMonthGrp[FilteredData.TotalCredit]);

            var LastFive = AllTransactions.Take(5).ToList();
            noDataInfoGraphic.gameObject.SetActive(LastFive.Count == 0);
            accountsHolder.gameObject.SetActive(LastFive.Count != 0);
            allTransactionBtn.interactable = LastFive.Count != 0;
            foreach (var item in transactionHolders)
            {
                item.gameObject.SetActive(false);
            }
            for (int i = 0; i < transactionHolders.Length && i < LastFive.Count; i++)
            {
                transactionHolders[i].InitTransaction(LastFive[i]);
                transactionHolders[i].gameObject.SetActive(true);
            }
            InitializeBudget();
            var sortedAccounts = TransactionsManager.GetAllAccounts(AllTransactions);
            int count = 0;
            foreach (var item in sortedAccounts)
            {
                var SortedTransaction = item.Value;
                string accountName = item.Key;
                float totalSpend = item.Value.Where(x => x.Type == TransactionType.debit && x.TransactionDate.Month == todaysDate.Month && x.TransactionDate.Year == todaysDate.Year).Sum(x => x.TransactionAmount);
                bool autoMode = item.Value[0].AccountInfo.autoMode;
                float balanceAmount = TransactionsManager.GetAvailableBalance(SortedTransaction);
                string bankName = item.Value != null ? item.Value[0].AccountInfo.BankInfo.BankName : "";
                if (SortedTransaction.Count == 0 || item.Key == TransactionsManager.cashAccountName ||bankName=="NONE")
                {
                    continue;
                }
                AccountHolder accountHolder = GetAccountHolder(count);
                Sprite bankIcon = AppResources.GetSpriteGroup(R_Drawables.BankIcons).GetSpriteResource(bankName).sprite;
                accountHolder.InitAccount(accountName, "₹" + (balanceAmount == 0 ? "N/A" : balanceAmount), "₹" + totalSpend, bankName, bankIcon);
                count++;
            }
            Account cashAccount = data.Accounts.Find(x => x.BankInfo.BankName == TransactionsManager.cashAccountName);
            if (cashAccount != null)
            {
                AccountHolder accountHolder = GetAccountHolder(count);
                if(cashAccount.Balance==0)
                {
                    Debug.Log("Cash Account Balance is 0");
                    cashAccount.Balance= TransactionsManager.GetAvailableBalance(AllTransactions.Where(x => x.AccountInfo.AccountNumber == cashAccount.AccountNumber).ToList());
                }
                string bankName = cashAccount.BankInfo.BankName;
                Sprite bankIcon = AppResources.GetSpriteGroup(R_Drawables.BankIcons).GetSpriteResource(bankName).sprite;
                float totalSpend = AllTransactions.Where(x => x.Type == TransactionType.debit && x.TransactionDate.Month == todaysDate.Month
                && x.TransactionDate.Year == todaysDate.Year && x.Type == TransactionType.debit && x.AccountInfo.AccountNumber == TransactionsManager.cashAccountName).Sum(x => x.TransactionAmount);
                accountHolder.InitAccount(cashAccount.AccountNumber, "₹" + cashAccount.Balance, "₹" + totalSpend, bankName, bankIcon);
                count = 1;
            }
            yield return null;
        }
        private void InitializeBudget()
        {
            if (data.BudgetValue > 0)
            {
                setBudgetHolder.SetActive(false);
                BudgetSetHolder.SetActive(true);
                UpdateBudgetValues();
            }
            else
            {
                BudgetSetHolder.SetActive(false);
                setBudgetHolder.SetActive(true);
            }
        }
       
        public void OpenBudgetUI()
        {
            budgetCharacter.Init();
            budgetSetInput.onValueChanged.RemoveAllListeners();
            budgetSetInput.onValueChanged.AddListener((value) =>
            {
                budgetCharacter.ChangeBehaviour(value);
            });
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Home, UIScreens.Budget, TransitionType.Vertical);
            keyBoardTrigger.ShowKeyboard();
            VirtualKeyboard.SetSubmitAction(() => { SetBudget(); });
            BackButtonHandler.SetBackAction(() => { BudgetBackBtn(); });   
        }
        public void BudgetBackBtn()
        {
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Budget, UIScreens.Home, TransitionType.Vertical);
            keyBoardTrigger.HideKeyboard();
        }
        void SetBudget()
        {
            int.TryParse(budgetSetInput.text, out int budget);
            data.BudgetValue = budget;
            AppManager.instance.SaveData();
            BackButtonHandler.ClearBackActions();
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Budget, UIScreens.Home, TransitionType.Vertical);
            InitializeBudget();
        }
        void UpdateBudgetValues()
        {
            float budget = data.BudgetValue;
            float budgetPercent = (totalDebitAmount / budget) * 100;
            float leftBudget = budget - totalDebitAmount;
            DateTime lastDayOfMonth = new DateTime(todaysDate.Year, todaysDate.Month, DateTime.DaysInMonth(todaysDate.Year, todaysDate.Month));
            int daysLeftInMonth = (lastDayOfMonth - todaysDate).Days + 1;
            float safePerDayAmount = (leftBudget / daysLeftInMonth);
            budgetslider.maxValue = budget;
            if (totalDebitAmount >= budget)
                budgetslider.value = budgetslider.maxValue;
            else
                budgetslider.value = totalDebitAmount;
            budgetPercentTxt.text = "Budget Spent " + budgetPercent.ToString("f0") + "%";
            if (safePerDayAmount > 0)
                budgetPerDayTxt.text = "safe to spend ₹" + safePerDayAmount.ToString("f0") + "/day";
            else
                budgetPerDayTxt.text = "OVER BUDGET!";
        }
    }
}
