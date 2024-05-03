
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole.MM {
    public class ObjectCreator : MonoBehaviour
    {
        #region Public Varaibles
        [SerializeField] private Image bgTint;
        [SerializeField] private TextMeshProUGUI titleTxt;
        [SerializeField] private RectTransform holder;

        [SerializeField] private TMP_InputField accountNumberInput;
        [SerializeField] private TMP_InputField bankNameInput;
        [SerializeField] private TMP_InputField accountBalanceInput;
        [SerializeField] private TMP_Dropdown bankNamesDropDown;
        [SerializeField] private TextMeshProUGUI errorTxt;
        [SerializeField] private TextMeshProUGUI bankFieldTxt;
        [SerializeField] private Button createBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private Color tintColor;
        public Action<Account> OnAccountCreated;
        private bool isCreatingNewBank;

        private SpriteResourceLibrary Banks;
        #endregion
        #region Private Varaibles
        #endregion
        private void Awake()
        {
            AccountHolder.OnAccountDetailsBtnClicked += AccountHolder_OnAccountDetailsBtnClicked;
            cancelBtn.onClick.RemoveAllListeners();
            cancelBtn.onClick.AddListener(() =>
            {
                ShowHideCreationPanel(false);
            });
            Button tintBackBtn = bgTint.GetComponent<Button>();
            tintBackBtn.onClick.RemoveAllListeners();
            tintBackBtn.onClick.AddListener(() =>
            {
                ShowHideCreationPanel(false);
            });
        }

        private void OnDestroy()
        {
            AccountHolder.OnAccountDetailsBtnClicked -= AccountHolder_OnAccountDetailsBtnClicked;
        }
        private void AccountHolder_OnAccountDetailsBtnClicked(string obj)
        {
            List<Account> AlLAccounts= AppManager.instance.GetSMSData().Accounts;
            foreach (var item in AlLAccounts)
            {
                print(item.AccountNumber);
                print(item.Balance);
            }
            //find account with account number
            Account account = AlLAccounts.Find(x => x.AccountNumber == obj);
            if (account != null)
            {
                //open account details panel
                EditAccountDetails(account);
                createBtn.onClick.RemoveAllListeners();
                createBtn.onClick.AddListener(() =>
                {
                    float balance = 0;
                    float.TryParse(accountBalanceInput.text, out balance);
                    account.Balance = balance;
                    ShowHideCreationPanel(false);
                    AppManager.instance.SaveData();
                });
            }
            else
            {
                Log("Account Not Found");
            }
            
        }

        public void ShowHideCreationPanel(bool show)
        {
            float height = holder.rect.height + 100;
            if (show)
            {
                bgTint.gameObject.SetActive(true);
                holder.gameObject.SetActive(true);
                holder.DOAnchorPosY(0, 0.4f).From(Vector2.up * height).SetEase(Ease.OutBack);
                bgTint.material.DOColor(tintColor, 0.5f).From(Color.white);
                bgTint.material.DOFloat(3, "_Size", 0.5f).From(0);
            }
            else
            {
                holder.DOAnchorPosY(height, 0.4f).From(Vector2.zero).SetEase(Ease.InBack).OnComplete(() =>
                {
                    holder.gameObject.SetActive(false);
                    bgTint.gameObject.SetActive(false);
                });
                bgTint.material.DOColor(Color.white, 0.5f).From(tintColor);
                bgTint.material.DOFloat(0, "_Size", 0.5f).From(3);
            }
        }
        public void OpenAccountCreationPanel(Action<Account> OnAccountCreated)
        {
            TouchScreenKeyboard.Android.consumesOutsideTouches = false;
            ShowHideCreationPanel(true);
            createBtn.onClick.RemoveAllListeners();
            bankFieldTxt.text = "Select Bank";
            this.OnAccountCreated = OnAccountCreated;
            createBtn.onClick.AddListener(CreateNewAccount);
            accountNumberInput.gameObject.SetActive(true);
            accountBalanceInput.gameObject.SetActive(true);
            bankNamesDropDown.gameObject.SetActive(true);
            bankNameInput.gameObject.SetActive(false);
            Banks = AppResources.GetSpriteGroup(R_Drawables.BankIcons);
            accountNumberInput.Select();

            var AllAccounts = AppManager.instance.GetSMSData().Accounts;
            List<TMP_Dropdown.OptionData> accountOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var item in AllAccounts)
            {
                if (!Banks.spriteResources.Exists(x => x.name == item.BankInfo.BankName))
                {
                    accountOptions.Add(new TMP_Dropdown.OptionData(item.BankInfo.BankName));
                }
            }
            foreach (var bank in Banks.spriteResources)
            {
                accountOptions.Add(new TMP_Dropdown.OptionData(bank.name, bank.sprite));
            }
           
            accountOptions.Add(new TMP_Dropdown.OptionData("Add New", AppResources.GetSpriteGroup(R_Drawables.AppGraphics).GetSpriteResource(R_AppGraphics.AddNew.ToString()).sprite));
            bankNamesDropDown.ClearOptions();
            bankNamesDropDown.AddOptions(accountOptions);
            bankNamesDropDown.onValueChanged.RemoveAllListeners();
            bankNamesDropDown.onValueChanged.AddListener((int index) =>
            {
                //check if add new category is selected
                if (accountOptions[index].text == "Add New")
                {
                    //open add new category screen
                    bankNamesDropDown.gameObject.SetActive(false);
                    bankNameInput.gameObject.SetActive(true);
                    isCreatingNewBank = true;
                  //  Debug.Log("Open new account creation screen");
                    return;

                }

            });
        }
        public void EditAccountDetails(Account account)
        {
            bankFieldTxt.text = "Bank";
            accountNumberInput.interactable = false;
            bankNameInput.interactable = false;
            bankNamesDropDown.gameObject.SetActive(false);
            bankNameInput.gameObject.SetActive(true);
            bankNameInput.text = account.BankInfo.BankName;
            accountNumberInput.text = account.AccountNumber;
            accountBalanceInput.text = account.Balance.ToString();
            ShowHideCreationPanel(true);
        }
        public void CreateNewAccount()
        {
          if (accountNumberInput.text.Length==0)
            {
                ShowError("Please Enter Account Number");
                return;
            }
            if (bankNameInput.text.Length == 0&&isCreatingNewBank)
            {
                ShowError("Please Enter Bank Name");
                return;
            }
            if (accountBalanceInput.text.Length == 0)
            {
                ShowError("Please Enter Account Balance");
                return;
            }
            string bankName = "Other";
            if (isCreatingNewBank)
            {
                bankName = bankNameInput.text;
            }
            else
            {
                bankName = bankNamesDropDown.options[bankNamesDropDown.value].text;
            }
            var account=CreateAndAddAccount(accountNumberInput.text, bankName, accountBalanceInput.text);
            errorTxt.text = "";
            accountNumberInput.text = "";
            bankNameInput.text = "";
            accountBalanceInput.text = "";
            ShowHideCreationPanel(false);
            OnAccountCreated?.Invoke(account);
        }
        public Account CreateAndAddAccount(string accountNumber,string BankName,string balance)
        {
            Account account = new Account();
            var canParse = float.TryParse(balance, out account.Balance);
            account.BankInfo = new BankInfo();
            account.autoMode = false;
            account.AccountNumber = accountNumber;
            account.BankInfo.BankName = BankName;
            account.BankInfo.Id = 0;
            return account;
        }
        void ShowError(string message)
        {
            errorTxt.text = message;
            errorTxt.gameObject.SetActive(true);

        }
        public void Log(string message, int logType = 0)
        {
            string log = this.name + ": " + message;
            switch (logType)
            {
                case 0:
                    Debug.Log(log);
                    break;
                case 1:
                    Debug.LogWarning(log);
                    break;
                case 2:
                    Debug.LogError(log);
                    break;
                default:
                    Debug.Log(log);
                    break;
            }
        }
    }
}