using Firebase;
using Firebase.Extensions;
using System;
using UnityEngine;
namespace CubeHole
{
    public class FirebaseManager : MonoBehaviour
    {
        [SerializeField] private FirebaseData firebaseData;

        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        public static bool firebaseInitialized;

        private void Awake()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                      "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }
        void InitializeFirebase()
        {
            Log("FireBase initialized");
            //firebaseInitialized = true;
            //FirebaseAnalyticsData firebaseAnalyticsData = firebaseData.firebaseAnalyticsData;
            //FbAnalytics.Init(firebaseAnalyticsData.EnableAnalytics, firebaseAnalyticsData.GetUserPropertiesDictionary(), firebaseAnalyticsData.UserId, firebaseAnalyticsData.SessionDurationInMinutes);
            //FirebaseRemoteConfigData firebaseRemoteConfigData = firebaseData.firebaseRemoteConfigData;
            //FbRemoteConfig.Init(firebaseRemoteConfigData.GetStringValuesDictionary(), firebaseRemoteConfigData.GetIntValuesDictionary(), firebaseRemoteConfigData.GetFloatValuesDictionary(), firebaseRemoteConfigData.GetBoolValuesDictionary(),new TimeSpan( 0,firebaseRemoteConfigData.cacheExpirationInMinutes,0));
            //if(firebaseRemoteConfigData.EnableRealTimeUpdates)
            //{
            //    FbRemoteConfig.EnableAutoFetch();
            //}
            //if(firebaseAnalyticsData.UserId!="")
            //{
            //    FbCrashlytics.SetUserID(firebaseAnalyticsData.UserId);
            //}
            //FbNotifications.Init("MM");
            //FbNotifications.RequestPermission();
        }

        private void OnDestroy()
        {
            FbRemoteConfig.DisableAutoFetch();
        }

        public void Log(string message)
        {
            Debug.Log(message);
        }
    }
}