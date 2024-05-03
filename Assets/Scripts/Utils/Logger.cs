using TMPro;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logTxt;
    public static Logger instance;
    public static bool isLogging = true;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);
        logTxt.text = "";  
    }

    #region Logging
    public static void LogError(string message)
    {
        if (instance != null)
        {
            message = "<color=red>" + message + "</color>";
            instance.LogMessage(message);
        }
        else
        {
            if (Application.isEditor)
                Debug.LogError(message);
            else
                Debug.Log(message);
        }
    }
    public static void LogWarning(string message)
    {
        if (instance != null)
        {
            message = "<color=yellow>" + message + "</color>";
            instance.LogMessage(message);
        }
        else
            Debug.LogWarning(message);
    }
    public static void LogInfo(string message)
    {
        if(instance != null)
        {
            message = "<color=green>" + message + "</color>";
            instance.LogMessage(message);
        }
        else
            Debug.Log(message);
    }
    public static void Log(string message)
    {
        if (instance != null)
            instance.LogMessage(message);
        else
            Debug.Log(message);
    }
    public void LogMessage(string message)
    {
        if (!isLogging)
            return;
        logTxt.text += "\n" + message;
        Debug.Log(message);
    }
    #endregion
}
