using DG.Tweening;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.UI.Extensions;

namespace CubeHole.MM
{
    public class TransactionViewer : MonoBehaviour
    {
        [SerializeField] private RectTransform transactionDetailsHolder;
        [SerializeField] private ObjectCreator objectCreator;
        #region Normal Varables
        [SerializeField] private CanvasGroup normalModeHolder;
        [SerializeField] private TextMeshProUGUI transactionAmountTxt;
        [SerializeField] private TextMeshProUGUI transactionDateTxt;
        [SerializeField] private TextMeshProUGUI transactionTypeTxt;
        [SerializeField] private Image transactionTypeImg;
        [SerializeField] private TextMeshProUGUI transactionNameTxt;
        [SerializeField] private TextMeshProUGUI transactionMessageTxt;
        [SerializeField] private TMP_Dropdown transactionBankAccountDropDown;
        [SerializeField] private TMP_Dropdown transactionCategoryDropDown;
        [SerializeField] private ToggleButton IsExpenseOrIcomeToggle;
        [SerializeField] private TextMeshProUGUI IsExpenseOrIcomeToggleTxt;
        #endregion
        #region Edit Mode Variables
        [SerializeField] private CanvasGroup editModeHolder;
        [SerializeField] private TMP_InputField transactionAmountInput;
        [SerializeField] private TMP_InputField transactionNameInput;
        [SerializeField] private TMP_InputField transactionDescriptionInput;
        [SerializeField] private TextMeshProUGUI transactionTypeEditTxt;
        [SerializeField] private TextMeshProUGUI transactionDateEditTxt;
        [SerializeField] private TextMeshProUGUI errorTxt;
        [SerializeField] private Image transactionTypeEditImg;
        [SerializeField] private Image outlineMaskImg;
        [SerializeField] private Image editModeTintImg;
        [SerializeField] private Button saveBtn;
        #endregion
        [SerializeField] private HomeScreen homeScreen;
        private Transaction transaction;
        private bool canToggle;
        private List<Account> AllAccounts;
        private bool isNewTransaction=false;
        private bool hasEdited=false;
        private void Awake()
        {
            TransactionHolder.OnViewTransaction += TransactionHolder_OnViewTransaction;
            // transactionViewerUI.gameObject.SetActive(false);
            TouchScreenKeyboard.Android.consumesOutsideTouches = false;
            TouchScreenKeyboard.hideInput = true;
        }

        private void ObjectCreator_OnAccountCreated(Account account)
        {
            Logger.Log($"Account Created {account.AccountNumber}");
            AllAccounts.Add(account);
            RefreshAccounts();
        }

        private void TransactionHolder_OnViewTransaction(TransactionHolder holder)
        {
           InitTransaction(holder.myTransaction);
        }

