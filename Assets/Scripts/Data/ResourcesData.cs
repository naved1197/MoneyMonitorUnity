using System.Collections.Generic;
using UnityEngine;
using MyBox;
namespace CubeHole
{
    [System.Serializable]
    public class AudioResource
    {
        public string name;
        public AudioClip clip;
        public float volume = 1;
        public bool pitchChange = false;
    }
    [System.Serializable]
    public class AudioResourceLibrary
    {
        public string name;
        public List<AudioResource> audioResources = new List<AudioResource>();
        public AudioResource GetSpriteResource(string name)
        {
            for (int i = 0; i < audioResources.Count; i++)
            {
                if (audioResources[i].name == name)
                    return audioResources[i];
            }
            Logger.LogError("audioResources: " + name + " not found");
            return audioResources[0];
        }
    }

    [System.Serializable]
    public class SpriteResource
    {
        public string name;
        public Sprite sprite;
        public Color defaultColor = Color.white;
    }
    [System.Serializable]
    public class SpriteResourceLibrary
    {
        public string name;
        public List<SpriteResource> spriteResources = new List<SpriteResource>();
        public SpriteResource GetSpriteResource(string name)
        {
            for (int i = 0; i < spriteResources.Count; i++)
            {
                if (spriteResources[i].name == name)
                    return spriteResources[i];
            }
            Logger.LogError("SpriteResource: " + name + " not found");
            return spriteResources[0];
        }
    }
    [System.Serializable]
    public class StringResource 
    { 
        public string name;
        [Multiline]
        public string value; 
    }
    [System.Serializable]
    public class StringResourceLibrary
    {
        public string name;
        public List<StringResource> stringLibraries;
        public string GetStringResource(string name)
        {
            for (int i = 0; i < stringLibraries.Count; i++)
            {
                if (stringLibraries[i].name == name)
                    return stringLibraries[i].value;
            }
            Logger.LogError("StringResource: " + name + " not found");
            return null;
        }
    }
    [System.Serializable]
    public class ColorResource
    {
        public string name;
        public Color color;
    }
    [System.Serializable]
    public class ColorResourceLibrary
    {
        public string name;
        public List<ColorResource> colorResources;
        public Color GetColorResource(string name)
        {
            for (int i = 0; i < colorResources.Count; i++)
            {
                if (colorResources[i].name == name)
                    return colorResources[i].color;
            }
            Logger.LogError("ColorResource: " + name + " not found");
            return Color.white;
        }
    }
    [CreateAssetMenu(fileName = "App Resources", menuName = "App_Data/ResourcesData")]
    public class ResourcesData : ScriptableObject
    {
        public List<SpriteResourceLibrary> drawablesResources = new List<SpriteResourceLibrary>();
        public List<StringResourceLibrary> stringResources = new List<StringResourceLibrary>();
        public List<ColorResourceLibrary> ColorResources = new List<ColorResourceLibrary>();
        public List<AudioResourceLibrary> audioResources = new List<AudioResourceLibrary>();
#if UNITY_EDITOR
        [ButtonMethod]
        public void GenerateEnums()
        {
            List<string> enumNames = new List<string>();
            List<string> subEnumNames = new List<string>();
            void ResetLists()
            {
                enumNames.Clear();
                subEnumNames.Clear();
                enumNames.Add("None");
                subEnumNames.Add("None");
            }
            ResetLists();

            for (int i = 0; i < drawablesResources.Count; i++)
            {
                string name= drawablesResources[i].name;
                if(enumNames.Contains(name))
                {
                    Debug.LogError("Duplicate name: " + name);
                    continue;
                }
                enumNames.Add(name);
                for (int j = 0; j < drawablesResources[i].spriteResources.Count; j++)
                {
                    if (subEnumNames.Contains(drawablesResources[i].spriteResources[j].name))
                    {
                        Debug.LogError("Duplicate name: " + drawablesResources[i].spriteResources[j].name);
                        continue;
                    }
                    subEnumNames.Add(drawablesResources[i].spriteResources[j].name);
                }
                GenerateEnum.GenerateEnums(name, subEnumNames.ToArray());
                subEnumNames.Clear();
            }
            GenerateEnum.GenerateEnums("Drawables", enumNames.ToArray());

            ResetLists();
            for (int i = 0; i < stringResources.Count; i++)
            {
                string name = stringResources[i].name;
                if (enumNames.Contains(name))
                {
                    Debug.LogError("Duplicate name: " + name);
                    continue;
                }
                enumNames.Add(name);
                for (int j = 0; j < stringResources[i].stringLibraries.Count; j++)
                {
                    if (subEnumNames.Contains(stringResources[i].stringLibraries[j].name))
                    {
                        Debug.LogError("Duplicate name: " + stringResources[i].stringLibraries[j].name);
                        continue;
                    }
                    subEnumNames.Add(stringResources[i].stringLibraries[j].name);
                }
                GenerateEnum.GenerateEnums(name, subEnumNames.ToArray());
                subEnumNames.Clear();
            }
            GenerateEnum.GenerateEnums("Strings", enumNames.ToArray());

            ResetLists();
            for (int i = 0; i < ColorResources.Count; i++)
            {
                string name = ColorResources[i].name;
                if (enumNames.Contains(name))
                {
                    Debug.LogError("Duplicate name: " + name);
                    continue;
                }
                enumNames.Add(name);
                for (int j = 0; j < ColorResources[i].colorResources.Count; j++)
                {
                    if (subEnumNames.Contains(ColorResources[i].colorResources[j].name))
                    {
                        Debug.LogError("Duplicate name: " + ColorResources[i].colorResources[j].name);
                        continue;
                    }
                    subEnumNames.Add(ColorResources[i].colorResources[j].name);
                }
                GenerateEnum.GenerateEnums(name, subEnumNames.ToArray());
                subEnumNames.Clear();
            }  
            GenerateEnum.GenerateEnums("Colors", enumNames.ToArray());

            ResetLists();
            for (int i = 0; i < audioResources.Count; i++)
            {
                string name = audioResources[i].name;
                if (enumNames.Contains(name))
                {
                    Debug.LogError("Duplicate name: " + name);
                    continue;
                }
                enumNames.Add(name);
                for (int j = 0; j < audioResources[i].audioResources.Count; j++)
                {
                    if (subEnumNames.Contains(audioResources[i].audioResources[j].name))
                    {
                        Debug.LogError("Duplicate name: " + audioResources[i].audioResources[j].name);
                        continue;
                    }
                    subEnumNames.Add(audioResources[i].audioResources[j].name);
                }
                GenerateEnum.GenerateEnums(name, subEnumNames.ToArray());
                subEnumNames.Clear();
            }
            GenerateEnum.GenerateEnums("Audio", enumNames.ToArray());
            ResetLists();

        }
#endif

    }
}

