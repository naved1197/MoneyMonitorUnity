using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public enum KeyType { None,Divide,Multiply,Equals,Minus,Plus,Backspace,Submit,Close }
[RequireComponent(typeof(Button))]
public class KeyHandler : MonoBehaviour
{
    public KeyType keyType = KeyType.None;
    private Button key;
    private string keyCharacter;
    public static event Action<string, KeyType> OnKeyPressed;

    private void Awake()
    {
        key = GetComponent<Button>();
        if (keyType == KeyType.None&&key.transform.GetChild(0).TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI textComp))
        {
            keyCharacter = textComp.text;
        }
        key.onClick.AddListener(() =>
        {
            OnKeyPressed?.Invoke(keyCharacter, keyType);
        });
    }

}
