using Rafasixteen.Runtime.ChunkLab;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public class TerrainLayer : Layer<TerrainLayer, TerrainLayerChunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {

        }
    }

    public class TerrainLayerChunk : Chunk<TerrainLayerChunk, TerrainLayer>
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