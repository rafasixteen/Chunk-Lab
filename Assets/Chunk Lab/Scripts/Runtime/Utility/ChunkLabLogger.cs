using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    internal static class ChunkLabLogger
    {
        private static bool _enableLogger;

        static ChunkLabLogger()
        {
            _enableLogger = false;
        }

        public static void SetActive(bool value)
        {
            _enableLogger = value;
        }

        public static void Log(string message)
        {
            if (_enableLogger)
                Debug.Log(message);
        }

        public static void LogWarning(string message)
        {
            if (_enableLogger)
                Debug.LogWarning(message);
        }

        public static void LogError(string message)
        {
            if (_enableLogger)
                Debug.LogError(message);
        }
    }
}