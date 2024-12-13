float3 Scale(uint3 voxelIndex, uint3 chunkSize, uint3 chunkResolution)
{
    float3 gridStep = (float3) chunkSize / (float3) (chunkResolution - 1);
    return (float3) voxelIndex * gridStep;
}

uint FlattenIndex(uint3 resolution,int x, int y, int z)
{
    return z * resolution.x * resolution.y + y * resolution.x + x;
}

uint FlattenIndex(uint3 resolution, uint3 index)
{
    return FlattenIndex(resolution, index.x, index.y, index.z);
}