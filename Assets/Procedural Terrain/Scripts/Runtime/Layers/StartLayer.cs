using Rafasixteen.Runtime.ChunkLab;
using Unity.Collections;
using Unity.Mathematics.Geometry;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    [LeafLayer]
    public class StartLayer : Layer<StartLayer, StartLayerChunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            MinMaxAABB bounds = chunkId.Bounds.GetExpanded(chunkId.Size * 2);
            TerrainLayer terrainLayer = LayerManager.GetLayer<TerrainLayer>();
            using NativeArray<ChunkId> dependencies = terrainLayer.GetChunkIdsWithinBounds(bounds, Allocator.Temp);
            ChunkDependencyManager.AddDependencies(chunkId, dependencies);
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