using System;
using MyBox;
using System.Collections.Generic;
using UnityEngine;
using CubeHole.MM;

namespace CubeHole
{
    public enum AndroidFunctions { SetUnityEnvironment, InitPlugin, Toast, RequestPermission, FetchMessages, CheckPermission, ChangeUIColors, GetNextSMS, OpenSettings, OpenDatePicker, ScheduleNotfication, CreateNotificationChannel }
    public class AndroidUtils : MonoBehaviour
    {
        public static event Action<int, int> OnMessageFecthed;
        public static AndroidUtils instance;
        public static AndroidJavaObject unityActivity;
        static AndroidJavaObject _pluginInstance;
        private List<SMS> AllSMS = new List<SMS>();
        private Action<int> OnTotalMessageSet;
        private Action<bool> OnPermissionSet;
        private Action<DateTime> OnDateSet;
        public static bool HavePermission = false;
        int totalMessages;
        private SMSData SMSData;
        #region PluginInit
        public void Init(SMSData data)
        {
            if (instance == null)
            { 
                instance = this;
                InitialisePlugin("com.cubehole.sms_plugin.SMSPlugin");
                AllSMS.Clear();
                SMSData = data;
            }
            else
                Destroy(instance);
          
        }
        void InitialisePlugin(string pluginName)
        {
            if (Application.isEditor)
            {
                return;
            }
            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            _pluginInstance = new AndroidJavaObject(pluginName);
            if (_pluginInstance == null)
            {
                LogToServer("PluginInstanceError"); 
                Logger.Log("Plugin Instance Error");
            }
            else
            {

                Logger.Log("Plugin Init");
            }
            _pluginInstance.SetStatic<bool>("fromUnity", true);
            SendToAndroid(AndroidFunctions.InitPlugin, unityActivity);
        }
        #endregion

        #region ToAndroid

        public static void SendToAndroid(AndroidFunctions methodName, params object[] args)
        {
#if UNITY_EDITOR
            if(Application.isEditor)
            {

                EditorSimulations(methodName, args);
                return;
            }
#endif
            if (_pluginInstance != null)
            {
                Logger.Log("Calling Androd function" + methodName.ToString());
                _pluginInstance.CallStatic(methodName.ToString(), args);
            }
            else
            {
                LogToServer("PluginInstanceError");
                Logger.Log("Plugin Instance Error");
            }
        }   
#if UNITY_EDITOR
        public static void EditorSimulations(AndroidFunctions methodName, params object[] args)
        {
            SMSData data = AppManager.instance.SMSDirectoryEditor;
            switch (methodName)
            {
                case AndroidFunctions.SetUnityEnvironment:
                    Logger.Log("Setting Unity Environment");
                    break;
                case AndroidFunctions.InitPlugin:
                    Logger.Log("Plugin Initialised");
                    break;
                case AndroidFunctions.Toast:
                    Logger.Log(args.ToString());
                    break;
                case AndroidFunctions.RequestPermission:
                    Logger.Log("Permission Requested");
                    break;
                case AndroidFunctions.FetchMessages:
                    instance.SetTotalMessages(data.SMSDirectory.Count.ToString());
                    break;
                case AndroidFunctions.CheckPermission:
                    Logger.Log("Press Editor Buttons");
                    break;
                case AndroidFunctions.ChangeUIColors:
                    Logger.Log("Won't work in editor");
                    break;
                case AndroidFunctions.GetNextSMS:
                    foreach (var item in data.SMSDirectory)
                    {
                        instance.AppendMessage(JsonUtility.ToJson(item));
                    }
                   instance.ProcessComplete("");
                    break;
            }
        }
        [ButtonMethod]
        public void SetPermissionTrue()
        {
            SetPermissionState("true");
        }
        [ButtonMethod]
        public void SetPermissionFalse()
        {
            SetPermissionState("false");
        }
#endif
#endregion
        #region FromAndroid
        public void DateTimeSet(string date)
        {
            Logger.Log("Date Set " + date);
            DateTime dateTime = TransactionsManager.ConvertDate(date);
            OnDateSet?.Invoke(dateTime);
        }
        public void PermissionDenied(string result)
        {
            LogToServer("PermissionDenied");
            Logger.LogError("Permission Denied");
            PopUp.ShowPopUp("This permission is required for the app to work",HeadingTxt:"Read SMS", yesText: "OpenSettings", YesAction: () => { SendToAndroid(AndroidFunctions.OpenSettings); });
        }
        public void SetPermissionState(string result)
        {

            HavePermission = bool.Parse(result);
            OnPermissionSet?.Invoke(HavePermission);
            Logger.Log("Have read sms permission= " + result);
        }
        public void AppendMessage(string data)
        {
            SMS sms = new SMS();
            JsonUtility.FromJsonOverwrite(data, sms);
            AllSMS.Add(sms);
            OnMessageFecthed?.Invoke(AllSMS.Count, totalMessages);
        }
        public void ProcessComplete(string data)
        {
            Logger.Log($"Process Complete new messages {AllSMS.Count}");
            AppManager.instance.AddNewSMS(AllSMS);
        }
        public void ReceiveError(string error)
        {
            LogToServer("errorFromPlugin");
            Logger.Log("Error from plugin" +error);
        }
        public void SetTotalMessages(string total)
        {
            int.TryParse(total, out totalMessages);
            AllSMS.Clear();
            if (!SMSData.IsInitialised)
            {
                SendToAndroid(AndroidFunctions.GetNextSMS);
               SMSData.TotalMessageCount = totalMessages;
            }
            else
                OnTotalMessageSet?.Invoke(totalMessages);
            Logger.Log("Got Total Messages" + total);
        }
        #endregion
        public static void SetDatePickCallBack(Action<DateTime> action)
        {
            instance.OnDateSet = null;
            instance.OnDateSet = action;
        }
        public static void SetTotalMessageCallBack(Action<int> action)
        {
            instance.OnTotalMessageSet = null;
            instance.OnTotalMessageSet = action;
        }
        public static void SetPermissionCallBack(Action<bool> action)
        {
            instance.OnPermissionSet = null;
            instance.OnPermissionSet = action;
        }
        public static void CreateNoficationChannel(string channelID,string channelName,string channelDescription)
        {
            SendToAndroid(AndroidFunctions.CreateNotificationChannel, channelID, channelName, channelDescription);
        }
        public static void ScheduleNotification(int notificationID, String tile, String content, int delay)
        {
            SendToAndroid(AndroidFunctions.ScheduleNotfication, "MMT", notificationID, tile, content, delay);
        }
        static void LogToServer(string eventName)
        {
            FbAnalytics.LogEvent(eventName);
        }
    }
}
