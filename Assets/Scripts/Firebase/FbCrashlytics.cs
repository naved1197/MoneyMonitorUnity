
using Firebase.Crashlytics;
using System;
using UnityEngine;
namespace CubeHole
{
    public class FbCrashlytics
    {
        public static void ThrowUncaughtException()
        {
            Log("Causing a platform crash.");
            throw new InvalidOperationException("Uncaught exception created from UI.");
        }

        // Log a caught exception.
        public static void LogCaughtException()
        {
            Log("Catching an logging an exception.");
            try
            {
                throw new InvalidOperationException("This exception should be caught");
            }
            catch (Exception ex)
            {
                Crashlytics.LogException(ex);
            }
        }

        // Write to the Crashlytics session log
        public static void WriteCustomLog(String s)
        {
            Log("Logging message to Crashlytics session: " + s);
            Crashlytics.Log(s);
        }

        // Add custom key / value pair to Crashlytics session
        public static void SetCustomKey(String key, String value)
        {
            Log("Setting Crashlytics Custom Key: <" + key + " / " + value + ">");
            Crashlytics.SetCustomKey(key, value);
        }

        // Set User Identifier for this Crashlytics session 
        public static void SetUserID(String id)
        {
            Log("Setting Crashlytics user identifier: " + id);
            Crashlytics.SetUserId(id);
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