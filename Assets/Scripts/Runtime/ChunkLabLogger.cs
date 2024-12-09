using Unity.Collections;
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

        public static string ArrayToString<T>(NativeArray<T> array)
            where T : unmanaged
        {
            if (!array.IsCreated || array.Length == 0)
                return "[]";

            string msg = "[";
            for (int i = 0; i < array.Length; i++)
            {
                msg += array[i].ToString();
                if (i < array.Length - 1)
                    msg += ", ";
            }
            msg += "]";
            return msg;
        }
    }
}