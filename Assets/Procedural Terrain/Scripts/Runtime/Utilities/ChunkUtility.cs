using System;
using Unity.Burst;
using Unity.Mathematics;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    [BurstCompile]
    public static class ChunkUtility
    {
        public static int3 LocalToWorldIndex(int3 localIndex, int3 chunkCoords, int3 chunkResolution, int3 scaleFactor)
        {
            // Calculate the offset in world space for the chunk based on its coords in chunk space.
            int3 chunkWorldOffset = chunkCoords * (chunkResolution - 1) * scaleFactor;

            // Convert local coordinates within the chunk to world coordinates by adding the chunk's world offset.
            int3 worldIndex = chunkWorldOffset + localIndex * scaleFactor;

            return worldIndex;
        }

        public static int3 WorldToLocalIndex(int3 worldIndex, int3 chunkCoords, int3 chunkResolution, int3 scaleFactor)
        {
            // Calculate the offset in world space for the chunk based on its coords in chunk space.
            int3 chunkWorldOffset = chunkCoords * (chunkResolution - 1) * scaleFactor;

            // Get the local index within the chunk by offsetting world index by the chunk's world start position.
            int3 localIndex = (worldIndex - chunkWorldOffset) / scaleFactor;

            // Wrap the local index to ensure it remains within the chunk's resolution bounds.
            localIndex = (localIndex + chunkResolution) % chunkResolution;

            return localIndex;
        }

        public static int3 GetChunkScaleFactor(int3 highestLodChunkSize, int3 chunkSize)
        {
            if (math.any(chunkSize % highestLodChunkSize != 0))
                throw new ArgumentException($"{nameof(highestLodChunkSize)} of {highestLodChunkSize} must be a factor of {chunkSize}.");

            return chunkSize / highestLodChunkSize;
        }

        public static int FlattenIndex(int3 index, int3 chunkResolution)
        {
            if (math.any(chunkResolution < 0))
                throw new ArgumentException($"{nameof(chunkResolution)} must be greater than or equal to zero.");

            return index.z * chunkResolution.x * chunkResolution.y + index.y * chunkResolution.x + index.x;
        }

        public static float3 GetVerticeScaleFactor(int3 chunkSize, int3 chunkResolution)
        {
            if (math.any(chunkSize < 0))
                throw new ArgumentException($"{nameof(chunkSize)} must be greater than or equal to zero.");

            if (math.any(chunkResolution < 0))
                throw new ArgumentException($"{nameof(chunkResolution)} must be greater than or equal to zero.");

            return (float3)chunkSize / (float3)(chunkResolution - 1);
        }
    }
}