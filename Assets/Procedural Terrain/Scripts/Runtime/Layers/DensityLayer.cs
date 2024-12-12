using Rafasixteen.Runtime.ChunkLab;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public class DensityLayer : Layer<DensityLayer, DensityLayerChunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {

        }
    }

    public class DensityLayerChunk : Chunk<DensityLayerChunk, DensityLayer>
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