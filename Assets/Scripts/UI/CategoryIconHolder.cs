using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole.MM {
    public class CategoryIconHolder : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Button categoryBtn;
        [SerializeField] private TextMeshProUGUI categoryText;
        [SerializeField] private bool isManual;
        public static event Action<string> categorySelected;
        public void InitCategory(SpriteResource category)
        {
            iconImage.sprite = category.sprite;
            iconImage.color = category.defaultColor;
            categoryText.text = category.name;
            if (isManual)
                return;
            categoryBtn.onClick.AddListener(() => {
                categorySelected?.Invoke(categoryText.text);
            });
        }
        public string GetName()
        {
            return categoryText.text;
        }

    }
}