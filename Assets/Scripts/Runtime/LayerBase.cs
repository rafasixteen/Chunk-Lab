using System.Collections.Generic;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class LayerBase
    {
        private UnityEngine.Pool.ObjectPool<ChunkBase> _chunkPool;
        private Dictionary<ChunkId, ChunkBase> _chunkDictionary;
        private List<ChunkBase> _chunks;

        public ChunkLabManager ChunkLabManager { get; internal set; }

        public LayerSettings Settings { get; internal set; }

        public string Name { get; internal set; }

        public LayerId Id { get; internal set; }

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
                    for (int i = 0; i < _chunks.Count; i++)
                        DrawChunk(_chunks[i]);
                }
            }
        }

        internal ChunkBase GetChunk(ChunkId chunkId)
        {
            return _chunkDictionary[chunkId];
        }

        internal bool HasChunk(ChunkId chunkId)
        {
            return _chunkDictionary.ContainsKey(chunkId);
        }

        internal bool TryGetChunk(ChunkId chunkId, out ChunkBase chunk)
        {
            return _chunkDictionary.TryGetValue(chunkId, out chunk);
        }

        internal ChunkBase CreateChunk(ChunkId chunkId)
        {
            ChunkBase chunk = _chunkPool.Get();
            _chunkDictionary.Add(chunkId, chunk);
            _chunks.Add(chunk);
            chunk.Id = chunkId;
            return chunk;
        }

        internal void ReleaseChunk(ChunkId chunkId)
        {
            ChunkBase chunk = GetChunk(chunkId);
            _chunkPool.Release(chunk);
            _chunkDictionary.Remove(chunk.Id);
            _chunks.Remove(chunk);
        }

        protected virtual void OnStart() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnDrawGizmos() { }

        private protected abstract ChunkBase InstantiateChunk();

        private void DrawChunk(ChunkBase chunk)
        {
            Gizmos.color = GetChunkColor(chunk.State);
            Gizmos.DrawWireCube(chunk.Bounds.Center, chunk.Bounds.Extents);
        }

        private Color GetChunkColor(EChunkState chunkState)
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
}