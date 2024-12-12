using Rafasixteen.Runtime.ChunkLab;
using System;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public class TerrainLayer : Layer<TerrainLayer, TerrainLayerChunk>
    {
        public TerrainManager TerrainManager { get; private set; }

        protected override void OnStart()
        {
            if (!TryGetComponent(out TerrainManager terrainManager))
                throw new InvalidOperationException($"{typeof(TerrainManager).Name} component must be attached to {ChunkLabManager.name}");

            TerrainManager = terrainManager;
        }

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