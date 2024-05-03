using Firebase.Extensions;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CubeHole
{

    [System.Serializable]
    public class FBCloudEventData
    {
        public string key = "";
        public string value = "";
    }

    public class FbNotifications
    {
        public static List<FBCloudEventData> CloudEvents = new List<FBCloudEventData>();
        public static void Init(string topic)
        {
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.SubscribeAsync(topic).ContinueWithOnMainThread(task =>
            {
                LogTaskCompletion(task, "SubscribeAsync");
            });
            Log("Firebase Messaging Initialized");
        }
        public static void RequestPermission()
        {
            FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(
          task =>
          {
              LogTaskCompletion(task, "RequestPermissionAsync");
          }
        );
        }
        static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Log("Received a new message");
            var notification = e.Message.Notification;
            if (notification != null)
            {
                Log("title: " + notification.Title);
                Log("body: " + notification.Body);
                var android = notification.Android;
                if (android != null)
                {
                    Log("android channel_id: " + android.ChannelId);
                }
            }
            if (e.Message.From.Length > 0)
                Log("from: " + e.Message.From);
            if (e.Message.Link != null)
            {
                Log("link: " + e.Message.Link.ToString());
            }
            if (e.Message.Data.Count > 0)
            {
                Log("data:");
                foreach (var item in e.Message.Data)
                {
                    Log("  " + item.Key + ": " + item.Value);
                    CloudEvents.Add(new FBCloudEventData() { key = item.Key, value = item.Value });
                }
            }
        }
        static void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Log("Received Registration Token: " + token.Token);
        }

        public void ToggleTokenOnInit()
        {
            bool newValue = !FirebaseMessaging.TokenRegistrationOnInitEnabled;
            FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
            Log("Set TokenRegistrationOnInitEnabled to " + newValue);
        }

        static bool LogTaskCompletion(Task task, string operation)
        {
            bool complete = false;
            if (task.IsCanceled)
            {
                Log(operation + " canceled.");
            }
            else if (task.IsFaulted)
            {
                Log(operation + " encounted an error.");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string errorCode = "";
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        errorCode = String.Format("Error.{0}: ",
                          ((Error)firebaseEx.ErrorCode).ToString());
                    }
                    Log(errorCode + exception.ToString());
                }
            }
            else if (task.IsCompleted)
            {
                Log(operation + " completed");
                complete = true;
            }
            return complete;
        }
        static void Log(string message, int logType = 0)
        {
            string log = "FBNotification " + message;
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