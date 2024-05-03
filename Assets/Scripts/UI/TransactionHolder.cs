using PolyAndCode.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
namespace CubeHole.MM
{
    public class TransactionHolder : MonoBehaviour, ICell
    {
        [SerializeField] private TextMeshProUGUI transactionAmountTxt;
        [SerializeField] private TextMeshProUGUI transactionDateTxt;
        [SerializeField] private TextMeshProUGUI transactionAccountTxt;
        [SerializeField] private RectTransform transactionTypeImg;
        [SerializeField] private Image categoryIconImg;
        [SerializeField] private Button selectCategoryBtn;
        [SerializeField] private Button detailViewBtn;
        public static event Action<TransactionHolder> OnSelectCategory;
        public static event Action<TransactionHolder> OnViewTransaction;
        public Transaction myTransaction;
        public void InitTransaction(Transaction transaction)
        {
            myTransaction = transaction;
            UpdateCategory(AppResources.GetSpriteGroup(myTransaction.Type==TransactionType.debit?R_Drawables.DebitCategories:R_Drawables.CreditCategories).GetSpriteResource(myTransaction.Category));
            transactionAccountTxt.text = transaction.CreditedAccountName;
            transactionDateTxt.text = TransactionsManager.GetDate(transaction.TransactionDateString);
            transactionAmountTxt.text = "₹" + transaction.TransactionAmount.ToString();
            transactionTypeImg.localEulerAngles = Vector3.forward * (transaction.Type == TransactionType.credit ? 180 : 0);
            selectCategoryBtn.onClick.RemoveAllListeners();
            selectCategoryBtn.onClick.AddListener(() => {
                OnSelectCategory?.Invoke(this);
            });
            detailViewBtn.onClick.RemoveAllListeners();
            detailViewBtn.onClick.AddListener(() =>
            {
                OnViewTransaction?.Invoke(this);
            }); 
        }
        public void UpdateCategory(SpriteResource category)
        {
            categoryIconImg.sprite = category.sprite;
            myTransaction.Category = category.name;
            categoryIconImg.color = category.defaultColor;
        }

        public string GetCategory()
        {
            return myTransaction.Category;
        }
    }
}
