using System;
using Unity.Mathematics;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class ChunkProcessingManager
    {
        public ChunkSchedulerManager ChunkSchedulerManager { get; set; }
        public ChunkStateManager ChunkStateManager { get; set; }
        public ChunkDependencyManager ChunkDependencyManager { get; set; }
        public LayerManager LayerManager { get; set; }

        public void Process(int maxChunksPerFrame)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkProcessingManager), nameof(Process)))
            {
                int chunksToProcess = math.min(ChunkSchedulerManager.Count, maxChunksPerFrame);

                for (int i = 0; i < chunksToProcess; i++)
                {
                    if (!ChunkSchedulerManager.TryDequeue(out ChunkId chunkId))
                        continue;

                    EChunkState chunkState = ChunkStateManager.GetState(chunkId);

                    switch (chunkState)
                    {
                        case EChunkState.AwaitingLoading:
                            ProcessAwaitingLoadingChunk(chunkId);
                            break;
                        case EChunkState.AwaitingUnloading:
                            ProcessAwaitingUnloadingChunk(chunkId);
                            break;
                        default:
                            throw new InvalidOperationException($"Cannot process chunk {chunkId} with state {chunkState}");
                    }
                }
            }
        }

        private void ProcessAwaitingLoadingChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkProcessingManager), nameof(ProcessAwaitingLoadingChunk)))
            {
                if (ChunkDependencyManager.AreDependenciesLoaded(chunkId))
                {
                    LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);

                    if (!layer.TryGetChunk(chunkId, out ChunkBase chunk))
                        chunk = layer.CreateChunk(chunkId);

                    chunk.StartLoadingInternal();
                }
                else
                {
                    ChunkSchedulerManager.ScheduleChunk(chunkId, EChunkState.AwaitingLoading);
                    ChunkSchedulerManager.ScheduleChunkDependenciesOf(chunkId, EChunkState.AwaitingLoading);
                }
            }
        }

        private void ProcessAwaitingUnloadingChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkProcessingManager), nameof(ProcessAwaitingUnloadingChunk)))
            {
                // If this chunk is still waiting in the queue to be unloaded, and a
                // another chunk adds this chunk as a dependency, once this chunk is
                // processed, it will have dependents. We must remove this chunk from
                // the queue when a dependent is added.
                if (ChunkDependencyManager.HasDependents(chunkId))
                {
                    // I think we can just ignore this, because this chunk will be marked as NotLoaded
                    // and then the chunk that depends on this chunk will just schedule this chunk for
                    // loading again.
                }
                else
                {
                    LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                    ChunkBase chunk = layer.GetChunk(chunkId);
                    chunk.StartUnloadingInternal();
                }
            }
        }
    }
}