using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AccountHolder : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI accountNumberTxt;
    [SerializeField] private TextMeshProUGUI accountBalanceTxt;
    [SerializeField] private TextMeshProUGUI accountSpendTxt;
    [SerializeField] private TextMeshProUGUI accountBankName;
    [SerializeField] private Image accountBankIcon;
    [SerializeField] private Button accountDetailsBtn;
    public static event Action<string> OnAccountDetailsBtnClicked;
    public void InitAccount(string accountNumber,string accountBalance,string accountSpend, string bankName, Sprite accountSprite=null)
    {
        accountNumberTxt.text = accountNumber ;
        accountBalanceTxt.text ="Balance "+ accountBalance;
        accountSpendTxt.text = accountSpend + " Spent";
        accountBankName.text = bankName;
        if (accountSprite != null)
        {
            accountBankIcon.sprite = accountSprite;
        }
        accountDetailsBtn.onClick.RemoveAllListeners();
        accountDetailsBtn.onClick.AddListener(() =>
        {
            OnAccountDetailsBtnClicked?.Invoke(accountNumber);
        });
    }

}
