
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CubeHole.MM
{

    public class SettingsHandler : MonoBehaviour

    {
        [SerializeField] private Toggle readSMSToggle;
        [SerializeField] private Toggle notificationToggle;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle vibrationToggle;

        [SerializeField] private Button clearDataBtn;
        [SerializeField] private Button creditsBtn;
        [SerializeField] private Button saveSettingsBtn;
        [SerializeField] private Button closeCreditsBtn;
        [SerializeField] private Button privacyPolicyBtn;
        
        [SerializeField] private Button navedFollow;
        [SerializeField] private Button freepikVisit;
        [SerializeField] private Button flaticonVisit;
        [SerializeField] private Image creditsHolder;
        private PlayerSettingsInfo settingsInfo;
        RectTransform creditsHolderRect;
        public void Init(PlayerSettingsInfo settings)
        {
            creditsHolderRect = creditsHolder.transform.GetChild(0).GetComponent<RectTransform>();
            settingsInfo = settings;
            AssignToggleListners(readSMSToggle, ToggleReadSMS);
            AssignToggleListners(notificationToggle, ToggleNotification);
            AssignToggleListners(soundToggle, ToggleSound);
            AssignToggleListners(vibrationToggle, ToggleVibration);

            readSMSToggle.isOn = settings.ReadSMS;
            notificationToggle.isOn = settings.Notification;
            soundToggle.isOn = settings.Sound;
            vibrationToggle.isOn = settings.Vibration;

            var webLinks = AppResources.GetStringsLibrary(R_Strings.WebLinks);
            navedFollow.onClick.AddListener(() => { OpenLink(webLinks.GetStringResource(R_WebLinks.NavedInsta.ToString())); });
            freepikVisit.onClick.AddListener(() => { OpenLink(webLinks.GetStringResource(R_WebLinks.Freepik.ToString())); });
            flaticonVisit.onClick.AddListener(() => { OpenLink(webLinks.GetStringResource(R_WebLinks.Flaticon.ToString())); });
            creditsBtn.onClick.AddListener(OpenCredits);
            closeCreditsBtn.onClick.AddListener(CloseCredits);
            clearDataBtn.onClick.AddListener(ClearData);
            saveSettingsBtn.onClick.AddListener(SaveSettings);
            privacyPolicyBtn.onClick.AddListener(() => { OpenLink(webLinks.GetStringResource(R_WebLinks.PrivacyPolicy.ToString())); });

           
        }
        void AssignToggleListners(Toggle toggle,UnityAction<bool> action)
        {
            ToggleButton toggleButton = toggle.GetComponent<ToggleButton>();
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(toggleButton.ToggleGroup);
            toggle.onValueChanged.AddListener(action);
        }
        public void ToggleReadSMS(bool result)
        {
            settingsInfo.ReadSMS = result;
        }
        public void ToggleNotification(bool result)
        {
            settingsInfo.Notification = result;
        }
        public void ToggleSound(bool result)
        {
            settingsInfo.Sound = result;
            AudioManager.isMute = !result;
            print("Is Music Mute: " + AudioManager.isMute);
        }
        public void ToggleVibration(bool result)
        {
            settingsInfo.Vibration = result;
            Vibration.canVibrate = result;
        }
        public void SaveSettings()
        {
            ScreenSwitcher.Instance.SwitchScreen(UIScreens.Settings,UIScreens.Home,TransitionType.Vertical);
            AppManager.instance.SaveData();
        }

        public void ClearData()
        {
            PopUp.ShowPopUp(message: "Are you sure you want to clear all data?", yesText: "Yes", NoText: "No", YesAction: () =>
            {
                SMSData sMSData = AppManager.instance.GetSMSData();
                sMSData = new SMSData();
                SaveSettings();
            }, HeadingTxt: "Caution");
        }

        public void OpenCredits()
        {
            creditsHolder.gameObject.SetActive(true);
            creditsHolder.DOFade(0.7f, 0.4f).From(0);
            creditsHolderRect.DOAnchorPosY(0, 0.4f).From(Vector2.down * creditsHolderRect.rect.height);
        }
        public void CloseCredits()
        {
            creditsHolder.DOFade(0, 0.4f).From(0.7f).OnComplete(()=> { creditsHolder.gameObject.SetActive(false); });
            creditsHolderRect.DOAnchorPosY(-creditsHolderRect.rect.height, 0.4f).From(Vector2.zero);
        }

        public void OpenLink(string link)
        {
            Application.OpenURL(link);
        }
    }

}