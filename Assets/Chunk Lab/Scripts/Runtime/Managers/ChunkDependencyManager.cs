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
        /// <param name="dependent">The chunk that depends on <paramref name="dependency"/>.</param>
        /// <param name="dependency">The chunk that <paramref name="dependent"/> depends on.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add a dependency where both <paramref name="dependent"/> 
        /// and <paramref name="dependency"/> belong to the same layer.
        /// </exception>
        /// <remarks>
        /// This method establishes a two-way relationship between the chunks:<br/>
        /// - <paramref name="dependency"/> will record <paramref name="dependent"/> as one of its dependents.<br/>
        /// - <paramref name="dependent"/> will record <paramref name="dependency"/> as one of its dependencies.<br/>
        /// 
        /// This relationship ensures that:<br/>
        /// 1. <paramref name="dependency"/> cannot be unloaded while it has active dependents like <paramref name="dependent"/>.<br/>
        /// 2. <paramref name="dependent"/> relies on <paramref name="dependency"/> being loaded.<br/>
        /// </remarks>
        public void AddDependency(ChunkId dependent, ChunkId dependency)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(AddDependency)))
            {
                if (dependent.LayerId == dependency.LayerId)
                    throw new InvalidOperationException($"Cannot add dependency between chunks {dependent} and {dependency} because they belong to the same layer.");

                if (!_dependencies.ContainsKey(dependent))
                    _dependencies[dependent] = new NativeHashSet<ChunkId>(0, Allocator.Persistent);

                if (!_dependents.ContainsKey(dependency))
                    _dependents[dependency] = new NativeHashSet<ChunkId>(0, Allocator.Persistent);

                if (_dependencies[dependent].Add(dependency))
                    ChunkLabLogger.Log($"Successfully added dependency {dependency} to chunk {dependent}.");
                else
                    ChunkLabLogger.LogWarning($"Duplicate dependency {dependency} added for chunk {dependent}.");

                if (_dependents[dependency].Add(dependent))
                    ChunkLabLogger.Log($"Successfully added dependent {dependent} to chunk {dependency}.");
                else
                    ChunkLabLogger.LogWarning($"Duplicate dependent {dependent} added for chunk {dependency}.");
            }
        }

        /// <summary>
        /// Removes a dependency relationship between two chunks.
        /// </summary>
        /// <param name="dependent">The chunk that depends on <paramref name="dependency"/>.</param>
        /// <param name="dependency">The chunk that <paramref name="dependent"/> depends on.</param>
        /// <remarks>
        /// This method breaks the two-way relationship previously established by <see cref="AddDependency"/>:<br/>
        /// - <paramref name="dependency"/> will no longer consider <paramref name="dependent"/> as one of its dependents.<br/>
        /// - <paramref name="dependent"/> will no longer consider <paramref name="dependency"/> as one of its dependencies.<br/>
        /// 
        /// This ensures that <paramref name="dependency"/> can now be safely unloaded if it has no other dependents.
        /// </remarks>
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

        /// <summary>
        /// Retrieves all dependencies of a specified chunk.
        /// </summary>
        /// <param name="chunkId">The <see cref="ChunkId"/> of the chunk whose dependencies are being retrieved.</param>
        /// <param name="allocator">The allocator to be used for the returned <see cref="NativeArray{ChunkId}"/>.</param>
        /// <returns>A <see cref="NativeArray{ChunkId}"/> containing the chunks that this chunk depends on. Returns an empty array if no dependencies are found.</returns>
        /// <remarks>
        /// Dependencies are chunks that must be loaded before this chunk can be loaded.
        /// </remarks>
        public NativeArray<ChunkId> GetDependencies(ChunkId chunkId, Allocator allocator)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(GetDependencies)))
            {
                if (!HasDependencies(chunkId))
                    return new NativeArray<ChunkId>(0, allocator);

                return _dependencies[chunkId].ToNativeArray(allocator);
            }
        }

        /// <summary>
        /// Retrieves all dependents of a specified chunk.
        /// </summary>
        /// <param name="chunkId">The <see cref="ChunkId"/> of the chunk whose dependents are being retrieved.</param>
        /// <param name="allocator">The allocator to be used for the returned <see cref="NativeArray{ChunkId}"/>.</param>
        /// <returns>A <see cref="NativeArray{ChunkId}"/> containing the chunks that depend on this chunk. Returns an empty array if no dependents are found.</returns>
        /// <remarks>
        /// Dependents are chunks that rely on this chunk to be loaded for them to be loaded.
        /// </remarks>
        public NativeArray<ChunkId> GetDependents(ChunkId chunkId, Allocator allocator)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(GetDependents)))
            {
                if (!HasDependents(chunkId))
                    return new NativeArray<ChunkId>(0, allocator);

                return _dependents[chunkId].ToNativeArray(allocator);
            }
        }

        /// <summary>
        /// Checks if the specified chunk has any dependencies.
        /// </summary>
        /// <param name="chunkId">The <see cref="ChunkId"/> of the chunk to check.</param>
        /// <returns><c>true</c> if the chunk has dependencies, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Dependencies are chunks that must be loaded before this chunk can be loaded.
        /// </remarks>
        public bool HasDependencies(ChunkId chunkId)
        {
            using (ProfilerUtility.StartSample(nameof(ChunkSchedulerManager), nameof(HasDependencies)))
            {
                return _dependencies.ContainsKey(chunkId) && !_dependencies[chunkId].IsEmpty;
            }
        }

        /// <summary>
        /// Checks if the specified chunk has any dependents.
        /// </summary>
        /// <param name="chunkId">The <see cref="ChunkId"/> of the chunk to check.</param>
        /// <returns><c>true</c> if the chunk has dependents, otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Dependents are chunks that rely on this chunk to be loaded for them to be loaded.
        /// </remarks>
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