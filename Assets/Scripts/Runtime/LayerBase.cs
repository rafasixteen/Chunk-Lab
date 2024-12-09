using System.Collections.Generic;

namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class LayerBase
    {
        private string _cachedName = default;

        private UnityEngine.Pool.ObjectPool<ChunkBase> _chunkPool;
        private Dictionary<ChunkId, ChunkBase> _chunkInstances;

        public string Name => _cachedName ??= GetType().Name;

        internal void OnStartInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnStartInternal)))
            {
                _chunkPool = new(InstantiateChunk);

                OnStart();
            }
        }

        internal void OnDestroyInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnDestroyInternal)))
            {
                OnDestroy();
            }
        }

        internal void OnDrawGizmosInternal()
        {
            using (ProfilerUtility.StartSample(Name, nameof(OnDrawGizmosInternal)))
            {
                OnDrawGizmos();
            }
        }

        internal ChunkBase GetChunk(ChunkId chunkId)
        {
            return _chunkInstances[chunkId];
        }

        internal bool HasChunk(ChunkId chunkId)
        {
            return _chunkInstances.ContainsKey(chunkId);
        }

        internal bool TryGetChunk(ChunkId chunkId, out ChunkBase chunk)
        {
            return _chunkInstances.TryGetValue(chunkId, out chunk);
        }

        internal ChunkBase CreateChunk(ChunkId chunkId)
        {
            ChunkBase chunk = _chunkPool.Get();
            _chunkInstances.Add(chunkId, chunk);
            chunk.Id = chunkId;
            return chunk;
        }

        internal void ReleaseChunk(ChunkId chunkId)
        {
            ChunkBase chunk = GetChunk(chunkId);
            _chunkPool.Release(chunk);
            _chunkInstances.Remove(chunk.Id);
        }

        protected virtual void OnStart() { }

        protected virtual void OnDestroy() { }

        protected virtual void OnDrawGizmos() { }

        private protected abstract ChunkBase InstantiateChunk();
    }
}