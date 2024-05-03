using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole.MM {
    public class CategorySelector : MonoBehaviour
    {

        [SerializeField] private CategoryIconHolder categoryIconHolder;
        [SerializeField] private Transform categoriesHolder;
        [SerializeField] private HorizontalLayoutGroup categorySelectorHolder ;
        [SerializeField] private CanvasGroup holder;
        private List<CategoryIconHolder> categoryIconHolders=new List<CategoryIconHolder>();
        private TransactionHolder selectedTransaction;
        private RectTransform rect;

        private void Awake()
        {
            TransactionHolder.OnSelectCategory += TransactionHolder_selectCategory;
            CategoryIconHolder.categorySelected += CategoryIconHolder_categorySelected;
            TryGetComponent(out rect);
        }

     

        private void OnDestroy()
        {
            TransactionHolder.OnSelectCategory -= TransactionHolder_selectCategory;
            CategoryIconHolder.categorySelected -= CategoryIconHolder_categorySelected;

        }
        private void TransactionHolder_selectCategory(TransactionHolder obj)
        {
            SpriteResourceLibrary category = AppResources.GetSpriteGroup(obj.myTransaction.Type == TransactionType.debit ? R_Drawables.DebitCategories : R_Drawables.CreditCategories);
            foreach (var item in categoryIconHolders)
            {
                item.gameObject.SetActive(false);
            }
            for (int i = 0; i < category.spriteResources.Count; i++)
            {
                categoryIconHolders[i].gameObject.SetActive(true);
                categoryIconHolders[i].InitCategory(category.spriteResources[i]);
            }
            selectedTransaction = obj;
            transform.SetParent(obj.transform, false);
            rect.anchoredPosition = Vector2.zero;
            GetSelectedIcon(selectedTransaction.GetCategory()).transform.SetAsFirstSibling();
            DOTween.To(() => categorySelectorHolder.spacing, x => categorySelectorHolder.spacing = x, 100, 0.5f).From(-100);
            holder.gameObject.SetActive(true);
            holder.DOFade(1, 0.2f).From(0);
        }
        private void CategoryIconHolder_categorySelected(string obj)
        {
            if (selectedTransaction == null)
            {
                return;
            }
            var spriteResource = AppResources.GetSpriteGroup(selectedTransaction.myTransaction.Type == TransactionType.debit ? R_Drawables.DebitCategories : R_Drawables.CreditCategories).GetSpriteResource(obj);
            selectedTransaction.UpdateCategory(spriteResource);
            DOTween.To(() => categorySelectorHolder.spacing, x => categorySelectorHolder.spacing = x, -100, 0.5f).From(100);
            holder.DOFade(0, 0.3f).From(1).OnComplete(()=> {
                holder.gameObject.SetActive(false);
            });
            HelperFunctions.DelayInvoke(this, () => { AppManager.instance.SaveData(); }, 1.5f);
        }
        CategoryIconHolder GetSelectedIcon(string categoryName)
        {
            //print(categoryName);
            CategoryIconHolder categoryIcon= categoryIconHolders.Where(x => x.GetName()==categoryName).FirstOrDefault();
            return categoryIcon;
          
        }
        void Start()
        {
            SpriteResourceLibrary debitCategoryLibrary = AppResources.GetSpriteGroup(R_Drawables.DebitCategories);
            bool first = true;

            foreach (var item in debitCategoryLibrary.spriteResources)
            {
                if (first)
                {
                    first = false;
                    categoryIconHolder.InitCategory(item);
                    categoryIconHolders.Add(categoryIconHolder);
                }
                else
                {
                    CategoryIconHolder categoryIcon = Instantiate(categoryIconHolder, categoriesHolder);
                    categoryIcon.InitCategory(item);
                    categoryIconHolders.Add(categoryIcon);
                }
            }
        }

    } 
}
