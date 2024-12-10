using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public static class ChunkLabLogger
    {
        private static bool _enableLogger;

        static ChunkLabLogger()
        {
            _enableLogger = true;
        }

        public static bool EnableProfiler
        {
            get => _enableLogger;
            set => _enableLogger = value;
        }

        public static void Log(string message)
        {
            Debug.Log(message);
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
    }
}