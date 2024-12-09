using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class ChunkDependencyManager
    {
        private NativeParallelMultiHashMap<ChunkId, ChunkId> _dependencies;
        private NativeParallelMultiHashMap<ChunkId, ChunkId> _dependents;

        public ChunkDependencyManager()
        {
            _dependencies = new(0, Allocator.Persistent);
            _dependents = new(0, Allocator.Persistent);
        }

        public void AddDependency(ChunkId dependent, ChunkId dependency)
        {
            _dependencies.Add(dependency, dependent);
            _dependents.Add(dependent, dependency);
        }

        public void RemoveDependency(ChunkId dependent, ChunkId dependency)
        {
            _dependencies.Remove(dependency, dependent);
            _dependents.Remove(dependent, dependency);
        }

        public NativeArray<ChunkId> GetDependencies(ChunkId chunkId, Allocator allocator)
        {
            if (!_dependencies.ContainsKey(chunkId))
                return new NativeArray<ChunkId>(0, allocator);

            int count = _dependencies.CountValuesForKey(chunkId);
            NativeArray<ChunkId> dependencies = new(count, allocator);

            int index = 0;
            if (_dependencies.TryGetFirstValue(chunkId, out ChunkId dependencyId, out NativeParallelMultiHashMapIterator<ChunkId> iterator))
            {
                do
                {
                    dependencies[index++] = dependencyId;
                }
                while (_dependencies.TryGetNextValue(out dependencyId, ref iterator));
            }

            return dependencies;
        }

        public NativeArray<ChunkId> GetDependents(ChunkId chunkId, Allocator allocator)
        {
            if (!_dependents.ContainsKey(chunkId))
                return new NativeArray<ChunkId>(0, allocator);

            int count = _dependents.CountValuesForKey(chunkId);
            NativeArray<ChunkId> dependents = new(count, allocator);

            int index = 0;
            if (_dependents.TryGetFirstValue(chunkId, out ChunkId dependencyId, out NativeParallelMultiHashMapIterator<ChunkId> iterator))
            {
                do
                {
                    dependents[index++] = dependencyId;
                }
                while (_dependents.TryGetNextValue(out dependencyId, ref iterator));
            }

            return dependents;
        }

        public bool HasDependencies(ChunkId chunkId)
        {
            return _dependencies.ContainsKey(chunkId);
        }

        public bool HasDependents(ChunkId chunkId)
        {
            return _dependents.ContainsKey(chunkId);
        }
    }
}