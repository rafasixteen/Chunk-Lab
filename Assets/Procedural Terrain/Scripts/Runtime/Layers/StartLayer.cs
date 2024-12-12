using Rafasixteen.Runtime.ChunkLab;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public class StartLayer : Layer<StartLayer, StartLayerChunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {

        }
    }

    public class StartLayerChunk : Chunk<StartLayerChunk, StartLayer>
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