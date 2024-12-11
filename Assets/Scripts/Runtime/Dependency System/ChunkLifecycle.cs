namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class ChunkLifecycle
    {
        internal virtual void OnChunkLoadedInternal(ChunkId chunkId) => OnChunkLoaded(chunkId);

        internal virtual void OnChunkUnloadedInternal(ChunkId chunkId) => OnChunkUnloaded(chunkId);

        internal virtual void OnChunkLoadingInternal(ChunkId chunkId) => OnChunkLoading(chunkId);

        internal virtual void OnChunkUnloadingInternal(ChunkId chunkId) => OnChunkUnloading(chunkId);

        internal virtual void OnChunkAwaitingLoadingInternal(ChunkId chunkId) => OnChunkAwaitingLoading(chunkId);

        internal virtual void OnChunkAwaitingUnloadingInternal(ChunkId chunkId) => OnChunkAwaitingUnloading(chunkId);

        protected virtual void OnChunkLoaded(ChunkId chunkId) { }

        protected virtual void OnChunkUnloaded(ChunkId chunkId) { }

        protected virtual void OnChunkLoading(ChunkId chunkId) { }

        protected virtual void OnChunkUnloading(ChunkId chunkId) { }

        protected virtual void OnChunkAwaitingLoading(ChunkId chunkId) { }

        protected virtual void OnChunkAwaitingUnloading(ChunkId chunkId) { }
    }
}