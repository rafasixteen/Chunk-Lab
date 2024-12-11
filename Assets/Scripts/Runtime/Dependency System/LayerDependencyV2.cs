using Unity.Mathematics;

namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class LayerDependencyV2 : ChunkLifecycle
    {
        public LayerBase Dependent { get; internal set; }

        public LayerBase Dependency { get; internal set; }

        private ChunkDependencyManager ChunkDependencyManager => Dependent.ChunkDependencyManager;

        protected void AddDependency(int3 dependentCoords, int3 dependencyCoords)
        {
            ChunkId dependent = CreateChunkId(Dependent, dependentCoords);
            ChunkId dependency = CreateChunkId(Dependency, dependencyCoords);
            ChunkDependencyManager.AddDependency(dependent, dependency);
        }

        protected void RemoveDependency(int3 dependentCoords, int3 dependencyCoords)
        {
            ChunkId dependent = CreateChunkId(Dependent, dependentCoords);
            ChunkId dependency = CreateChunkId(Dependency, dependencyCoords);
            ChunkDependencyManager.RemoveDependency(dependency, dependent);
        }

        private ChunkId CreateChunkId(LayerBase layer, int3 chunkCoords)
        {
            return new(layer.Id, chunkCoords, layer.Settings.ChunkSize);
        }
    }

    public class TestLayerDependency : LayerDependencyV2
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            AddDependency(chunkId.Coords, chunkId.Coords);
        }
    }
}