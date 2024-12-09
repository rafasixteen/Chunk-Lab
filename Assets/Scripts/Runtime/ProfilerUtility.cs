using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Rafasixteen.Runtime.ChunkLab
{
    /// <summary>
    /// A utility class for managing profiler samples with caching and scoped profiling.
    /// </summary>
    public static class ProfilerUtility
    {
        private static readonly DisposableAction _endSampleDisposable;
        private static readonly Dictionary<int, string> _sampleNameCache;
        private static bool _enableProfiler;

        static ProfilerUtility()
        {
            _endSampleDisposable = new(EndSample);
            _sampleNameCache = new();
            _enableProfiler = true;
        }

        /// <summary>
        /// Enables or disables the profiler dynamically.
        /// </summary>
        public static bool EnableProfiler
        {
            get => _enableProfiler;
            set => _enableProfiler = value;
        }

        /// <summary>
        /// Begins profiling a sample and returns a disposable to ensure paired profiling.
        /// </summary>
        /// <param name="contextName">The name of the context being profiled (e.g., system, component, etc.).</param>
        /// <param name="methodName">The name of the method or operation being profiled.</param>
        /// <returns>A disposable object to end the profiler sample.</returns>
        public static IDisposable StartSample(string contextName, string methodName)
        {
            if (_enableProfiler)
            {
                string cachedSampleName = GetCachedSampleName(contextName, methodName);
                Profiler.BeginSample(cachedSampleName);
            }

            return _endSampleDisposable;
        }

        /// <summary>
        /// Retrieves a cached sample name to avoid repeated string allocations.
        /// </summary>
        /// <param name="contextName">The name of the context.</param>
        /// <param name="methodName">The name of the method or operation.</param>
        /// <returns>A cached string for the profiler sample.</returns>
        private static string GetCachedSampleName(string contextName, string methodName)
        {
            int key = GetCachedSampleNameKey(contextName, methodName);

            if (!_sampleNameCache.TryGetValue(key, out string cachedName))
            {
                cachedName = $"[{contextName}] {methodName}";
                _sampleNameCache[key] = cachedName;
            }

            return cachedName;
        }

        private static int GetCachedSampleNameKey(string contextName, string methodName)
        {
            return HashCode.Combine(contextName, methodName);
        }

        private static void EndSample()
        {
            if (_enableProfiler)
                Profiler.EndSample();
        }

        /// <summary>
        /// A helper class for scoped profiling with the `using` statement.
        /// </summary>
        private class DisposableAction : IDisposable
        {
            private readonly Action _endAction;

            public DisposableAction(Action endAction) => _endAction = endAction;

            public void Dispose() => _endAction?.Invoke();
        }
    }
}