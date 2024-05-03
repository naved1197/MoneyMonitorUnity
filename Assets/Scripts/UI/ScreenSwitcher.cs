using DG.Tweening;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

namespace CubeHole.MM {
    public enum TransitionType { Vertical,Horizontal}
    public enum UIScreens { Home,Settings,TransactionViewer,AllTransactions,MessageReading,Budget,Permission,Welcome}

    [System.Serializable]
    public class ScreenUIHolder
    {
        public UIScreens screen;
        public RectTransform rectTransform;
    }
    public class ScreenSwitcher : MonoBehaviour
    {
        public static ScreenSwitcher Instance;
        [SerializeField] private ScreenUIHolder[] screens;
        [SerializeField] private AnimationCurve outCurve;
        [SerializeField] private AnimationCurve inCurve;
        [SerializeField] private float speed=1f;
        private Dictionary<UIScreens, RectTransform> screenDictionary = new Dictionary<UIScreens, RectTransform>();
        private Sequence swapSequence;
        private event Action OnComplete;
        private RectTransform fromScreen;
        private RectTransform toScreen;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                StartCoroutine(InitScreens());
            }
            else
                Destroy(this.gameObject);
        }
        IEnumerator InitScreens()
        {
            foreach (var item in screens)
            {
                screenDictionary.Add(item.screen, item.rectTransform);
            }
            yield return null;
        }
        public void SwitchScreen(UIScreens from, UIScreens to, TransitionType type, bool isReversible = false, Action OnComplete = null)
        {
            this.OnComplete = null;
            this.OnComplete = OnComplete;
             fromScreen = screenDictionary[from];
             toScreen = screenDictionary[to];
            ChangeScreen(fromScreen, toScreen, type, isReversible);
        }
        public void SwitchScreen(UIScreens to, TransitionType type, bool isReversible = false, Action OnComplete = null)
        {
            this.OnComplete = null;
            this.OnComplete = OnComplete;
            toScreen = screenDictionary[to];
            foreach (var item in screenDictionary)
            {
                if (item.Value.gameObject.activeSelf)
                    fromScreen = item.Value;
            } 
            ChangeScreen(fromScreen, toScreen, type, isReversible);
        }
        void ChangeScreen(RectTransform fromScreen,RectTransform toScreen,TransitionType type,bool isReversible)
        {
            swapSequence = DOTween.Sequence();
            switch (type)
            {
                case TransitionType.Vertical:
                    fromScreen.gameObject.SetActive(true);
                    swapSequence.Append(fromScreen.DOAnchorPosY(fromScreen.rect.height + 100, speed).From(fromScreen.anchoredPosition).OnComplete(() =>
                    {
                        fromScreen.gameObject.SetActive(false);
                    }).SetEase(outCurve));
                    toScreen.gameObject.SetActive(true);
                    swapSequence.Join(toScreen.DOAnchorPosY(0, speed).From(-Vector2.up * (toScreen.rect.height + 100)).SetEase(inCurve));
                    break;
                case TransitionType.Horizontal:
                    fromScreen.gameObject.SetActive(true);
                    swapSequence.Append(fromScreen.DOAnchorPosX(-(fromScreen.rect.width + 100), speed).From(fromScreen.anchoredPosition).OnComplete(() =>
                    {
                        fromScreen.gameObject.SetActive(false);
                    }).SetEase(outCurve));
                    toScreen.gameObject.SetActive(true);
                    swapSequence.Join(toScreen.DOAnchorPosX(0, speed).From(Vector2.right * (toScreen.rect.width + 100)).SetEase(inCurve));
                    break;
            }
            swapSequence.Play().SetAutoKill(false);
            swapSequence.OnComplete(() =>
            {
                OnComplete?.Invoke();
            });
            if (!isReversible)
                return;
            BackButtonHandler.SetBackAction(() => {
                RevertScreen();
            });
        }
        public void ChangeScreenState(UIScreens name,bool state)
        {
            screenDictionary[name].gameObject.SetActive(state);
        }
        public void RevertScreen()
        {
            fromScreen.gameObject.SetActive(true);
            swapSequence.SetAutoKill(true).PlayBackwards();
            HelperFunctions.DelayInvoke(this, () =>
            {
                toScreen.gameObject.SetActive(false);
            }, speed);
        }
    }
}
