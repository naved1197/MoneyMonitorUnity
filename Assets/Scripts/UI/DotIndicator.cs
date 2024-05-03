using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class DotIndicator : MonoBehaviour
{
    [SerializeField] private GameObject dot;
    [SerializeField] private Transform dotHolder;
    [SerializeField] private Transform contentHolder;
    [SerializeField] private float dotRadius=50;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color normalColor;
    [SerializeField] private List<GameObject> dots=new List<GameObject>();
    [SerializeField] private bool Update = false;
    [SerializeField] private int numberOfPages=3;
    private int _selectedPage = 0;
    private void OnValidate()
    {
        Init();
    }
    private void Awake()
    {
        ChangePage(0);
    }
    void Init()
    {
        if (!Update&&!gameObject.activeInHierarchy)
            return;
        numberOfPages = contentHolder.childCount;
        if (dots.Count != numberOfPages&&dotHolder!=null&&dot!=null && contentHolder != null)
        {
            for (int i = 1; i < dotHolder.childCount; i++)
            {
                Destroy(dotHolder.GetChild(i));
            }
            dots.Clear();
            this.dots.Add(dot);
            for (int i = 1; i < numberOfPages; i++)
            {
                GameObject dot = Instantiate(this.dot, dotHolder);
                this.dots.Add(dot);
            }
        }
        ChangePage(0);
    
    }
    public void ChangePage(int page)
    {
        _selectedPage = page;
        for (int i = 0; i < dots.Count; i++)
        {
            UISquircle dotImage = dots[i].GetComponent<UISquircle>();
            RectTransform rect = dotImage.GetComponent<RectTransform>();
            rect.sizeDelta = Vector2.one * dotRadius;
            if(i== _selectedPage)
            {
                dotImage.color = selectedColor;
                rect.DOSizeDelta(new Vector2(dotRadius * 3, dotRadius), 0.5f);
            }
            else if(dotImage.color==selectedColor)
            {
                dotImage.color = normalColor;
                rect.DOSizeDelta(Vector2.one * dotRadius, 0.5f);

            }
        }
    }
}
