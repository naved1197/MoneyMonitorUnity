using Firebase.Analytics;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CubeHole
{
    public class FbAnalytics 
    {
        public static bool isInited = false;
        public static void Init(bool logEnabled = true, Dictionary<string, string> userProperties = null, string userID = "", int DurationInMinutes = -1)
        {
            isInited= true;
            Log($"Firebase Log enabled= {logEnabled}");
            if (!logEnabled)
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(logEnabled);


            if (userProperties != null)
            {
                foreach (var item in userProperties)
                {
                    Log($"Setting User Property {item.Key}={item.Value}");
                    FirebaseAnalytics.SetUserProperty(item.Key, item.Value);
                }
            }
            if (userID != "")
            {
                Log($"Setting User ID {userID}");
                FirebaseAnalytics.SetUserId(userID);
            }
            if (DurationInMinutes > 0)
            {
                Log($"Setting DurationInMinutes {DurationInMinutes}");
                FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, DurationInMinutes, 0));
            }
        }
        public static void LogEvent(string eventName)
        {
            if (!isInited)
            {
                Log("Firebase not inited");
                return;
            }
            Log("Logging firebase event " + eventName);
            FirebaseAnalytics.LogEvent(eventName);
        }
        public static void LogEvent(string eventName, string parameterName, string parameterValue)
        {
            if (!isInited)
            {
                Log("Firebase not inited");
                return;
            }
            Log("Logging firebase event " + eventName + " with parameter " + parameterName + " = " + parameterValue);
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }
        public static void LogEvent(string eventName, string parameterName, int parameterValue)
        {
            if (!isInited)
            {
                Log("Firebase not inited");
                return;
            }
            Log("Logging firebase event " + eventName + " with parameter " + parameterName + " = " + parameterValue);
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }
        public static void LogEvent(string eventName, string parameterName, double parameterValue)
        {
            if (!isInited)
            {
                Log("Firebase not inited");
                return;
            }
            Log("Logging firebase event " + eventName + " with parameter " + parameterName + " = " + parameterValue);
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }
        public static void LogEvent(string eventName, Parameter[] parameters)
        {
            if (!isInited)
            {
                Log("Firebase not inited");
                return;
            }
            Log("Logging firebase event " + eventName + " with parameters");
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }
        public static void ResetAnalyticsData()
        {
            if (!isInited)
            {
                Log("Firebase not inited");
                return;
            }
            Log("Reset analytics data.");
            FirebaseAnalytics.ResetAnalyticsData();
        }
        public Task<string> DisplayAnalyticsInstanceId()
        {
            return FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(task => {
                if (task.IsCanceled)
                {
                    Log("App instance ID fetch was canceled.");
                }
                else if (task.IsFaulted)
                {
                    Log(string.Format("Encounted an error fetching app instance ID {0}",
                                            task.Exception.ToString()));
                }
                else if (task.IsCompleted)
                {
                    Log(string.Format("App instance ID: {0}", task.Result));
                }
                return task;
            }).Unwrap();
        }
        public static void Log(string message, int logType = 0)
        {
            string log = message;
            switch (logType)
            {
                case 0:
                    Debug.Log(log);
                    break;
                case 1:
                    Debug.LogWarning(log);
                    break;
                case 2:
                    Debug.LogError(log);
                    break;
                default:
                    Debug.Log(log);
                    break;
            }
        }
    }
}