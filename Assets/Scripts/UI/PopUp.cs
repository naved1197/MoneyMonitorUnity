using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
namespace CubeHole.MM
{
    public class PopUpData
    {
        public string message;
        public string heading;
        public string yesText;
        public string noText;
        public bool canClose;
        public string redirectURL;
    }
    public class PopUp : MonoBehaviour
    {
        [SerializeField] private Image Tint;
        [SerializeField] private RectTransform Holder;
        [SerializeField] private Button CloseBtn;
        [SerializeField] private Image InfoImage;
        [SerializeField] private Image LoadingImage;
        [SerializeField] private TextMeshProUGUI InfoTxt;
        [SerializeField] private TextMeshProUGUI YesTxt, NoTxt;
        [SerializeField] private TextMeshProUGUI HeadingText;
        [SerializeField] private Button YesBtn, NoBtn;
        [SerializeField] private Color tintColor;
        public static PopUp i;
        private Vector2 startPos;
        private void Awake()
        {
            if (i == null)
                i = this;
            else
                Destroy(i);

            startPos = Vector2.up * (CloseBtn.GetComponent<RectTransform>().rect.height + 100);
        }
        public static void ShowPopUp(PopUpData data)
        {
            i.ShowPopUpNow(message: data.message, YesAction: () =>
            {
                if (data.redirectURL != null)
                    Application.OpenURL(data.redirectURL);
            }, yesText: data.yesText, NoAction: null, NoText: data.noText, canClose: data.canClose, HeadingTxt: data.heading);
        }
        public static void ShowPopUp(string message, Sprite infoSprite = null, Action YesAction = null,
        string yesText = null, Action NoAction = null, string NoText = null, bool canClose = true,
        string HeadingTxt = null,bool loadingType=false,bool registerBack=false)
        {
            if(i!=null)
            {
                LogToServer("CallingShowPopUpBeforeInit");
                return;
            }    
            i.ShowPopUpNow(message: message, infoSprite: infoSprite, YesAction: YesAction, yesText: yesText, NoAction: NoAction, NoText: NoText, canClose: canClose, HeadingTxt: HeadingTxt, loadingType:loadingType,registerBack:registerBack);
        }
        
        void ShowPopUpNow(string message, Sprite infoSprite = null, Action YesAction = null, string yesText = null, Action NoAction = null, string NoText = null, bool canClose = true, string HeadingTxt = null, bool loadingType = false, bool registerBack = false)
        {
            Color color = tintColor;
            Tint.gameObject.SetActive(true);
            Tint.material.DOColor(tintColor, 0.5f).From(Color.white);
            Tint.material.DOFloat(3, "_Size", 0.5f).From(0);
            Holder.DOAnchorPosY(0, 0.5f).From(startPos).SetEase(Ease.InBounce);
            LoadingImage.gameObject.SetActive(loadingType);
            InfoImage.gameObject.SetActive(infoSprite != null && !loadingType);
            CloseBtn.interactable = canClose;
           if (HeadingTxt != null&&!string.IsNullOrEmpty(HeadingTxt))
            {
                HeadingText.transform.parent.gameObject.SetActive(true);
                HeadingText.text = HeadingTxt;
            }
            else
            {
                HeadingText.transform.parent.gameObject.SetActive(false);
            }
            if (!loadingType)
            {
                if (infoSprite == null)
                {
                    InfoImage.transform.parent.gameObject.SetActive(false);
                    InfoImage.gameObject.SetActive(false);
                }
                else
                {
                    InfoImage.transform.parent.gameObject.SetActive(true);
                    InfoImage.gameObject.SetActive(true);
                    InfoImage.sprite = infoSprite;
                }
            }
            else
            {
                InfoImage.transform.parent.gameObject.SetActive(true);
                LoadingImage.fillClockwise = true;
                LoadingImage.DOFillAmount(1, 1f).From(0).SetLoops(-1, LoopType.Yoyo).OnStepComplete(() =>
                {
                    LoadingImage.fillClockwise = !LoadingImage.fillClockwise;
                }).SetId("loading");
            }
            InfoTxt.text = message;
            YesBtn.onClick.RemoveAllListeners();
            NoBtn.onClick.RemoveAllListeners();
            NoBtn.onClick.AddListener(() => { ClosePop(); });
            YesBtn.onClick.AddListener(() => { ClosePop(); });
            if (yesText != null && !string.IsNullOrEmpty(yesText))
            {
                YesBtn.gameObject.SetActive(true);
                YesBtn.onClick.AddListener(() => { YesAction?.Invoke(); });
                YesTxt.text = yesText;
            }
            else
            {
                YesBtn.gameObject.SetActive(false);

            }
            if (NoText != null && !string.IsNullOrEmpty(NoText))
            {
                NoBtn.onClick.AddListener(() => { NoAction?.Invoke(); });
                NoBtn.gameObject.SetActive(true);
                NoTxt.text = NoText;
            }
            else
            {
                NoBtn.gameObject.SetActive(false);
            }
            if(yesText == null&& NoText == null)
            {
                YesBtn.transform.parent.gameObject.SetActive(false);
            }
            else
            {

                YesBtn.transform.parent.gameObject.SetActive(true);
            }
        }
        public static void Close()
        {
            if(i!=null)
            {
                LogToServer("CallingPopUpBeforeInit");
                return;
            }
            i.ClosePop();
        }
        public void ClosePop()
        {
            if (!Tint.gameObject.activeInHierarchy)
                return;
            Tint.material.DOColor(Color.white, 0.5f).From(tintColor);
            Tint.material.DOFloat(0, "_Size", 0.5f).From(3);
            DOTween.Kill("loading");
            Holder.DOAnchorPosY(startPos.y, 0.5f).From(Vector2.zero).SetEase(Ease.OutSine).OnComplete(()=> {
                Tint.gameObject.SetActive(false);
            });
        }
        static void LogToServer(string eventName)
        {
            FbAnalytics.LogEvent(eventName);
        }
    }
}   
