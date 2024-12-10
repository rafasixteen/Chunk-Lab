using Rafasixteen.Runtime.ChunkLab;

namespace Rafasixteen
{
    [LeafLayer]
    public class LayerA : Layer<LayerA, LayerA.Chunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            LayerB layer = LayerManager.GetLayer<LayerB>();

            ChunkId dependencyId = new(layer.Id, chunkId.Coords, layer.Settings.ChunkSize);

            ChunkDependencyManager.AddDependency(dependencyId, chunkId);
        }

        public class Chunk : Chunk<Chunk, LayerA>
        {
            protected override void StartLoading()
            {
                FinishLoading();
            }

            protected override void StartUnloading()
            {
                FinishUnloading();
            }
        }
    }

    public class LayerB : Layer<LayerB, LayerB.Chunk>
    {
        public class Chunk : Chunk<Chunk, LayerB>
        {
            protected override void StartLoading()
            {
                FinishLoading();
            }

            protected override void StartUnloading()
            {
                FinishUnloading();
            }
        }
    }

    [LeafLayer]
    public class LayerC : Layer<LayerC, LayerC.Chunk>
    {
        public class Chunk : Chunk<Chunk, LayerC>
        {
            protected override void StartLoading()
            {
                FinishLoading();
            }

            protected override void StartUnloading()
            {
                FinishUnloading();
            }
        }
    }
}