using System;

namespace Soda.Runtime.Utils
{
    using UnityEngine;

    public static class SodaLogger
    {
        private const string prefix = "<color=green>[Soda]-></color> ";
       
        private static bool loggingEnabled = true;

        public static void ToggleLogging(bool enabled)
        {
            loggingEnabled = enabled;
        }

        public static void Log(object message)
        {
            if (loggingEnabled)
                Debug.Log(prefix + message);
        }

        public static void LogWarning(object message)
        {
            if (loggingEnabled)
                Debug.LogWarning(prefix + message);
        }

        public static void LogError(object message)
        {
            if (loggingEnabled)
                Debug.LogError(prefix + message);
        }

        public static void LogEditor(object message)
        {
#if UNITY_EDITOR
            if (loggingEnabled)
                Debug.Log(prefix + message);
#endif
        }

        public static void LogWarningEditor(object message)
        {
#if UNITY_EDITOR
            if (loggingEnabled)
                Debug.LogWarning(prefix + message);
#endif
        }

        public static void LogErrorEditor(object message)
        {
#if UNITY_EDITOR
            if (loggingEnabled)
                Debug.LogError(prefix + message);
#endif
        }

        public static void LogException(Exception exception)
        {
            if (loggingEnabled)
                Debug.LogException(exception);
        }
    }
}