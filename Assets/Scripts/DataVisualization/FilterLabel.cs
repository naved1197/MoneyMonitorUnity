using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FilterLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI filterLabelTxt;
    [SerializeField] private Button removeFilterBtn;
    public static event Action<string> OnFilterRemoved;
    public string label;
    public void Init(string label)
    {
        this.label= label;
        filterLabelTxt.text = label;
        removeFilterBtn.onClick.RemoveAllListeners();
        removeFilterBtn.onClick.AddListener(() =>
        {
            OnFilterRemoved?.Invoke(label);
            gameObject.SetActive(false);
        });
    }
}
