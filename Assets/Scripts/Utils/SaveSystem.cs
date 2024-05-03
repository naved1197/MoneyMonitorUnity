using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using CubeHole.MM;
using System;

namespace CubeHole
{
    public class SaveSystem
    {
       public static bool CheckIfDataExist(string path)
        {
            return (File.Exists(path));
        }
        public static void SaveData<T>(string path,string fileName,T data)
        {
            string dataPath = Application.persistentDataPath + path;
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            BinaryFormatter bf = new BinaryFormatter();
            dataPath += "/" + fileName;
            FileStream file = File.Create(dataPath);
            var json = JsonUtility.ToJson(data);
            bf.Serialize(file, json);
            file.Close();
            Logger.LogInfo($"File {fileName} Saved at {path}");
        }
        public static void LoadData<T>(string path, string fileName, Action<T> OnDataLoaded, Action OnDataLoadFailed) where T:new()
        {
            string dataPath = Application.persistentDataPath + path + "/" + fileName;
            Logger.Log($"Loading {fileName} File from {path}");
            T dataObject = new T();
            if (CheckIfDataExist(dataPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file;
                try
                {
                    file = File.Open(dataPath, FileMode.Open);
                    var data = (string)bf.Deserialize(file);
                    JsonUtility.FromJsonOverwrite(data, dataObject);
                    file.Close();
                }
                catch(Exception e)
                {
                    OnDataLoadFailed?.Invoke();
                    Logger.Log($"File {fileName} Loaded Failed from {path} reason {e.Message}");
                    return;
                }
                if (dataObject != null)
                {
                    OnDataLoaded?.Invoke(dataObject);
                    Logger.LogInfo($"File {fileName} Loaded from {path}");
                }
                else
                {
                    OnDataLoadFailed?.Invoke();
                    Logger.Log($"File {fileName} Loaded Failed from {path}");
                }
            }
            else
            {
                OnDataLoadFailed?.Invoke();
                Logger.Log($"File {fileName} Loaded Failed from {path}");
            }
        }
        public static void LoadSMS(string path,string fileName,Action<SMSData> OnDataLoaded,Action OnDataLoadFailed)
        {
            string dataPath = Application.persistentDataPath + path + "/" + fileName;
            SMSData smsData = (SMSData)ScriptableObject.CreateInstance(nameof(SMSData));
            Logger.Log($"Loading {fileName} File from {path}");
            if (CheckIfDataExist(dataPath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file;
                try
                {
                    file = File.Open(dataPath, FileMode.Open);
                    var data = (string)bf.Deserialize(file);
                    JsonUtility.FromJsonOverwrite(data, smsData);
                    file.Close();
                }
                catch
                {
                    OnDataLoadFailed?.Invoke();
                    Logger.Log($"File {fileName} Loaded Failed from {path}");
                    return;
                }
                if (smsData != null)
                {
                    OnDataLoaded?.Invoke(smsData);
                    Logger.LogInfo($"File {fileName} Loaded from {path}");
                }
                else
                {
                    OnDataLoadFailed?.Invoke();
                    Logger.Log($"File {fileName} Loaded Failed from {path}");
                }
            }
            else
            {
                OnDataLoadFailed?.Invoke();
                Logger.Log($"File {fileName} Loaded Failed from {path}");
            }
        }
    }
}
