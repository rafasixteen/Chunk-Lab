using Unity.Burst;
using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    [BurstCompile]
    public static class ChunkDependencyManagerExtensions
    {
        public static bool AreDependenciesLoaded(this ChunkDependencyManager manager, ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(AreDependenciesLoaded)))
            {
                using NativeArray<ChunkId> dependencies = manager.GetDependencies(chunkId, Allocator.Temp);

                for (int i = 0; i < dependencies.Length; i++)
                {
                    ChunkId dependencyId = dependencies[i];

                    if (!manager.ChunkStateManager.HasState(dependencyId))
                        return false;

                    if (manager.ChunkStateManager.GetState(dependencyId) != EChunkState.Loaded)
                        return false;
                }

                return true;
            }
        }

        public static void AddDependencies(this ChunkDependencyManager manager, ChunkId dependent, in NativeArray<ChunkId> dependencies)
        {
            for (int i = 0; i < dependencies.Length; i++)
                manager.AddDependency(dependent, dependencies[i]);
        }
    }
}