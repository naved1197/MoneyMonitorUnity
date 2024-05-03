using UnityEngine;
using TMPro;
namespace CubeHole
{
    [RequireComponent(typeof(TMP_InputField))]
    public class KeyBoardTrigger : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        //function to show the keyboard

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }
        public void ShowKeyboard()
        {
            VirtualKeyboard.ShowKeyboard(inputField);
        }
        //function to hide the keyboard
        public void HideKeyboard()
        {
            VirtualKeyboard.HideKeyboard();
        }
    }
}
