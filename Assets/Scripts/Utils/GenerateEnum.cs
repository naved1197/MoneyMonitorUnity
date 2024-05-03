#if UNITY_EDITOR
using UnityEditor;
using System.IO;

public class GenerateEnum
{
    public static void GenerateEnums(string name, string[] entries)
    {
        string postFix = "R_";
        string enumName = postFix+name;
        string[] enumEntries = entries;
        for (int i = 0; i < enumEntries.Length; i++)
        {
            enumEntries[i] = enumEntries[i].Replace(" ", "_");
            enumEntries[i] = enumEntries[i].Replace("&", "");
            //remove special characters
        }
        string filePathAndName = "Assets/Scripts/Enums/" + enumName + ".cs"; //The folder Scripts/Enums/ is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
        {
            streamWriter.WriteLine("public enum " + enumName);
            streamWriter.WriteLine("{");
            for (int i = 0; i < enumEntries.Length; i++)
            {
                streamWriter.WriteLine("\t" + enumEntries[i] + ",");
            }
            streamWriter.WriteLine("}");
        }
        AssetDatabase.Refresh();
    }
}
#endif