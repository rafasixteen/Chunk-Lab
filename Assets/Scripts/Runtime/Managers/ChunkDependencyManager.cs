using System;
using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class ChunkDependencyManager : IDisposable
    {
        private NativeHashMap<ChunkId, NativeHashSet<ChunkId>> _dependencies;
        private NativeHashMap<ChunkId, NativeHashSet<ChunkId>> _dependents;

        public ChunkDependencyManager()
        {
            _dependencies = new(0, Allocator.Persistent);
            _dependents = new(0, Allocator.Persistent);
        }

        public ChunkStateManager ChunkStateManager { get; internal set; }

        public void AddDependency(ChunkId dependent, ChunkId dependency)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(AddDependency)))
            {
                if (dependent.LayerId == dependency.LayerId)
                    throw new InvalidOperationException($"Cannot add dependency between chunks {dependent} and {dependency} because they belong to the same layer.");

                if (!HasDependencies(dependency))
                    _dependencies[dependency] = new NativeHashSet<ChunkId>(0, Allocator.Persistent);

                if (!HasDependents(dependent))
                    _dependents[dependent] = new NativeHashSet<ChunkId>(0, Allocator.Persistent);

                if (_dependencies[dependency].Add(dependent))
                    ChunkLabLogger.Log($"Successfully added dependent {dependent} to chunk {dependency}.");
                else
                    ChunkLabLogger.LogWarning($"Duplicate dependent {dependent} added for chunk {dependency}.");

                if (_dependents[dependent].Add(dependency))
                    ChunkLabLogger.Log($"Successfully added dependency {dependency} to chunk {dependent}.");
                else
                    ChunkLabLogger.LogWarning($"Duplicate dependency {dependency} added for chunk {dependent}.");
            }
        }

        public void RemoveDependency(ChunkId dependent, ChunkId dependency)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(RemoveDependency)))
            {
                if (HasDependencies(dependency))
                {
                    if (_dependencies[dependency].Remove(dependent))
                        ChunkLabLogger.Log($"Successfully removed dependent {dependent} from chunk {dependency}.");
                    else
                        ChunkLabLogger.LogWarning($"Trying to remove non-existent dependent {dependent} from chunk {dependency}.");
                }
                else
                {
                    ChunkLabLogger.LogWarning($"Trying to remove dependent {dependent}, but chunk {dependency} does not exist.");
                }

                if (HasDependents(dependent))
                {
                    if (_dependents[dependent].Remove(dependency))
                        ChunkLabLogger.Log($"Successfully removed dependency {dependency} from chunk {dependent}.");
                    else
                        ChunkLabLogger.LogWarning($"Trying to remove non-existent dependency {dependency} from chunk {dependent}.");
                }
                else
                {
                    ChunkLabLogger.LogWarning($"Trying to remove dependency {dependency}, but chunk {dependent} does not exist.");
                }
            }
        }

        public NativeArray<ChunkId> GetDependencies(ChunkId chunkId, Allocator allocator)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(GetDependencies)))
            {
                if (!HasDependencies(chunkId))
                    return new NativeArray<ChunkId>(0, allocator);

                return _dependencies[chunkId].ToNativeArray(allocator);
            }
        }

        public NativeArray<ChunkId> GetDependents(ChunkId chunkId, Allocator allocator)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(GetDependents)))
            {
                if (!HasDependents(chunkId))
                    return new NativeArray<ChunkId>(0, allocator);

                return _dependents[chunkId].ToNativeArray(allocator);
            }
        }

        public bool HasDependencies(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(HasDependencies)))
            {
                return _dependencies.ContainsKey(chunkId) && !_dependencies[chunkId].IsEmpty;
            }
        }

        public bool HasDependents(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(HasDependents)))
            {
                return _dependents.ContainsKey(chunkId) && !_dependents[chunkId].IsEmpty;
            }
        }

        public void Dispose()
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(Dispose)))
            {
                using (NativeArray<NativeHashSet<ChunkId>> dependencies = _dependencies.GetValueArray(Allocator.Temp))
                {
                    for (int i = 0; i < dependencies.Length; i++)
                        dependencies[i].Dispose();
                }

                using (NativeArray<NativeHashSet<ChunkId>> dependents = _dependents.GetValueArray(Allocator.Temp))
                {
                    for (int i = 0; i < dependents.Length; i++)
                        dependents[i].Dispose();
                }

                _dependencies.Dispose();
                _dependents.Dispose();
            }
        }
    }
}