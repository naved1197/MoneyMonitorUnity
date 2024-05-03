using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeHole {
    public class BackButtonHandler : MonoBehaviour
    {
        private Stack<Action> backActionList = new Stack<Action>();
        private Action currentBackAction;
        private static BackButtonHandler instance;
        private event Action<bool> GetFocusStatus;
        private bool canInteract;
        private bool isLastAction;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                canInteract = true;
            }
            else
                Destroy(this);
        }
        public static void SetBackAction(Action action)
        {
            Logger.LogInfo("Adding new back action");
            instance.isLastAction = false;
            instance.backActionList.Push(action);
        }
        private void Update()
        {
            if (canInteract&&Input.GetKeyDown(KeyCode.Escape) )
            {
                ExecuteBackAction();
            }
        }
        public static void ClearBackActions()
        {
            instance.backActionList.Clear();
        }
        public void ExecuteBackAction()
        {

            if (backActionList.TryPop(out currentBackAction))
            {
                currentBackAction?.Invoke();
                currentBackAction = null;
            }
            else
            {
                if (isLastAction)
                {
                    Debug.Log("Exiting the game");
                    Application.Quit();
                }
                else
                {
                    AndroidUtils.SendToAndroid(AndroidFunctions.Toast, "Press back again to exit");
                    isLastAction = true;
                }
            }
            canInteract = false;
            Debug.Log("Back Button Action list length= " + backActionList.Count);
            HelperFunctions.DelayInvoke(this, () => { canInteract = true; }, 1f);
        }
        public static void SetFocusCallback(Action<bool> focusAction)
        {
           instance.GetFocusStatus = null;
            instance.GetFocusStatus = focusAction;
        }
        private void OnApplicationFocus(bool focus)
        {
            GetFocusStatus?.Invoke(focus);
        }
    }
}
