using System.Collections.Generic;
using UnityEngine;
namespace CubeHole
{
    [System.Serializable]
    public class StringLibs
    {
        public List<StringResourceLibrary> libraries = new List<StringResourceLibrary>();
    }
    public class AppResources : MonoBehaviour
    {
        public ResourcesData resourcesData;
        private static ResourcesData resourcesDataPrivate;
        public StringLibs localStringLibs;
        public void Init()
        {
            Logger.LogInfo("AppResources Init");
            resourcesDataPrivate = resourcesData;
            LoadLocalStringResources();
        }
        public static StringResourceLibrary GetStringsLibrary(R_Strings name)
        {
            if(resourcesDataPrivate == null)
            {
                LogToServer("ResourcesDataIsNull");
                Debug.LogError("ResourcesData is null");
                return null;
            }
            var stringResources = resourcesDataPrivate.stringResources;
            for (int i = 0; i < stringResources.Count; i++)
            {
                if (stringResources[i].name == name.ToString())
                    return stringResources[i];
            }
            LogToServer("StringResourceLibrary:" + name + " NotFound");
            Logger.Log("StringResourceLibrary: " + name + " not found");
            return null;
        }
        public static SpriteResourceLibrary GetSpriteGroup(R_Drawables name)
        {
            if (resourcesDataPrivate == null)
            {
                LogToServer("ResourcesDataIsNull");
                Debug.LogError("ResourcesData is null");
                return null;
            }
            var drawablesResource = resourcesDataPrivate.drawablesResources;
            for (int i = 0; i < drawablesResource.Count; i++)
            {
                if (drawablesResource[i].name == name.ToString())
                    return drawablesResource[i];
            }
            LogToServer("SpriteResourceLibrary:" + name + " NotFound");
            Logger.Log("SpriteResourceLibrary: " + name + " not found");
            return null;
        }
        public static ColorResourceLibrary GetColorGroup(R_Colors name)
        {
              if (resourcesDataPrivate == null)
            {
                LogToServer("ResourcesDataIsNull");
                Debug.LogError("ResourcesData is null");
                return null;
            }
            var colorResources = resourcesDataPrivate.ColorResources;
            for (int i = 0; i < colorResources.Count; i++)
            {
                if (colorResources[i].name == name.ToString())
                    return colorResources[i];
            }
            LogToServer("ColorResourceLibrary:" + name + " NotFound");
            Logger.LogError("ColorResourceLibrary: " + name + " not found");
            return null;
        }
        public static AudioResourceLibrary GetAudioResourceLibrary(R_Audio name)
        {
            if (resourcesDataPrivate == null)
            {
                LogToServer("ResourcesDataIsNull");
                Logger.Log("ResourcesData is null");
                return null;
            }
            var audioResources = resourcesDataPrivate.audioResources;
            for (int i = 0; i < audioResources.Count; i++)
            {
                if (audioResources[i].name == name.ToString())
                    return audioResources[i];
            }
            LogToServer("AudioResourceLibrary:" + name + " NotFound");
            Logger.Log("AudioResourceLibrary: " + name + " not found");
            return null;
        }
        public void LoadStringsFromRemoteConfig()
        {
            if(!FbRemoteConfig.isInitialized)
            {
                Logger.Log("Remote config is not initialized");
                return;
            }
            StringLibs remoteStringLibs = new StringLibs();
            string remateStringLibsJson = FbRemoteConfig.GetString("stringLibs");
            Logger.Log("String resource from server is " + remateStringLibsJson);
            if (remateStringLibsJson == "")
            {
                Logger.LogError("Cannot load stringLibs from remote config");
                return;
            }
            remoteStringLibs = JsonUtility.FromJson<StringLibs>(remateStringLibsJson);
            if (remoteStringLibs.libraries.Count == 0)
            {
                Logger.LogError("Cannot load stringLibs from remote config");
                return;
            }
            resourcesData.stringResources = remoteStringLibs.libraries;
            SaveStringResources();
        }
        public void SaveStringResources()
        {
            if(resourcesDataPrivate==null)
            {
                LogToServer("CannotSaveStringData_ResourceDataPrivate_isNull");
                Logger.Log("Cannot Save data resourcesDataPrivate is null");
                return;
            }
            List<StringResourceLibrary> stringResources = new List<StringResourceLibrary>();
            for (int i = 0; i < resourcesDataPrivate.stringResources.Count; i++)
            {
                stringResources.Add(resourcesDataPrivate.stringResources[i]);
            }
            StringLibs stringLib = new StringLibs();
            stringLib.libraries = stringResources;
            string savePath = GetStringsLibrary(R_Strings.System).GetStringResource(R_System.SavePath.ToString());
            SaveSystem.SaveData(savePath, "strings.mm", stringLib);
        }
        public void LoadLocalStringResources()
        {
            string savePath = GetStringsLibrary(R_Strings.System).GetStringResource(R_System.SavePath.ToString());
            SaveSystem.LoadData<StringLibs>(savePath, "strings.mm", (localStrings) => { 
               localStringLibs= localStrings;
            }, () =>
            {
                Logger.Log("Cannot Load string data");
            });
            if (localStringLibs != null && localStringLibs.libraries.Count != 0)
            {
                Logger.Log("Loaded local string data");
                resourcesData.stringResources = localStringLibs.libraries;
            }
            else
            {
               LogToServer("CannotLoadLocalStringData"); 
                Logger.Log("Cannot Load local string data");

            }
        }
        static void LogToServer(string eventName)
        {
            FbAnalytics.LogEvent(eventName);
        }
    }
}
