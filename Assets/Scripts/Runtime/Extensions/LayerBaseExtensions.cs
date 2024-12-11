using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;

namespace Rafasixteen.Runtime.ChunkLab
{
    [BurstCompile]
    public static class LayerBaseExtensions
    {
        public static NativeArray<ChunkId> GetChunkIdsWithinBounds(this LayerBase layer, MinMaxAABB bounds, Allocator allocator)
        {
            MinMaxAABB dividedBounds = bounds.GetDivided(layer.Settings.ChunkSize);
            int3 min = (int3)dividedBounds.Min;
            int3 max = (int3)dividedBounds.Max;

            NativeList<ChunkId> chunkIds = new(allocator);

            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    for (int z = min.z; z < max.z; z++)
                    {
                        int3 chunkCoords = new(x, y, z);
                        ChunkId chunkId = new(layer.Id, chunkCoords, layer.Settings.ChunkSize);
                        chunkIds.Add(chunkId);
                    }
                }
            }

            return chunkIds.AsArray();
        }

        public static NativeArray<ChunkId> GetChunkIdsWithinRadius(this LayerBase layer, float3 center, float radius, Allocator allocator)
        {
            MinMaxAABB bounds = new(center - radius, center + radius);

            using NativeArray<ChunkId> potentialChunks = layer.GetChunkIdsWithinBounds(bounds, Allocator.Temp);

            NativeList<ChunkId> chunkIds = new(allocator);

            for (int i = 0; i < potentialChunks.Length; i++)
            {
                ChunkId chunkId = potentialChunks[i];

                if (math.distance(chunkId.Position, center) <= radius)
                    chunkIds.Add(chunkId);
            }

            return chunkIds.AsArray();
        }
    }
}