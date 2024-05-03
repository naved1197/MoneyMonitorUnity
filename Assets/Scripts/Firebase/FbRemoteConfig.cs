using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.RemoteConfig;
namespace CubeHole
{
    public class FbRemoteConfig
    {
        public static bool isInitialized;
        public static event Action<bool> OnRemoteConfigInitialized;
        private static TimeSpan cacheExpiryTime;
        public static void Init(Dictionary<string, string> stringValue, 
        Dictionary<string, int> intValues, 
        Dictionary<string, float> floatValues,
        Dictionary<string, bool> boolValues, TimeSpan remoteCacheExpiryTime)
        {
            cacheExpiryTime = remoteCacheExpiryTime;
            Dictionary<string, object> defaults = new Dictionary<string, object>();
            foreach (var item in stringValue)
            {
                defaults.Add(item.Key, item.Value);
            }
            foreach (var item in intValues)
            {
                defaults.Add(item.Key, item.Value);
            }
            foreach (var item in floatValues)
            {
                defaults.Add(item.Key, item.Value);
            }
            foreach (var item in boolValues)
            {
                defaults.Add(item.Key, item.Value);
            }
            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
          .ContinueWithOnMainThread(task =>
          {
              Log("RemoteConfig configured and ready!");
              FetchDataAsync();
          });

        }
        public static Task FetchDataAsync()
        {
            Log("Fetching data...");
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                cacheExpiryTime);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        static void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                Log("Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        isInitialized = true;
                        OnRemoteConfigInitialized?.Invoke(true);
                        Log(string.Format("Remote data loaded and ready (last fetch time {0}).",
                                       info.FetchTime));
                    });

                    break;
                case LastFetchStatus.Failure:
                    OnRemoteConfigInitialized?.Invoke(false);
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            Log("Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case LastFetchStatus.Pending:
                    Log("Latest Fetch call still pending.");
                    OnRemoteConfigInitialized?.Invoke(false);
                    break;
            }

        }

        public static string GetString(string key)
        {
            if(!isInitialized)
            {
                Log("Remote config not initialized yet");
                return "";
            }
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
        }
        public static int GetInt(string key)
        {
            if (!isInitialized)
            {
                Log("Remote config not initialized yet");
                return 0;
            }
            return (int)FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
        }
        public static float GetFloat(string key)
        {
            if (!isInitialized)
            {
                Log("Remote config not initialized yet");
                return 0;
            }
            return (float)FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
        }
        public static bool GetBool(string key)
        {
            if (!isInitialized)
            {
                Log("Remote config not initialized yet");
                return false;
            }
            return FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
        }

        public static void EnableAutoFetch()
        {
            Log("Enabling auto-fetch:");
            FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener
                += ConfigUpdateListenerEventHandler;
        }

        public static void DisableAutoFetch()
        {
            Log("Disabling auto-fetch:");
            FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener
                -= ConfigUpdateListenerEventHandler;
        }
        static void ConfigUpdateListenerEventHandler(
          object sender, ConfigUpdateEventArgs args)
        {
            if (args.Error != RemoteConfigError.None)
            {
                Log(string.Format("Error occurred while listening: {0}", args.Error));
                return;
            }
            Log(string.Format("Auto-fetch has received a new config. Updated keys: {0}",
                string.Join(", ", args.UpdatedKeys)));
            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
              .ContinueWithOnMainThread(task =>
              {
                  Log(string.Format("Remote data loaded and ready (last fetch time {0}).",
                                      info.FetchTime));
              });
        }

        static void Log(string message, int logType = 0)
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