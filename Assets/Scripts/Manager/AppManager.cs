using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeHole.MM
{
    public class AppManager : MonoBehaviour
    {
        public static AppManager instance;
        public static event Action OnDataProcessStart;
        public static event Action OnProgressComplete;
        public static event Action<AnimationFunctions> OnInitialised;
        [SerializeField] private AnimationFunctions splashAnim;
        [SerializeField] private AppResources resources;
        [SerializeField] private AndroidUtils androidUtils;
        [SerializeField] private AudioManager audioManager;
        private SMSData SMSDirectory;



        private string savePath;

        #region Initialisation
        private void Awake()
        {
            if (instance == null)
            {
                Screen.fullScreen = false;
                Application.targetFrameRate = 60;

                instance = this;
                resources.Init();
                AudioResourceLibrary audioResourceLibrary = AppResources.GetAudioResourceLibrary(R_Audio.UI);
                List<AudioClip> audioClips = new List<AudioClip>();
                foreach (var item in audioResourceLibrary.audioResources)
                {
                    audioClips.Add(item.clip);
                }
                audioManager.InitAudio(audioClips.ToArray());
                splashAnim.gameObject.SetActive(true);
                PlayerPrefs.SetInt("CanRefresh", 1);
                savePath = AppResources.GetStringsLibrary(R_Strings.System).GetStringResource(R_System.SavePath.ToString());
                LoadData();
            }
            else
                Destroy(instance);
        }
        #endregion
        public SMSData GetSMSData()
        {
            return SMSDirectory;
        }
        #region SAVE/LOAD
        public void LoadRemoteConfig()
        {
            resources.LoadStringsFromRemoteConfig();
        }
        public void LoadData()
        {
            SaveSystem.LoadSMS(savePath, "sms_data.mm", (data) =>
            {
                Logger.LogInfo("Data Loaded");
                SMSDirectory = data;
                Vibration.canVibrate= SMSDirectory.Settings.Vibration;
                AudioManager.isMute = !SMSDirectory.Settings.Sound;
                StartCoroutine(ConvertDates());
            },
            () =>
            {
                SMSDirectory = (SMSData)ScriptableObject.CreateInstance(nameof(SMSData));
                SMSDirectory.IsInitialised  = false;
                Logger.LogInfo("Creating new data file");
                resources.SaveStringResources();
                Init();
            });
        }
        public void SaveData()
        {
            SaveSystem.SaveData(savePath, "sms_data.mm", SMSDirectory);
        }
        #endregion
        IEnumerator ConvertDates()
        {
            Logger.LogInfo("Converting Dates");
            foreach (var item in SMSDirectory.Transactions)
            {
                item.TransactionDate = TransactionsManager.ConvertDate(item.TransactionDateString);
            }
            yield return null;
            Init();
        }
        void Init()
        {
            Logger.LogInfo("Init function called from app manager");
            androidUtils.Init(SMSDirectory);
            OnInitialised?.Invoke(splashAnim);
        }
        #region Data Manipulation
        public void AddNewSMS(List<SMS> newSms = null)
        {
            if (newSms == null || newSms.Count == 0)
            {
                LogToServer("AddNewSMSFailed");
                PopUp.ShowPopUp("Something went wrong!");
                return;
            }
            OnDataProcessStart?.Invoke();
            SMSDirectory.LatestSMS = newSms[0];
            Logger.Log($"New Sms size {newSms.Count}");
            StartCoroutine(AddNewSMSCoroutine(newSms));
        }
        IEnumerator AddNewSMSCoroutine(List<SMS> newSms)
        {
            TransactionsManager.Init(SMSDirectory, newSms, () =>
            {
                Logger.LogInfo("TransactionsManager Process Complete");
                SaveData();
                OnProgressComplete?.Invoke();
            });
            yield return null;
        }
        public void DeleteTransaction(Transaction deletedTransaction)
        {
            LogToServer("TransactionDeleted");
            Logger.LogInfo($"Deleted Transaction with id {deletedTransaction.TransactionId}");
            SMSDirectory.Transactions.Remove(deletedTransaction);
            SaveData();
        }
        public void AddNewTransaction(Transaction newTransaction)
        {
            LogToServer("TransactionAdded");
            Logger.LogInfo($"Added new Transaction with id {newTransaction.TransactionId}");
            SMSDirectory.Transactions.Insert(0,newTransaction);
            SaveData();
        }
        #endregion

        public void LogToServer(string eventName)
        {
            FbAnalytics.LogEvent(eventName);
        }
#if UNITY_EDITOR
        public SMSData SMSDirectoryEditor;

        [ButtonMethod]
        public void GenerateFakeSMS()
        {
            int index = SMSDirectoryEditor.SMSDirectory.Count;
            for (int i = 0; i < 10; i++)
            {
                SMS sMS = new SMS();
                sMS.id = index.ToString();
                index++;
                sMS.address = "CP-BOIIND";
                sMS.date = "2023-06-15 19:32:16";
                sMS.body = "BOI UPI -VPA navedkh1197@okhdfcbank linked to Bank of India a/c XX6385 debited Rs." + UnityEngine.Random.Range(20, 2000) + " and credited to q810891053@ybl -Ref 316814837552";
                SMSDirectoryEditor.SMSDirectory.Add(sMS);
            }
        }
#endif
    }
}