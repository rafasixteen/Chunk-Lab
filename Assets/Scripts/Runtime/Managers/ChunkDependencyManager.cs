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

        public LayerManager LayerManager { get; internal set; }

        /// <summary>
        /// Adds a dependency relationship between two chunks.
        /// </summary>
        /// <param name="chunkA">The chunk that depends on <paramref name="chunkB"/>.</param>
        /// <param name="chunkB">The chunk that <paramref name="chunkA"/> depends on.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add a dependency where both <paramref name="chunkA"/> 
        /// and <paramref name="chunkB"/> belong to the same layer.
        /// </exception>
        /// <remarks>
        /// This method establishes a two-way relationship between the chunks:<br/>
        /// - <paramref name="chunkB"/> will record <paramref name="chunkA"/> as one of its dependents.<br/>
        /// - <paramref name="chunkA"/> will record <paramref name="chunkB"/> as one of its dependencies.<br/>
        /// 
        /// This relationship ensures that:<br/>
        /// 1. <paramref name="chunkB"/> cannot be unloaded while it has active dependents like <paramref name="chunkA"/>.<br/>
        /// 2. <paramref name="chunkA"/> relies on <paramref name="chunkB"/> being loaded.<br/>
        /// </remarks>
        public void AddDependency(ChunkId chunkA, ChunkId chunkB)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(AddDependency)))
            {
                if (chunkA.LayerId == chunkB.LayerId)
                    throw new InvalidOperationException($"Cannot add dependency between chunks {chunkA} and {chunkB} because they belong to the same layer.");

                if (!HasDependencies(chunkB))
                    _dependencies[chunkB] = new NativeHashSet<ChunkId>(0, Allocator.Persistent);

                if (!HasDependents(chunkA))
                    _dependents[chunkA] = new NativeHashSet<ChunkId>(0, Allocator.Persistent);

                if (_dependencies[chunkB].Add(chunkA))
                    ChunkLabLogger.Log($"Successfully added dependent {chunkA} to chunk {chunkB}.");
                else
                    ChunkLabLogger.LogWarning($"Duplicate dependent {chunkA} added for chunk {chunkB}.");

                if (_dependents[chunkA].Add(chunkB))
                    ChunkLabLogger.Log($"Successfully added dependency {chunkB} to chunk {chunkA}.");
                else
                    ChunkLabLogger.LogWarning($"Duplicate dependency {chunkB} added for chunk {chunkA}.");
            }
        }

        /// <summary>
        /// Removes a dependency relationship between two chunks.
        /// </summary>
        /// <param name="chunkA">The chunk that depends on <paramref name="chunkB"/>.</param>
        /// <param name="chunkB">The chunk that <paramref name="chunkA"/> depends on.</param>
        /// <remarks>
        /// This method breaks the two-way relationship previously established by <see cref="AddDependency"/>:<br/>
        /// - <paramref name="chunkB"/> will no longer consider <paramref name="chunkA"/> as one of its dependents.<br/>
        /// - <paramref name="chunkA"/> will no longer consider <paramref name="chunkB"/> as one of its dependencies.<br/>
        /// 
        /// This ensures that <paramref name="chunkB"/> can now be safely unloaded if it has no other dependents.
        /// </remarks>
        public void RemoveDependency(ChunkId chunkA, ChunkId chunkB)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(RemoveDependency)))
            {
                if (HasDependencies(chunkB))
                {
                    if (_dependencies[chunkB].Remove(chunkA))
                        ChunkLabLogger.Log($"Successfully removed dependent {chunkA} from chunk {chunkB}.");
                    else
                        ChunkLabLogger.LogWarning($"Trying to remove non-existent dependent {chunkA} from chunk {chunkB}.");
                }
                else
                {
                    ChunkLabLogger.LogWarning($"Trying to remove dependent {chunkA}, but chunk {chunkB} does not exist.");
                }

                if (HasDependents(chunkA))
                {
                    if (_dependents[chunkA].Remove(chunkB))
                        ChunkLabLogger.Log($"Successfully removed dependency {chunkB} from chunk {chunkA}.");
                    else
                        ChunkLabLogger.LogWarning($"Trying to remove non-existent dependency {chunkB} from chunk {chunkA}.");
                }
                else
                {
                    ChunkLabLogger.LogWarning($"Trying to remove dependency {chunkB}, but chunk {chunkA} does not exist.");
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