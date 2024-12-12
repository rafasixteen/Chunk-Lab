using Rafasixteen.Runtime.ChunkLab;
using Unity.Collections;
using Unity.Mathematics.Geometry;

namespace Rafasixteen
{
    [LeafLayer]
    public class LayerA : Layer<LayerA, LayerA.Chunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            LayerB layer = LayerManager.GetLayer<LayerB>();

            MinMaxAABB bounds = chunkId.Bounds.GetExpanded(chunkId.Size);
            using NativeArray<ChunkId> dependencies = layer.GetChunkIdsWithinBounds(bounds, Allocator.Temp);

            ChunkId dependencyId = new(layer.Id, chunkId.Coords, layer.Settings.ChunkSize);
            ChunkDependencyManager.AddDependencies(chunkId, dependencies);
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
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            LayerC layer = LayerManager.GetLayer<LayerC>();

            MinMaxAABB bounds = chunkId.Bounds.GetExpanded(chunkId.Size);
            using NativeArray<ChunkId> dependencies = layer.GetChunkIdsWithinBounds(bounds, Allocator.Temp);

            ChunkId dependencyId = new(layer.Id, chunkId.Coords, layer.Settings.ChunkSize);
            ChunkDependencyManager.AddDependencies(chunkId, dependencies);
        }

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

    public class LayerC : Layer<LayerC, LayerC.Chunk>
    {
        protected override void OnChunkAwaitingLoading(ChunkId chunkId)
        {
            LayerD layer = LayerManager.GetLayer<LayerD>();

            MinMaxAABB bounds = chunkId.Bounds.GetExpanded(chunkId.Size);
            using NativeArray<ChunkId> dependencies = layer.GetChunkIdsWithinBounds(bounds, Allocator.Temp);

            ChunkId dependencyId = new(layer.Id, chunkId.Coords, layer.Settings.ChunkSize);
            ChunkDependencyManager.AddDependencies(chunkId, dependencies);
        }

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

    public class LayerD : Layer<LayerD, LayerD.Chunk>
    {
        public class Chunk : Chunk<Chunk, LayerD>
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