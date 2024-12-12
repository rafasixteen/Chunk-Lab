using Rafasixteen.Runtime.ChunkLab;
using System;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public class DensityLayer : Layer<DensityLayer, DensityLayerChunk>
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

    public class DensityLayerChunk : Chunk<DensityLayerChunk, DensityLayer>
    {
        private NativeArray<float> _densities;

        protected override void StartLoading()
        {
            _densities = new(Layer.TerrainManager.Volume, Allocator.Persistent);
            Layer.TerrainManager.DispatchVoxelGenerator(Id);
            Layer.TerrainManager.RequestDensityData(_densities, OnReadbackComplete);
        }

        protected override void StartUnloading()
        {
            _densities.Dispose();
            FinishUnloading();
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            FinishLoading();
        }
    }
}