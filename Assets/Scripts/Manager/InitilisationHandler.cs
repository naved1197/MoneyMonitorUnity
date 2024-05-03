using MyBox;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CubeHole.MM
{
    public class InitilisationHandler : MonoBehaviour
    {
        [SerializeField] private HomeScreen HomeScreenManager;
        [SerializeField] private SettingsHandler settingsHandler;
        [SerializeField] private TransactionsFilterHandler filterHandler;
        [SerializeField] private TextMeshProUGUI messageProgressTxt;
        [SerializeField] private Slider messageProgressSlider;
        [SerializeField] private GameObject WelcomeScreen;
        [SerializeField] private GameObject HomeScreen;
        private bool _firstFetch;
        private AnimationFunctions splashAnimator;
        private SMSData appData;
        public int oldDataSize;
        private bool isRefreshingData=false;

        private void Awake()
        {
            HomeScreen.SetActive(false);
            WelcomeScreen.SetActive(false);
            AppManager.OnInitialised += Initialise;
            AndroidUtils.OnMessageFecthed += NewMessageFetched;
            AppManager.OnDataProcessStart += DataProcessStart;
            AppManager.OnProgressComplete += ProgressComplete;
        }
        private void OnDestroy()
        {
            AppManager.OnInitialised -= Initialise;
            AndroidUtils.OnMessageFecthed -= NewMessageFetched;
            AppManager.OnProgressComplete -= ProgressComplete;
            AppManager.OnDataProcessStart -= DataProcessStart;
        }
        private void Initialise(AnimationFunctions animatorSplash)
        {
            splashAnimator= animatorSplash;
            appData = AppManager.instance.GetSMSData();
            filterHandler.Init(appData);
            settingsHandler.Init(appData.Settings);
                isRefreshingData=false;
            if (appData.IsInitialised)
            {
                Debug.Log("Already Intialised");
                if (PlayerPrefs.HasKey("CanRefresh") && appData.Settings.ReadSMS)
                    CheckIfNewMessage();
                else
                    splashAnimator.MyAnimator.SetTrigger("exit");
            }
            else
            {
                
                Debug.Log("Not Intialised");
                AndroidUtils.CreateNoficationChannel("MMT", "Transactions", "Notification for transactions");
                splashAnimator.MyAnimator.SetTrigger("exit");
            }
            splashAnimator.OnAnimationComplete += () =>
            {
                if (!appData.IsInitialised)
                {
                    WelcomeScreen.SetActive(true);
                }
                else
                {
                    TriggerCloudEvent();
                    if(!appData.Settings.ReadSMS)
                    {
                        InitManualMode();
                        return;
                    }
                    if (isRefreshingData)
                        ScreenSwitcher.Instance.ChangeScreenState(UIScreens.MessageReading, true);
                    else
                        HomeScreen.SetActive(true);
                }
            };
        }
        void TriggerCloudEvent()
        {
            Logger.Log("Cloud event data size" + FbNotifications.CloudEvents.Count);
            if (FbNotifications.CloudEvents.Count > 0)
            {
                foreach (var item in FbNotifications.CloudEvents)
                {
                    if (item.key == "message")
                    {
                        PopUpData popUpData=JsonUtility.FromJson<PopUpData>(item.value);
                        if (popUpData == null)
                            return;
                        PopUp.ShowPopUp(popUpData);
                    }
                }
            }
            FbNotifications.CloudEvents.Clear();
           
        }
        public void GivePermission()
        {
            Logger.Log("Permission Requested");
            BackButtonHandler.ClearBackActions();
            PopUp.ShowPopUp("Please wait!", loadingType: true, canClose: false);
            AndroidUtils.SendToAndroid(AndroidFunctions.RequestPermission);
            BackButtonHandler.SetFocusCallback((focus) =>
            {
                if (focus)
                {
                    AndroidUtils.SendToAndroid(AndroidFunctions.CheckPermission);
                    AndroidUtils.SetPermissionCallBack((permissionState) =>
                    {
                        PopUp.Close();
                        if (permissionState)
                        {
                            appData.Settings.ReadSMS = true;
                            Logger.LogInfo("Permission Granted");
                            FetchMessages();
                        }
                        else
                        {
                            AndroidUtils.SendToAndroid(AndroidFunctions.Toast, "Permission declined");
                            Logger.Log("Permission declined");
                        }
                    });

                }
            });
        }
        public void FetchMessages()
        {
            Logger.Log("Fetching Messages");
            _firstFetch = true;
            BackButtonHandler.SetFocusCallback(null);
            AndroidUtils.SetPermissionCallBack(null);
            if (AndroidUtils.HavePermission)
                AndroidUtils.SendToAndroid(AndroidFunctions.FetchMessages, "Null");
            else
            {
                Logger.Log("Permission Not Granted");
                AndroidUtils.SendToAndroid(AndroidFunctions.Toast, "Permission Not Granted");
            }
        }
        private void NewMessageFetched(int newMessageCount, int totalMessageCount)
        {
            if (_firstFetch)
            {
                _firstFetch = false;
                if (!isRefreshingData)
                    ScreenSwitcher.Instance.SwitchScreen(UIScreens.Permission, UIScreens.MessageReading, TransitionType.Vertical, false);
                else
                    splashAnimator.MyAnimator.SetTrigger("exit");
                messageProgressSlider.maxValue = totalMessageCount;

                Logger.Log("Total Message Count " + totalMessageCount);
            }
            messageProgressTxt.text = newMessageCount + "/" + totalMessageCount;
            messageProgressSlider.value = newMessageCount;
        }
        private void DataProcessStart()
        {
            Logger.Log("Data Process Start");
            messageProgressTxt.text = "Please wait! generating your report";
        }
        private void ProgressComplete()
        {
            Logger.Log("Progress Complete");
            HomeScreenManager.Init(appData);
            HelperFunctions.DelayInvoke(this, () => {
                isRefreshingData=false;
                ScreenSwitcher.Instance.SwitchScreen(UIScreens.MessageReading, UIScreens.Home, TransitionType.Vertical, false);
            }, 2.5f);
        }
        public void CheckIfNewMessage()
        {
            Logger.LogInfo("Checking for new messages");
            PlayerPrefs.DeleteKey("CanRefresh");
            oldDataSize = appData.TotalMessageCount;
            AndroidUtils.SetTotalMessageCallBack(TotalMessageCallBack);
            AndroidUtils.SendToAndroid(AndroidFunctions.FetchMessages, "Null");
        }
        public void ManualMode()
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            appData.LatestSMS = new SMS();
            appData.LatestSMS.date = currentDate;
            appData.LastSyncDate = currentDate;
            appData.Settings.ReadSMS = false;
            appData.IsInitialised = true;
            appData.TotalMessageCount = 0;
            appData.Accounts = new List<Account>();
            appData.Accounts.Add(TransactionsManager.GetCashAccount());
            AppManager.instance.SaveData();
            InitManualMode();
        }
        void InitManualMode()
        {
           
            HomeScreenManager.Init(appData);
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Permission, UIScreens.Home, TransitionType.Vertical, false);

        }
        public void TotalMessageCallBack(int total)
        {
            Logger.Log("Current data size " + oldDataSize);
            Logger.Log("Recieved data size " + total);
            appData.TotalMessageCount = total;
            if (total > oldDataSize)
            {
                Logger.Log("New Message Found");
                isRefreshingData=true;
                AndroidUtils.SetTotalMessageCallBack((s) =>
                {
                    Logger.Log($"Got new messages {s}");
                    RefreshData();
                });
                AndroidUtils.SendToAndroid(AndroidFunctions.FetchMessages, appData.LatestSMS.date);
            }
            else
            {
                isRefreshingData=false;
                Logger.Log("No New Message Found");
                splashAnimator.MyAnimator.SetTrigger("exit");
                HomeScreenManager.Init(appData);
            }
        }
        public void RefreshData()
        {
            Logger.Log("Refereshing data");
            _firstFetch = true;
            AndroidUtils.SendToAndroid(AndroidFunctions.GetNextSMS);
        }
        public void OpenPrivacyPolicy()
        {
            string privacyPolicyLink = AppResources.GetStringsLibrary(R_Strings.WebLinks).GetStringResource(R_WebLinks.PrivacyPolicy.ToString());
            Application.OpenURL(privacyPolicyLink);
           // AndroidUtils.SendToAndroid(AndroidFunctions.Toast, "Open's Privacy Policy");
        }
    }
}
