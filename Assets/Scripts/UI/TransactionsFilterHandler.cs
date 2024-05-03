using DG.Tweening;
using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
namespace CubeHole.MM {
    public class TransactionsFilterHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI totalSpendsTxt;
        [SerializeField] private TextMeshProUGUI totalIncomeTxt;
        [SerializeField] private TextMeshProUGUI fromDateTxt;
        [SerializeField] private TextMeshProUGUI toDateTxt;
        [SerializeField] private RectTransform navigationPointer;
        [SerializeField] private RectTransform[] navigationButtons;
        [SerializeField] private RectTransform[] transactionStyles;
        [SerializeField] private TransactionRecyclerViewHandler filteredTransactions;
        [SerializeField] private BarGraphHandler barGraphHandler;
        [SerializeField] private PieChartHandler pieChartHandler;

        [SerializeField] private RectTransform customFilterHolder;
        [SerializeField] private TextMeshProUGUI customFilterHeaderTxt;
        [SerializeField] private GameObject defaultFilterTag;
        [SerializeField] private FilterLabel customFilterTagPrefab;
        [SerializeField] private Transform customFilterTagParent;
        [SerializeField] private FiltersHolder filtersHolder;
        private List<FilterLabel> filterLabels = new List<FilterLabel>();
        private SMSData smsData;
        private bool canInteract;
        private int previousSelectedIndex;
        private DateTime fromDate;
        private DateTime toDate;
        public string fromDateInput;
        public string toDateInput;
        public List<string> selectedAccounts= new List<string>();
        private Sprite emptySprite;
        private void Awake()
        {
            customFilterHolder.gameObject.SetActive(false);
            ToggleSwitch.OnToggleStateChanged += OnCustomFilterStatusChanged;
            FilterLabel.OnFilterRemoved += RemoveFilterTags; ;
        }
        private void OnDestroy()
        {
            ToggleSwitch.OnToggleStateChanged -= OnCustomFilterStatusChanged;
            FilterLabel.OnFilterRemoved -= RemoveFilterTags;
        }
        public void Init(SMSData data)
        {
            smsData = data;
            List<string> AllAccountNumbers= new List<string>();
            AllAccountNumbers=smsData.Accounts.Select(x=>x.AccountNumber).ToList();
            filtersHolder.Init(AllAccountNumbers);
        }
        public void ShowFiteredTransactions()
        {
            selectedAccounts.Clear();
            previousSelectedIndex = 0;
            canInteract=true;
            foreach (var item in transactionStyles)
            { 
                item.gameObject.SetActive(true);
                item.anchoredPosition = new Vector2(item.rect.width, 0);
            }
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Home, UIScreens.AllTransactions, TransitionType.Vertical, true);
            toDate = HomeScreen.todaysDate;
            fromDate = new DateTime(toDate.Year, toDate.Month, 1);
            toDateInput = toDate.ToString("dd/MM/yyyy");
            fromDateInput = fromDate.ToString("dd/MM/yyyy");
            ChangeViewStyle(1);
            FilterTransactions();
            ClearFilters();  
        }
        [ButtonMethod]
        public void FilterTransactions()
        {
            bool canParseFromDate;
            bool canParseToDate;
            canParseFromDate= DateTime.TryParse(fromDateInput,out fromDate);
            canParseToDate = DateTime.TryParse(toDateInput, out toDate);
            Logger.Log("canParseFromDate: " + canParseFromDate + " canParseToDate: " + canParseToDate);
            if(!(canParseFromDate && canParseToDate))
            {
                PopUp.ShowPopUp("Something went wrong! Please try again");
                return;
            }
            if(fromDate>toDate)
            {
                PopUp.ShowPopUp("From Date is greater than To Date");
                Logger.LogError("From Date is greater than To Date");
                return;
            }
            StartCoroutine(LoadData());
        }
        public void ChangeViewStyle(int type)
        {
            if (!canInteract)
                return;
            canInteract= false;
            Vector2 fromPosition = navigationButtons[previousSelectedIndex].anchoredPosition;
            Vector2 targetPosition = navigationButtons[type].anchoredPosition;
            navigationPointer.DOAnchorPosX(targetPosition.x, 0.5f).From(fromPosition).SetEase(Ease.OutBack).OnComplete(() => {
                canInteract = true;
            });
            RectTransform previousScreen = transactionStyles[previousSelectedIndex];
            RectTransform targetScreen = transactionStyles[type];
            float previousScreenX = type > previousSelectedIndex ? previousScreen.rect.width : -previousScreen.rect.width;
            float targetScreenX = type > previousSelectedIndex ? -targetScreen.rect.width : targetScreen.rect.width;
            previousScreen.DOAnchorPosX(previousScreenX, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
            {
               // previousScreen.gameObject.SetActive(false);
            });
            targetScreen.gameObject.SetActive(true);
            targetScreen.DOAnchorPosX(0, 0.5f).From(new Vector2(targetScreenX,0)).SetEase(Ease.Linear);
            previousSelectedIndex = type;
        }
        IEnumerator LoadData()
        {
            yield return null;
            fromDateTxt.text = "From Date\n" + fromDate.ToString("dd MMM yyy");
            toDateTxt.text = "To Date\n" + toDate.ToString("dd MMM yyy");


            var FilteredTransactions = TransactionsManager.FilterTransactionByDate(fromDate,toDate, smsData.Transactions);
            if (selectedAccounts.Count > 0)
                FilteredTransactions = TransactionsManager.FilterTransactionsByAccount(selectedAccounts, FilteredTransactions);
            var FilteredData=TransactionsManager.ExtractDataFromTransactions(FilteredTransactions);
            totalSpendsTxt.text =TransactionsManager.ruppeSymbol+  FilteredData[MM.FilteredData.TotalDebit].ToString();
            totalIncomeTxt.text = TransactionsManager.ruppeSymbol + FilteredData[MM.FilteredData.TotalCredit].ToString();

            filteredTransactions.Init(FilteredTransactions);
            List<BarGraphInfo> barGraphData = new List<BarGraphInfo>();
            //Grouping Transactions by Date
            var groupedTransactions = TransactionsManager.GroupTransactionsByDate(FilteredTransactions);
            Color spendsColor = AppResources.GetColorGroup(R_Colors.AppColors).GetColorResource(R_AppColors.Orange.ToString());
            Color incomeColor = AppResources.GetColorGroup(R_Colors.AppColors).GetColorResource(R_AppColors.Green.ToString());
            foreach (var item in groupedTransactions)
            {
                BarGraphInfo barGraphHolder = new BarGraphInfo();
                barGraphHolder.label=item.Key.ToString("dd MMM");
                var FilteredDataByDate=TransactionsManager.ExtractDataFromTransactions(item.Value);
                BarGraphData spendsData = new BarGraphData();
                spendsData.value = FilteredDataByDate[MM.FilteredData.TotalDebit];
                spendsData.color = spendsColor;
                spendsData.label = TransactionsManager.ruppeSymbol + FilteredDataByDate[MM.FilteredData.TotalDebit];
                spendsData.type = BarType.Vertical;
                BarGraphData incomeData = new BarGraphData();
                incomeData.value = FilteredDataByDate[MM.FilteredData.TotalCredit];
                incomeData.color = incomeColor;
                incomeData.label = TransactionsManager.ruppeSymbol + FilteredDataByDate[MM.FilteredData.TotalCredit];
                incomeData.type = BarType.Vertical;
                barGraphHolder.data = new List<BarGraphData>();
                barGraphHolder.data.Add(spendsData);
                barGraphHolder.data.Add(incomeData);
                barGraphData.Add(barGraphHolder);
            }
            barGraphHandler.Init(barGraphData);

            List<BarGraphData> pieCharData=new List<BarGraphData>();
            var groupedTransactionsByCategory=TransactionsManager.GroupTransactionsByCategory(FilteredTransactions);
            foreach (var item in groupedTransactionsByCategory)
            {
                BarGraphData pieChartData = new BarGraphData();
                pieChartData.value = item.Value.Count;
                pieChartData.color = AppResources.GetSpriteGroup(R_Drawables.DebitCategories).GetSpriteResource(item.Key.ToString()).defaultColor;
                pieChartData.label = item.Key;
                pieCharData.Add(pieChartData);
            }

            pieChartHandler.Init(pieCharData);
            print("Refresh data");
        }

        public void SelectFromDate(bool fromDate)
        {
            AndroidUtils.SetDatePickCallBack((date) =>
            {
                if (fromDate)
                    fromDateInput = date.ToString("dd MMM yyy");
                else
                    toDateInput = date.ToString("dd MMM yyy");
            });
            AndroidUtils.SendToAndroid(AndroidFunctions.OpenDatePicker);
            fromDateTxt.text = "From Date\n" + fromDateInput;
            toDateTxt.text = "To Date\n" + toDateInput;
            FilterTransactions();
        }
        private void RemoveFilterTags(string obj)
        {
            selectedAccounts.Remove(obj);
            if (selectedAccounts.Count == 0)
            {
                defaultFilterTag.SetActive(true);
            }
            var filterLabel = filterLabels.Find(x => x.label == obj);
            if (filterLabel != null)
            {
                filterLabel.gameObject.SetActive(false);
            }
            print($"Removing filter label {obj}");
            FilterTransactions();
        }
        public void ShowHideCustomFilters(bool show)
        {
            if (show)
            {
                customFilterHolder.gameObject.SetActive(true);
                customFilterHolder.DOAnchorPosY(0, 0.4f).From(Vector2.down* (customFilterHolder.rect.height + 10));
            }
            else
            {
                customFilterHolder.DOAnchorPosY(-(customFilterHolder.rect.height + 10), 0.4f).From(Vector2.zero).OnComplete(() => { 
                
                customFilterHolder.gameObject.SetActive(false);
                });
            }
        }
        public void ClearFilters()
        {
            selectedAccounts.Clear();
            foreach (var item in filterLabels)
            {
                item.gameObject.SetActive(false);
            }
            defaultFilterTag.SetActive(true);
            filtersHolder.RemoveAllFilters();
            FilterTransactions();
        }
        private void OnCustomFilterStatusChanged(string arg1, bool arg2)
        {
            if (arg2)
            {
                selectedAccounts.Add(arg1);
                defaultFilterTag.SetActive(false);
                if (selectedAccounts.Count > filterLabels.Count)
                {
                    FilterLabel filterLabel = Instantiate(customFilterTagPrefab, customFilterTagParent);
                    filterLabel.Init(arg1);
                    filterLabels.Add(filterLabel);
                }
                else
                {
                    var filterLabel = GetInactiveFilterLabel();
                    if (filterLabel != null)
                    {
                        filterLabel.gameObject.SetActive(true);
                        filterLabel.Init(arg1);
                    }
                  
                }
                FilterTransactions();
            }
            else
            {
                RemoveFilterTags(arg1);
            }
        }
        FilterLabel GetInactiveFilterLabel()
        {
            foreach (var item in filterLabels)
            {
                if (!item.gameObject.activeSelf)
                    return item;
            }
            return null;
        }
    }
}
