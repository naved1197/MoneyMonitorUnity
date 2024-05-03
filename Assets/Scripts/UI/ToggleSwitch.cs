using System;
using TMPro;
using UnityEngine;

public class ToggleSwitch : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelTxt;
    [SerializeField] private GameObject outline;
    private bool isOn=false;
    public static event Action<string,bool> OnToggleStateChanged;
    public int id;
    public string label;
   public void Init(int id,string label,bool state)
    {
        this.id = id;
        this.label = label;
        labelTxt.text = label;
        isOn = state;
        outline.SetActive(isOn);
    }
    public void ChangeState()
    {
        isOn= !isOn;
        outline.SetActive(isOn);
        OnToggleStateChanged?.Invoke(labelTxt.text,isOn);
    }
    public void ResetSwitch()
    {
        isOn = false;
        outline.SetActive(isOn);
    }
}
