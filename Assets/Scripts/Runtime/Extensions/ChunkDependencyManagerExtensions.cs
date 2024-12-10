using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public static class ChunkDependencyManagerExtensions
    {
        public static void RemoveAllDependents(this ChunkDependencyManager manager, ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveAllDependents)))
            {
                using NativeArray<ChunkId> dependents = manager.GetDependents(chunkId, Allocator.Temp);

                for (int i = 0; i < dependents.Length; i++)
                    manager.RemoveDependency(dependents[i], chunkId);
            }
        }

        public static void RemoveAllDependencies(this ChunkDependencyManager manager, ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkStateManager), nameof(RemoveAllDependencies)))
            {
                using NativeArray<ChunkId> dependencies = manager.GetDependencies(chunkId, Allocator.Temp);

                for (int i = 0; i < dependencies.Length; i++)
                    manager.RemoveDependency(dependencies[i], chunkId);
            }
        }

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
    }
}
