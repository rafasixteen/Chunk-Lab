using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class LayerBase
    {
        #region FIELDS

        private UnityEngine.Pool.ObjectPool<ChunkBase> _chunkPool;
        private Dictionary<ChunkId, ChunkBase> _chunkDictionary;
        private List<ChunkBase> _chunks;

        #endregion

        #region PROPERTIES

        public ChunkLabManager ChunkLabManager { get; internal set; }

        public ChunkDependencyManager ChunkDependencyManager => ChunkLabManager.ChunkDependencyManager;

        public ChunkStateManager ChunkStateManager => ChunkLabManager.ChunkStateManager;

        public ChunkSchedulerManager ChunkSchedulerManager => ChunkLabManager.ChunkSchedulerManager;

        public LayerManager LayerManager => ChunkLabManager.LayerManager;

        public LayerSettings Settings { get; internal set; }

        public string Name { get; internal set; }

        public LayerId Id { get; internal set; }

        #endregion

        #region METHODS

        #region LIFECYCLE

        internal void OnStartInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnStartInternal)))
            {
                _chunkPool = new(InstantiateChunk);
                _chunkDictionary = new();
                _chunks = new();

                OnStart();
            }
        }

        internal void OnDestroyInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnDestroyInternal)))
            {
                foreach (ChunkBase chunk in _chunks)
                    _chunkPool.Release(chunk);

                _chunkPool.Dispose();

                OnDestroy();
            }
        }

        internal void OnDrawGizmosInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnDrawGizmosInternal)))
            {
                EVisualizationSettings settings = Settings.VisualizationSettings;

                if (!Settings.EnableVisualization)
                    return;

                if (settings.HasFlag(EVisualizationSettings.Custom))
                    OnDrawGizmos();

                if (settings.HasFlag(EVisualizationSettings.ShowChunkBounds))
                {
                    using NativeArray<ChunkId> chunkIds = ChunkStateManager.GetChunkIdsOfLayer(Id, Allocator.Temp);

                    for (int i = 0; i < chunkIds.Length; i++)
                        DrawChunk(chunkIds[i]);
                }
            }
        }

        internal void OnChunkLoadedInternal(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnChunkLoadedInternal)))
            {
                OnChunkLoaded(chunkId);
            }
        }

        internal void OnChunkUnloadedInternal(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnChunkUnloadedInternal)))
            {
                OnChunkUnloaded(chunkId);
            }
        }

        internal void OnChunkLoadingInternal(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnChunkLoadingInternal)))
            {
                OnChunkLoading(chunkId);
            }
        }

        internal void OnChunkUnloadingInternal(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnChunkUnloadingInternal)))
            {
                OnChunkUnloading(chunkId);
            }
        }

        internal void OnChunkAwaitingLoadingInternal(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnChunkAwaitingLoadingInternal)))
            {
                OnChunkAwaitingLoading(chunkId);
            }
        }

        internal void OnChunkAwaitingUnloadingInternal(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnChunkAwaitingUnloadingInternal)))
            {
                OnChunkAwaitingUnloading(chunkId);
            }
        }

        protected virtual void OnStart() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnDrawGizmos() { }

        protected virtual void OnChunkLoaded(ChunkId chunkId) { }

        protected virtual void OnChunkUnloaded(ChunkId chunkId) { }

        protected virtual void OnChunkLoading(ChunkId chunkId) { }

        protected virtual void OnChunkUnloading(ChunkId chunkId) { }

        protected virtual void OnChunkAwaitingLoading(ChunkId chunkId) { }

        protected virtual void OnChunkAwaitingUnloading(ChunkId chunkId) { }

        #endregion

        internal bool HasChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(HasChunk)))
            {
                return _chunkDictionary.ContainsKey(chunkId);
            }
        }

        internal bool TryGetChunk(ChunkId chunkId, out ChunkBase chunk)
        {
            using (ProfilerUtility.StartSample(Name, nameof(TryGetChunk)))
            {
                return _chunkDictionary.TryGetValue(chunkId, out chunk);
            }
        }

        internal ChunkBase GetChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(GetChunk)))
            {
                return _chunkDictionary[chunkId];
            }
        }

        internal ChunkBase CreateChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(CreateChunk)))
            {
                ChunkBase chunk = _chunkPool.Get();
                _chunkDictionary.Add(chunkId, chunk);
                _chunks.Add(chunk);
                chunk.Id = chunkId;
                return chunk;
            }
        }

        internal void ReleaseChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(ReleaseChunk)))
            {
                ChunkBase chunk = GetChunk(chunkId);
                _chunkPool.Release(chunk);
                _chunkDictionary.Remove(chunk.Id);
                _chunks.Remove(chunk);
            }
        }

        private protected abstract ChunkBase InstantiateChunk();

        private void DrawChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(Name, nameof(DrawChunk)))
            {
                EChunkState state = ChunkStateManager.GetState(chunkId);

                int3 position = chunkId.Coords * chunkId.Size;
                MinMaxAABB bounds = new(position, position + chunkId.Size);

                Gizmos.color = GetChunkColor(state);
                Gizmos.DrawWireCube(bounds.Center, bounds.Extents);
            }
        }

        private Color GetChunkColor(EChunkState chunkState)
        {
            using (ProfilerUtility.StartSample(Name, nameof(GetChunkColor)))
            {
                if (!Settings.VisualizationSettings.HasFlag(EVisualizationSettings.HighlightChunkBoundsByState))
                    return Settings.ChunkColor;

                return chunkState switch
                {
                    EChunkState.AwaitingLoading => Settings.ChunkColorPendingCreation,
                    EChunkState.AwaitingUnloading => Settings.ChunkColorPendingDestruction,
                    EChunkState.Loading => Settings.ChunkColorCreating,
                    EChunkState.Loaded => Settings.ChunkColor,
                    _ => Color.black,
                };
            }
        }

        #endregion
    }
}