using Rafasixteen.JobManager;
using Rafasixteen.Runtime.ChunkLab;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public class TerrainLayer : Layer<TerrainLayer, TerrainLayerChunk>
    {
        public TerrainManager TerrainManager { get; private set; }

        public NativeArray<float>.ReadOnly GetDensities(int3 chunkCoords)
        {
            DensityLayer densityLayer = LayerManager.GetLayer<DensityLayer>();
            DensityLayerChunk chunk = densityLayer.GetChunk(chunkCoords);
            return chunk.Densities;
        }

        protected override void OnStart()
        {
            if (!TryGetComponent(out TerrainManager terrainManager))
                throw new InvalidOperationException($"{typeof(TerrainManager).Name} component must be attached to {ChunkLabManager.name}");

            TerrainManager = terrainManager;
        }

        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            DensityLayer densityLayer = LayerManager.GetLayer<DensityLayer>();
            ChunkId densityChunkId = new(densityLayer.Id, chunkId.Coords, densityLayer.Settings.ChunkSize);
            ChunkDependencyManager.AddDependency(chunkId, densityChunkId);
        }
    }

    public class TerrainLayerChunk : Chunk<TerrainLayerChunk, TerrainLayer>
    {
        protected override void StartLoading()
        {
            Remesh();
        }

        protected override void StartUnloading()
        {
            Layer.TerrainManager.ReleaseChunkObject(Id);

            FinishUnloading();
        }

        private void Remesh()
        {
            NativeArray<float>.ReadOnly densities = Layer.GetDensities(Coords);
            ChunkData chunkData = new(densities, Id);
            SurfaceNetsMesher mesher = new(chunkData, default, Layer.TerrainManager);

            const bool k_complete = true;

            if (k_complete)
            {
                mesher.Schedule().Complete();

                if (mesher.WillMeshBeGenerated())
                {
                    ChunkObject chunkObject = Layer.TerrainManager.GetChunkObject(Id);
                    mesher.ProcessMesh(chunkObject.Mesh);
                    chunkObject.SetMesh(chunkObject.Mesh);
                }
                else
                {
                    Layer.TerrainManager.ReleaseChunkObject(Id);
                }

                mesher.Dispose();
                OnChunkMeshed();

            }
            else
            {
                mesher.Schedule().SetCallback(() =>
                {
                    if (mesher.WillMeshBeGenerated())
                    {
                        ChunkObject chunkObject = Layer.TerrainManager.GetChunkObject(Id);
                        mesher.ProcessMesh(chunkObject.Mesh);
                        chunkObject.SetMesh(chunkObject.Mesh);
                    }
                    else
                    {
                        Layer.TerrainManager.ReleaseChunkObject(Id);
                    }

                    mesher.Dispose();
                    OnChunkMeshed();
                });
            }
        }
    
        private void OnChunkMeshed()
        {
            FinishLoading();
        }
    }
}