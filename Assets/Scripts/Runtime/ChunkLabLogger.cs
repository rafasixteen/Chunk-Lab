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

        public static void Log(string context, string message)
        {
            Debug.Log($"[{context}] {message}");
        }

        public static void LogWarning(string context, string message)
        {
            Debug.LogWarning($"[{context}] {message}");
        }

        public static void LogError(string context, string message)
        {
            Debug.LogError($"[{context}] {message}");
        }
    }
}