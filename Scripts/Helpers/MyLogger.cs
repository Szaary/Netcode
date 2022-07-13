using UnityEngine;

public enum LogLevel
{
    Full,
    None
}

public static class MyLogger
{
    public static LogLevel logLevel = LogLevel.Full;
    
    public static void Log(string log)
    {
        if (logLevel == LogLevel.Full)
        {
            Debug.Log(log); 
        }
        else
        {
            
        } 
    }

    public static void LogError(string log)
    {
        Debug.LogError(log);
    }
    
    public static void Log(string log, LogLevel logLevel)
    {
        if (logLevel == LogLevel.Full)
        {
            Debug.Log(log); 
        }
        else
        {
            
        }
    }

    public static void LogWarning(string log)
    {
        Debug.LogWarning(log);
    }
}