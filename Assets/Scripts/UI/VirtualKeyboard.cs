using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Data;
using System;
namespace CubeHole
{

    public class VirtualKeyboard : MonoBehaviour
    {
        public static VirtualKeyboard instance;
        [SerializeField] private RectTransform Holder;
        [SerializeField] private VerticalLayoutGroup layoutGroup;
        private TMP_InputField inputField;
        private string inputString;
        private bool isActive = false;
        private Action submitAction;
        private void Awake()
        {
            //if instance is null, assign this to instance
            if (instance == null)
                instance = this;
            else
                Destroy(this);
            KeyHandler.OnKeyPressed += KeyHandler_OnKeyPressed;
        }
        private void KeyHandler_OnKeyPressed(string arg1, KeyType arg2)
        {
            //if the keyboard is not active, do not do anything
            if (!isActive)
                return;
            switch (arg2)
            {
                case KeyType.None:
                    AddKey(arg1);
                    break;
                case KeyType.Divide:
                    if (inputString.Length > 0 && char.IsNumber(inputString[^1]))
                    {
                        AddKey("/");
                    }
                    break;
                case KeyType.Multiply:
                    if (inputString.Length > 0 && char.IsNumber(inputString[^1]))
                    {
                        AddKey("*");
                    }
                    break;
                case KeyType.Equals:
                    Equals();
                    break;
                case KeyType.Minus:
                    if (inputString.Length > 0 && char.IsNumber(inputString[^1]))
                    {
                        AddKey("-");
                    }
                    break;
                case KeyType.Plus:
                    if (inputString.Length > 0 && char.IsNumber(inputString[^1]))
                    {
                        AddKey("+");
                    }
                    break;
                case KeyType.Backspace:
                    RemoveKey();
                    break;
                case KeyType.Submit:
                    Equals();
                    submitAction?.Invoke();
                    submitAction = null;
                    Hide();
                    break;
                case KeyType.Close:
                    Hide();
                    break;
            }
        }
        void Equals()
        {
            //separate the input string with '+', '-', '*', '/' and calculate the result according to BODMAS rule
            inputString = Calculate(inputString).ToString();
            inputField.text = inputString;
        }
        private void AddKey(string key)
        {
            inputString += key;
            inputField.text = inputString;
        }
        private void RemoveKey()
        {
            if (inputString.Length > 0)
            {
                inputString = inputString.Remove(inputString.Length - 1);
                inputField.text = inputString;
            }
        }
        private void Clear()
        {
            inputString = string.Empty;
            inputField.text = inputString;
        }
        void Show()
        {
            if (isActive)
                return;
            isActive = true;
            Holder.gameObject.SetActive(true);
            Holder.DOAnchorPosY(0, 0.5f).From(Vector2.down * Holder.rect.height);

        }
        void Hide()
        {
            if (!isActive)
                return;
            inputField = null;
            isActive = false;
            Holder.DOAnchorPosY(-Holder.rect.height, 0.5f).From(Vector2.zero).OnComplete(() =>
            {
                Holder.gameObject.SetActive(false);
            });
        }
        public static double Calculate(string expression)
        {
            // Removing any whitespace from the expression
            expression = expression.Replace(" ", "");

            // Using DataTable.Compute to evaluate the expression
            DataTable dataTable = new DataTable();
            DataColumn column = new DataColumn("Expression", typeof(double), expression);
            dataTable.Columns.Add(column);
            dataTable.Rows.Add(0);
            double result = (double)(dataTable.Rows[0]["Expression"]);

            return result;
        }
        //function to assign sumbit action
        public static void SetSubmitAction(Action action)
        {
            instance.submitAction = action;
        }
        public static void ShowKeyboard(TMP_InputField inputField)
        {
            instance.inputField = inputField;
            instance.Show();
        }
        public static void HideKeyboard()
        {
            instance.Hide();
        }
        private void OnDestroy()
        {
            KeyHandler.OnKeyPressed -= KeyHandler_OnKeyPressed;
        }

    }
}