        private void OnDestroy()
        {
            TransactionHolder.OnViewTransaction -= TransactionHolder_OnViewTransaction;
        }
        public void Initialise(List<Account> accounts)
        {
            AllAccounts = accounts;
        }
        private void InitTransaction(Transaction myTransaction)
        {
            isNewTransaction = false;
            hasEdited = false;
            transaction=myTransaction;
            outlineMaskImg.gameObject.SetActive(false);
            editModeTintImg.gameObject.SetActive(false);
            editModeHolder.gameObject.SetActive(false);
            normalModeHolder.gameObject.SetActive(true);
            normalModeHolder.alpha = 1;
            transactionDetailsHolder.anchoredPosition = Vector2.up * -120;

            StartCoroutine(InitViewer(myTransaction));

            transactionMessageTxt.gameObject.SetActive(true);
            transactionDescriptionInput.gameObject.SetActive(false);
            
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.TransactionViewer, TransitionType.Vertical, true);

        }
        IEnumerator InitViewer(Transaction transaction)
        {
            transactionCategoryDropDown.ClearOptions();
            List<TMP_Dropdown.OptionData> categoryOptions = new List<TMP_Dropdown.OptionData>();
            SpriteResourceLibrary categories = AppResources.GetSpriteGroup(transaction.Type == TransactionType.debit ? R_Drawables.DebitCategories : R_Drawables.CreditCategories);
            foreach (var item in categories.spriteResources)
            {
                categoryOptions.Add(new TMP_Dropdown.OptionData(item.name, item.sprite));
            }
            transactionCategoryDropDown.AddOptions(categoryOptions);
            transactionCategoryDropDown.onValueChanged.RemoveAllListeners();
            transactionCategoryDropDown.onValueChanged.AddListener((int index) =>
            {
                transactionDescriptionInput.gameObject.SetActive(true);
                //check if add new category is selected
                if (categoryOptions[index].text == "Add New")
                {
                    //open add new category screen
                    Debug.Log("Open new category creation screen");
                    return;
                }
                transactionCategoryDropDown.captionImage.color = categories.spriteResources[index].defaultColor;
                hasEdited = true;
                Debug.Log($"Category changed to {categoryOptions[index].text}");
             
            });
            RefreshAccounts();
            transactionNameTxt.text = transaction.CreditedAccountName;
            transactionAmountTxt.text = TransactionsManager.GetAmountString(transaction.TransactionAmount);
            transactionDateTxt.text = transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss");
            transactionTypeTxt.text = transaction.Type.ToString().ToUpper();
            transactionTypeImg.transform.localEulerAngles = new Vector3(0, 0, transaction.Type == TransactionType.credit ? 180 : 0);
            transactionCategoryDropDown.value = categories.spriteResources.FindIndex(x => x.name == transaction.Category);
            transactionCategoryDropDown.captionImage.color = categories.spriteResources[transactionCategoryDropDown.value].defaultColor;
            transactionBankAccountDropDown.value = AllAccounts.FindIndex(x => x.BankInfo.BankName == transaction.AccountInfo.BankInfo.BankName);
            transactionMessageTxt.text = transaction.TransactionSMS == "" ? "Enter transaction details" : transaction.TransactionSMS;
            IsExpenseOrIcomeToggle.ToggleGroup(transaction.IsTransfer);
            IsExpenseOrIcomeToggleTxt.text = "Is Transfer";
            yield return null;

        }
        void RefreshAccounts()
        {
            transactionBankAccountDropDown.ClearOptions();
            SpriteResourceLibrary Banks = AppResources.GetSpriteGroup(R_Drawables.BankIcons);
            List<TMP_Dropdown.OptionData> accountOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var item in AllAccounts)
            {
                SpriteResource bankIcon = Banks.GetSpriteResource(item.BankInfo.BankName);
                accountOptions.Add(new TMP_Dropdown.OptionData(item.AccountNumber, bankIcon.sprite));
            }
            accountOptions.Add(new TMP_Dropdown.OptionData("Add New", AppResources.GetSpriteGroup(R_Drawables.AppGraphics).GetSpriteResource(R_AppGraphics.AddNew.ToString()).sprite));
            transactionBankAccountDropDown.AddOptions(accountOptions);
            transactionBankAccountDropDown.onValueChanged.RemoveAllListeners();
            transactionBankAccountDropDown.onValueChanged.AddListener((int index) =>
            {
                transactionDescriptionInput.gameObject.SetActive(true);
                //check if add new category is selected
                if (accountOptions[index].text == "Add New")
                {
                    //open add new category screen
                    objectCreator.OpenAccountCreationPanel(ObjectCreator_OnAccountCreated);
                    transactionBankAccountDropDown.value = AllAccounts.Count-1;
                    Debug.Log("Open new account creation screen");
                    return;

                }
                hasEdited = true;
            });
        }
        public void OnDropDownsSelected()
        {
            transactionDescriptionInput.gameObject.SetActive(false);
        }
        public void CreateNewTransaction(bool isCredit)
        {
            saveBtn.interactable = false;
            isNewTransaction = true;
            Transaction newTransaction = new Transaction();
            transaction = newTransaction;
            transaction.AccountInfo = TransactionsManager.GetCashAccount();


            transaction.Type = isCredit ? TransactionType.credit : TransactionType.debit;
            transactionDateEditTxt.text= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            transactionTypeEditTxt.text = isCredit ? "Credit" : "Debit";
            IsExpenseOrIcomeToggleTxt.text = "Is Transfer";
            transactionTypeEditImg.transform.localEulerAngles = new Vector3(0, 0, isCredit ? 180 : 0);
            transactionTypeEditImg.color = isCredit ? Color.green : Color.red;
            transactionAmountInput.text = "";
            transactionNameInput.text = "";
            transactionAmountInput.placeholder.GetComponent<TextMeshProUGUI>().text = TransactionsManager.ruppeSymbol + "Enter Amount";
            transactionNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter transaction name"; 
            transactionDescriptionInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter transaction description";
            transactionMessageTxt.gameObject.SetActive(false);
            transactionDescriptionInput.gameObject.SetActive(true);
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Home, UIScreens.TransactionViewer, TransitionType.Vertical, true, () => { 
                PlayEditAnimation();
            });
            StartCoroutine(InitViewer(transaction));
            //transactionScreen.ChangeScreen(true, () => {
            //});
        }
        private void PlayEditAnimation()
        {
            canToggle = true;
            editModeTintImg.gameObject.SetActive(true);
            editModeTintImg.DOFade(0.8f, 0.5f);
            outlineMaskImg.gameObject.SetActive(true);
            outlineMaskImg.DOFade(1, 0.2f).From(0);
            outlineMaskImg.DOFillAmount(1, 0.5f).From(0);
            editModeHolder.gameObject.SetActive(true);
            editModeHolder.DOFade(1, 0.5f).From(0);
            normalModeHolder.DOFade(0, 0.5f).From(1).OnComplete(() => normalModeHolder.gameObject.SetActive(false));
            float YPos = transactionDetailsHolder.anchoredPosition.y;
            transactionDetailsHolder.DOAnchorPosY(YPos * 2, 0.5f).From(Vector2.up * YPos).OnComplete(() =>
            {
                transactionNameInput.Select();
            });
            if (!isNewTransaction)
                return;
            transactionNameInput.onEndEdit.AddListener((string value) =>
            {
                if (value == "")
                {
                    ShowError("Please enter transaction name",transactionNameInput.gameObject);
                }
                else
                {
                   transactionAmountInput.Select();
                }
            });
            transactionAmountInput.onEndEdit.AddListener((string value) =>
            {
                if (value == "")
                {
                    ShowError("Please enter transaction amount", transactionAmountInput.gameObject);
                }
                else
                {
                    saveBtn.interactable = true;
                    transactionBankAccountDropDown.Select();
                    EndEdit();
                }
            });
            transactionBankAccountDropDown.onValueChanged.AddListener((int index) =>
            {
                transactionCategoryDropDown.Select();
            });
            transactionCategoryDropDown.onValueChanged.AddListener((int index) =>
            {
                transactionDescriptionInput.Select();
            });
        }
        void ShowError(string error,GameObject gameObject)
        {
            Logger.Log($"Error: is save transaction {error}");
            if(errorTxt.gameObject.activeInHierarchy)
            {
                return;
            }
            Color orgColor = Color.white;
            if(gameObject.TryGetComponent<NicerOutline>(out NicerOutline outline))
            {
                orgColor = outline.effectColor;
                outline.effectColor = Color.red;
            }
            errorTxt.text = error;
            errorTxt.gameObject.SetActive(true);
            errorTxt.DOFade(1, 0.5f).From(0);
            errorTxt.DOFade(0, 0.5f).From(1).SetDelay(2).OnComplete(() =>
            {
                errorTxt.gameObject.SetActive(false);
                if(outline!=null)
                {
                    outline.effectColor = orgColor;
                }
                });
        }
        public void EditTransaction()
        {
            hasEdited = true;
            transactionAmountInput.text = transactionAmountTxt.text;
            transactionDateEditTxt.text = transactionDateTxt.text;
            transactionNameInput.text = transactionNameTxt.text;
            transactionDescriptionInput.text = transactionMessageTxt.text;
            transactionTypeEditTxt.text = transactionTypeTxt.text;
            transactionTypeEditImg.transform.localEulerAngles = transactionTypeImg.transform.localEulerAngles;

            PlayEditAnimation();
        }
        public void DeleteTransaction()
        {
            PopUp.ShowPopUp(HeadingTxt: "Delete Transaction", message: "Are you sure you want to delete this transaction?", yesText: "Yes", YesAction: () =>
            {
                AppManager.instance.DeleteTransaction(transaction);
                CloseTransactionViewer();
                homeScreen.IntialiseTransactions();
            }, NoText: "No", registerBack: true);
        }
        public void CancelTransaction()
        {
            ScreenSwitcher.Instance.RevertScreen();
        }
        public void CloseTransactionViewer()
        {
            CancelTransaction();
            StartCoroutine(CloseTransactionCoroutine());
        }
        IEnumerator CloseTransactionCoroutine()
        {
            transaction.CreditedAccountName = transactionNameTxt.text;
            transaction.TransactionAmount = -1;
            string amount = transactionAmountTxt.text.Replace(TransactionsManager.ruppeSymbol, "");
            bool canParse = float.TryParse(amount, out float amt);
            transaction.TransactionAmount = amt;
            DateTime convertedDate = TransactionsManager.ConvertDate(transactionDateTxt.text);
            transaction.TransactionDate = convertedDate;
            transaction.TransactionDateString = convertedDate.ToString("yyyy-MM-dd HH:mm:ss");
            transaction.TransactionSMS = transactionMessageTxt.text;
            transaction.Type = transactionTypeTxt.text == "Credit" ? TransactionType.credit : TransactionType.debit;
            transaction.Category = AppResources.GetSpriteGroup(transaction.Type == TransactionType.debit ? R_Drawables.DebitCategories : R_Drawables.CreditCategories).spriteResources[transactionCategoryDropDown.value].name;
            transaction.AccountInfo = AllAccounts[transactionBankAccountDropDown.value];

            if (isNewTransaction)
            {
                AppManager.instance.AddNewTransaction(transaction);
            }
            if (hasEdited)
            {
                Logger.Log("Transaction Edited");
                AppManager.instance.SaveData();
            }
          
            homeScreen.IntialiseTransactions();
            yield return null;  
        }
        public void EndEdit()
        {
            if(!isNewTransaction)
            {
                CancelTransaction();
                return;
            }
            if (transactionNameInput.text == "")
            {
                ShowError("Please enter transaction name", transactionNameInput.gameObject);
                return;
            }
            string amount = transactionAmountInput.text.Replace(TransactionsManager.ruppeSymbol, "");
            bool canParse = float.TryParse(amount, out float amt);
            if (amount == ""||!canParse)
            {
                ShowError("Please enter transaction amount", transactionAmountInput.gameObject);
                return;
            }
            editModeTintImg.DOFade(0, 0.5f).OnComplete(() => editModeTintImg.gameObject.SetActive(false));
            outlineMaskImg.DOFade(0, 0.2f).OnComplete(() => outlineMaskImg.gameObject.SetActive(false));
            editModeHolder.DOFade(0, 0.5f).OnComplete(() => editModeHolder.gameObject.SetActive(false));
            normalModeHolder.gameObject.SetActive(true);
            normalModeHolder.DOFade(1, 0.5f);
            float YPos = transactionDetailsHolder.anchoredPosition.y;
            transactionDetailsHolder.DOAnchorPosY(YPos / 2, 0.5f).From(Vector2.up * YPos);
            transactionNameTxt.text = transactionNameInput.text;
            transactionAmountTxt.text = transactionAmountInput.text;
            transactionDateTxt.text = transactionDateEditTxt.text;
            transactionMessageTxt.text = transactionDescriptionInput.text;
            transactionTypeTxt.text = transactionTypeEditTxt.text;
            transactionTypeImg.transform.localEulerAngles = transactionTypeEditImg.transform.localEulerAngles;
        }
        public void ExpenseToggle(bool isExpense)
        {
            transaction.IsTransfer = isExpense;
        }
        public void OnDateEdit()
        {
            AndroidUtils.SendToAndroid(AndroidFunctions.OpenDatePicker);
            AndroidUtils.SetDatePickCallBack((date) =>
            {
                transactionDateEditTxt.text=date.ToString("yyyy-MM-dd HH:mm:ss");
            });
        }
        public void OnEndAmountEdit()
        {
            if (!transactionAmountInput.text.Contains(TransactionsManager.ruppeSymbol))
                transactionAmountInput.text = TransactionsManager.ruppeSymbol + transactionAmountInput.text;
        }
        public void DescriptionEdit()
        {
            transactionMessageTxt.gameObject.SetActive(false);
            transactionDescriptionInput.gameObject.SetActive(true);
            transactionDescriptionInput.Select();
            transactionDescriptionInput.text = transactionMessageTxt.text;
        }
        public void OnEndDescriptionEdit()
        {
            transactionMessageTxt.text= transactionDescriptionInput.text;
            if (transactionDescriptionInput.text == "")
                return;
            transactionMessageTxt.gameObject.SetActive(true);
            transactionDescriptionInput.gameObject.SetActive(false);
        }
        public void ToggleTransactionType()
        {
            if (!canToggle)
                return;
            canToggle= false;
            transaction.Type = transaction.Type == TransactionType.credit ? TransactionType.debit : TransactionType.credit;
            float targetAngle = transaction.Type == TransactionType.credit ? 180 : 0;
            transactionTypeEditImg.transform.DOLocalRotate(new Vector3(0, 0, targetAngle), 0.5f).OnComplete(() => canToggle = true);
            transactionTypeEditImg.color = targetAngle==180 ? Color.green : Color.red;
            transactionTypeEditTxt.text = transaction.Type.ToString().ToUpper();
        }
     
    }
}
