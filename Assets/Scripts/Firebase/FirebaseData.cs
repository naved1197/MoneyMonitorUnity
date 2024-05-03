
using System.Collections.Generic;
using UnityEngine;

namespace CubeHole
{
    [System.Serializable]
    public class MyStringStringPair
    {
        public string key; public string value;
    }
    [System.Serializable]
    public class MyStringIntPair
    {
        public string key; public int value;
    }
    [System.Serializable]
    public class MyStringFloatPair
    {
        public string key; public float value;
    }
         [System.Serializable]
    public class MyStringBoolPair
    {
        public string key; public bool value;
    }
    [System.Serializable]
    public class FirebaseAnalyticsData
    {
        public bool EnableAnalytics = true;
        public int SessionDurationInMinutes = -1;
        public string UserId = "";
        [SerializeField] private MyStringStringPair[] userProperties;
        public string AnalyticsDebugCommand = "adb shell setprop debug.firebase.analytics.app PACKAGE_NAME";
        public string AnalyticsDisableDebugCommand = "adb shell setprop debug.firebase.analytics.app .none.";
        private readonly Dictionary<string, string> userPropertiesDictionary = new Dictionary<string, string>();
        public Dictionary<string, string> GetUserPropertiesDictionary()
        {
            foreach (var item in userProperties)
            {
                userPropertiesDictionary.Add(item.key, item.value);
            }
            return userPropertiesDictionary;
        }
    }
    [System.Serializable]
    public class FirebaseRemoteConfigData
    {
       public int cacheExpirationInMinutes = 0;
        public bool EnableRealTimeUpdates = false;
        [SerializeField] private MyStringStringPair[] stringValues;
        [SerializeField] private MyStringIntPair[] intValues;
        [SerializeField] private MyStringFloatPair[] floatValues;
        [SerializeField] private MyStringBoolPair[] boolValues;
        private readonly Dictionary<string, string> stringValuesDictionary = new Dictionary<string, string>();
        private readonly Dictionary<string, int> intValuesDictionary = new Dictionary<string, int>();
        private readonly Dictionary<string, float> floatValuesDictionary = new Dictionary<string, float>();
        private readonly Dictionary<string, bool> boolValuesDictionary = new Dictionary<string, bool>();

        public Dictionary<string, string> GetStringValuesDictionary()
        {
            foreach (var item in stringValues)
            {
                stringValuesDictionary.Add(item.key, item.value);
            }
            return stringValuesDictionary;
        }
        public Dictionary<string, int> GetIntValuesDictionary()
        {
            foreach (var item in intValues)
            {
                intValuesDictionary.Add(item.key, item.value);
            }
            return intValuesDictionary;
        }
        public Dictionary<string, float> GetFloatValuesDictionary()
        {
            foreach (var item in floatValues)
            {
                floatValuesDictionary.Add(item.key, item.value);
            }
            return floatValuesDictionary;
        }
        public Dictionary<string, bool> GetBoolValuesDictionary()
        {
            foreach (var item in boolValues)
            {
                boolValuesDictionary.Add(item.key, item.value);
            }
            return boolValuesDictionary;
        }
    }
    [CreateAssetMenu(fileName = "Firebase Data", menuName = "Firebase/Firebase Data")]
    public class FirebaseData : ScriptableObject
    {
        [Header("Analytics")]
        public FirebaseAnalyticsData firebaseAnalyticsData;
        [Header("Analytics")]
        public FirebaseRemoteConfigData firebaseRemoteConfigData;
        private void OnValidate()
        {
            if(firebaseAnalyticsData == null)
            {
                return;
            }
            if (firebaseAnalyticsData.AnalyticsDebugCommand == "")
            {
                firebaseAnalyticsData.AnalyticsDebugCommand = "adb shell setprop debug.firebase.analytics.app " + Application.identifier;
            }
            if (firebaseAnalyticsData.AnalyticsDisableDebugCommand == "")
            {
                firebaseAnalyticsData.AnalyticsDisableDebugCommand = "adb shell setprop debug.firebase.analytics.app .none.";
            }
        }


        public void Log(string message, int logType = 0)
        {
            string log = this.name + ": " + message;
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
