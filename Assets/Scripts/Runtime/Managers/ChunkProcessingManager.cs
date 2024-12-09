using System;
using Unity.Collections;
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
                    ChunkId chunkId = ChunkSchedulerManager.Dequeue();
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
                using NativeArray<ChunkId> dependencies = ChunkDependencyManager.GetDependencies(chunkId, Allocator.Temp);

                if (ChunkStateManager.AreChunksLoaded(dependencies))
                {
                    LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);

                    if (!layer.TryGetChunk(chunkId, out ChunkBase chunk))
                        chunk = layer.CreateChunk(chunkId);

                    chunk.StartLoadingInternal();
                }
                else
                {
                    ChunkSchedulerManager.ScheduleChunk(chunkId, EChunkState.AwaitingLoading);

                    for (int i = 0; i < dependencies.Length; i++)
                        ChunkSchedulerManager.ScheduleChunk(dependencies[i], EChunkState.AwaitingLoading);
                }
            }
        }

        private void ProcessAwaitingUnloadingChunk(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkProcessingManager), nameof(ProcessAwaitingUnloadingChunk)))
            {
                using NativeArray<ChunkId> dependents = ChunkDependencyManager.GetDependents(chunkId, Allocator.Temp);

                if (dependents.Length > 0)
                {
                    for (int i = 0; i < dependents.Length; i++)
                        ChunkSchedulerManager.ScheduleChunk(dependents[i], EChunkState.AwaitingUnloading);

                    ChunkSchedulerManager.ScheduleChunk(chunkId, EChunkState.AwaitingUnloading);
                }
                else
                {
                    LayerBase layer = LayerManager.GetLayer(chunkId.LayerId);
                    ChunkBase chunk = layer.GetChunk(chunkId);
                    chunk.StartUnloadingInternal();

                    using NativeArray<ChunkId> dependencies = ChunkDependencyManager.GetDependencies(chunkId, Allocator.Temp);

                    for (int i = 0; i < dependencies.Length; i++)
                        ChunkSchedulerManager.ScheduleChunk(dependencies[i], EChunkState.AwaitingUnloading);
                }
            }
        }
    }
}