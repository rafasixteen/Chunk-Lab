using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public static class ChunkSchedulerManagerExtensions
    {
        public static void ScheduleChunkDependenciesOf(this ChunkSchedulerManager manager, ChunkId chunkId, EChunkState desiredState)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(ScheduleChunkDependenciesOf)))
            {
                using NativeArray<ChunkId> dependencies = manager.ChunkDependencyManager.GetDependencies(chunkId, Allocator.Temp);

                for (int i = 0; i < dependencies.Length; i++)
                    manager.ScheduleChunk(dependencies[i], desiredState);
            }
        }
    }
}