using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    [BurstCompile]
    public unsafe struct SurfaceNetsMesher : IJob, IDisposable
    {
        #region FIELDS

        private ChunkData _chunkData;
        private NeighborChunks _neighborChunks;

        private NativeArray<byte>.ReadOnly _cubeEdgesTable;
        private NativeArray<ushort>.ReadOnly _edgeTable;

        private int3 _resolution;
        private float _isoLevel;
        private EMeshingOptions _meshingOptions;

        private NativeList<float3> _vertices;
        private NativeList<float3> _normals;
        private NativeList<uint> _indices;

        #endregion

        #region CONSTRUCTORS

        public SurfaceNetsMesher(ChunkData chunk, NeighborChunks neighbors, TerrainManager manager)
        {
            _chunkData = chunk;
            _neighborChunks = neighbors;

            _cubeEdgesTable = manager.CubeEdgesTable;
            _edgeTable = manager.EdgeTable;

            _resolution = manager.Resolution;
            _isoLevel = manager.IsoLevel;
            _meshingOptions = manager.MeshingOptions;

            _vertices = new(Allocator.Persistent);
            _normals = new(Allocator.Persistent);
            _indices = new(Allocator.Persistent);
        }

        #endregion

        #region PROPERTIES

        private static VertexAttributeDescriptor[] VertexLayout => new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, stream: 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, stream: 1)
        };

        private readonly bool SmoothVertices => (_meshingOptions & EMeshingOptions.SmoothVertices) != 0;

        private readonly bool CalculateNormals => (_meshingOptions & EMeshingOptions.UseNormalsFromJob) != 0;

        private readonly bool GenerateSameLodSeams => (_meshingOptions & EMeshingOptions.GenerateSameLodSeams) != 0;

        private readonly bool GenerateCrossLodSeams => (_meshingOptions & EMeshingOptions.GenerateCrossLodSeams) != 0;

        #endregion

        #region METHODS

        public readonly bool WillMeshBeGenerated()
        {
            return _vertices.Length >= 3 && _indices.Length >= 6;
        }

        public void ProcessMesh(Mesh mesh)
        {
            int vertexCount = _vertices.Length;
            int indexCount = _indices.Length;

            MeshUpdateFlags flags = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds;
            const int k_vertexPositionStream = 0;
            const int k_vertexNormalStream = 1;

            mesh.SetVertexBufferParams(vertexCount, VertexLayout);
            mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

            mesh.SetVertexBufferData(_vertices.AsArray(), 0, 0, vertexCount, k_vertexPositionStream, flags);
            mesh.SetIndexBufferData(_indices.AsArray(), 0, 0, indexCount, flags);

            mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexCount), flags);
            mesh.RecalculateBounds();

            if (CalculateNormals)
                mesh.SetVertexBufferData(_normals.AsArray(), 0, 0, vertexCount, k_vertexNormalStream, flags);
            else
                mesh.RecalculateNormals();
        }

        public void Dispose()
        {
            _vertices.Dispose();
            _normals.Dispose();
            _indices.Dispose();
        }

        void IJob.Execute()
        {
            int3 size = GetLoopSize();

            int3 rowOffsets = new(1, size.x + 1, (size.x + 1) * (size.y + 1));
            int bufferToggle = 1;
            int bufferIndex = -1;

            float* cell = stackalloc float[8];
            int bufferSize = rowOffsets[2] * 2;
            NativeArray<uint> buffer = new(bufferSize, Allocator.Temp);

            for (int z = 0; z < size.z - 1; z++)
            {
                bufferToggle ^= 1;
                rowOffsets[2] = -rowOffsets[2];
                bufferIndex = 1 + (size.x + 1) * (1 + bufferToggle * (size.y + 1));

                for (int y = 0; y < size.y - 1; y++, bufferIndex += 2)
                {
                    for (int x = 0; x < size.x - 1; x++, bufferIndex++)
                    {
                        int3 localIndex = new(x, y, z);

                        if (ShouldSkipCorner(localIndex))
                            continue;

                        int3 offset = GetMaxBoundaryOffset(localIndex);
                        ChunkData chunkData = offset.Equals(0) ? _chunkData : _neighborChunks.GetLowerLevelChunk(offset);
                        int3 chunkLocalIndex = localIndex - offset * (_resolution - 1);
                        SampleCell(chunkData.Densities, cell, chunkLocalIndex);

                        int cornerMask = GetCornerMask(cell, _isoLevel);

                        if (cornerMask == 0 || cornerMask == 255)
                            continue;

                        int edgeMask = _edgeTable[cornerMask];

                        float3 vertice = float3.zero;

                        if (SmoothVertices)
                            vertice = GetVerticePositionWithinCell(cell, edgeMask);

                        vertice += localIndex;
                        vertice *= ChunkUtility.GetVerticeScaleFactor(chunkData.Size, _resolution);

                        buffer[bufferIndex] = (uint)_vertices.Length;
                        _vertices.Add(vertice);

                        if (CalculateNormals)
                            _normals.Add(GetNormalFromCell(cell));

                        if (ShouldAddFaces(localIndex))
                            AddFaces(localIndex, cornerMask, edgeMask, rowOffsets, bufferIndex, buffer);
                    }
                }
            }
        }

        private readonly void AddFaces(int3 localIndex, int cornerMask, int edgeMask, int3 rowOffsets, int bufferIndex, NativeArray<uint> buffer)
        {
            (int, int) GetOrthogonalAxes(int mainAxis)
            {
                int iu = (mainAxis + 1) % 3;
                int iv = (mainAxis + 2) % 3;
                return (iu, iv);
            }

            (int, int) GetAdjacentEdges(int iu, int iv)
            {
                int du = rowOffsets[iu];
                int dv = rowOffsets[iv];
                return (du, dv);
            }

            bool IsOnBoundary(int3 localIndex, int iu, int iv)
            {
                return localIndex[iu] == 0 || localIndex[iv] == 0;
            }

            void AddTriangle(NativeList<uint> indices, uint a, uint b, uint c)
            {
                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
            }

            bool flipTriangle = (cornerMask & 1) != 0;

            for (int i = 0; i < 3; i++)
            {
                if (!HasEdgeCrossing(edgeMask, i))
                    continue;

                (int iu, int iv) = GetOrthogonalAxes(i);

                if (IsOnBoundary(localIndex, iu, iv))
                    continue;

                (int du, int dv) = GetAdjacentEdges(iu, iv);

                if (flipTriangle)
                {
                    AddTriangle(_indices, buffer[bufferIndex], buffer[bufferIndex - du - dv], buffer[bufferIndex - du]);
                    AddTriangle(_indices, buffer[bufferIndex], buffer[bufferIndex - dv], buffer[bufferIndex - du - dv]);
                }
                else
                {
                    AddTriangle(_indices, buffer[bufferIndex], buffer[bufferIndex - du - dv], buffer[bufferIndex - dv]);
                    AddTriangle(_indices, buffer[bufferIndex], buffer[bufferIndex - du], buffer[bufferIndex - du - dv]);
                }
            }
        }

        private readonly void SampleCell(NativeArray<float>.ReadOnly densities, float* cell, int3 localIndex)
        {
            float GetDensity(int3 index, int3 resolution)
            {
                return densities[ChunkUtility.FlattenIndex(index, resolution)];
            }

            cell[0] = GetDensity(localIndex + new int3(0, 0, 0), _resolution);
            cell[1] = GetDensity(localIndex + new int3(1, 0, 0), _resolution);
            cell[2] = GetDensity(localIndex + new int3(0, 1, 0), _resolution);
            cell[3] = GetDensity(localIndex + new int3(1, 1, 0), _resolution);
            cell[4] = GetDensity(localIndex + new int3(0, 0, 1), _resolution);
            cell[5] = GetDensity(localIndex + new int3(1, 0, 1), _resolution);
            cell[6] = GetDensity(localIndex + new int3(0, 1, 1), _resolution);
            cell[7] = GetDensity(localIndex + new int3(1, 1, 1), _resolution);
        }

        private readonly bool ShouldAddFaces(int3 localIndex)
        {
            // If meshing options do not include same seam generation, add faces by default.
            if (!GenerateSameLodSeams)
                return true;

            // Check for the presence of neighboring chunks on each axis if at the boundary.
            bool needsFaceX = localIndex.x == 0 && !_neighborChunks.ContainsSameLevelChunk(new int3(-1, 0, 0));
            bool needsFaceY = localIndex.y == 0 && !_neighborChunks.ContainsSameLevelChunk(new int3(0, -1, 0));
            bool needsFaceZ = localIndex.z == 0 && !_neighborChunks.ContainsSameLevelChunk(new int3(0, 0, -1));

            // Add faces if any boundary on the negative side lacks a neighboring chunk.
            if (needsFaceX || needsFaceY || needsFaceZ)
                return true;

            // If not on any boundary needing faces, add faces only if not fully inside the chunk.
            return math.all(localIndex > 0);
        }

        private readonly bool ShouldSkipCorner(int3 localIndex)
        {
            int3 offset = GetMaxBoundaryOffset(localIndex);

            // If the offset is (0, 0, 0), we're not at the boundary, so no need to skip.
            if (math.all(offset == 0))
                return false;

            // We only check for the chunk at the calculated offset because if the neighbor chunks
            // exist (as determined by the other parts of the loop), the check for the individual 
            // axis offsets is redundant. The neighbor chunks will influence the behavior of the 
            // loops in such a way that we don't need additional checks for individual axes.
            return !_neighborChunks.ContainsSameLevelChunk(offset);
        }

        private readonly bool IsInsideBoundary(int3 localIndex)
        {
            return math.all(localIndex >= 0) && math.all(localIndex <= _resolution - 2);
        }

        private readonly int3 GetMaxBoundaryOffset(int3 localIndex)
        {
            return new(
                localIndex.x == _resolution.x - 1 ? 1 : 0,
                localIndex.y == _resolution.y - 1 ? 1 : 0,
                localIndex.z == _resolution.z - 1 ? 1 : 0
            );
        }

        private readonly int3 GetLoopSize()
        {
            int3 size = _resolution;

            if (GenerateSameLodSeams)
            {
                size.x += _neighborChunks.ContainsSameLevelChunk(new int3(1, 0, 0)) ? 1 : 0;
                size.y += _neighborChunks.ContainsSameLevelChunk(new int3(0, 1, 0)) ? 1 : 0;
                size.z += _neighborChunks.ContainsSameLevelChunk(new int3(0, 0, 1)) ? 1 : 0;
            }

            return size;
        }

        private readonly float3 GetVerticePositionWithinCell(float* cell, int edgeMask)
        {
            float3 accumulatedPosition = float3.zero;
            int intersectingEdges = 0;

            for (int i = 0; i < 12; ++i)
            {
                if (!HasEdgeCrossing(edgeMask, i))
                    continue;

                int cornerA = _cubeEdgesTable[i << 1];
                int cornerB = _cubeEdgesTable[(i << 1) + 1];

                float densityA = cell[cornerA];
                float densityB = cell[cornerB];

                float t = densityA / (densityA - densityB);

                float3 positionA = new(cornerA & 1, (cornerA >> 1) & 1, (cornerA >> 2) & 1);
                float3 positionB = new(cornerB & 1, (cornerB >> 1) & 1, (cornerB >> 2) & 1);

                float3 intersectionPoint = math.lerp(positionA, positionB, t);

                accumulatedPosition += intersectionPoint;
                intersectingEdges++;
            }

            if (intersectingEdges == 0)
                return float3.zero;

            return accumulatedPosition / intersectingEdges;
        }

        private readonly float3 GetNormalFromCell(float* cell)
        {
            float dx = cell[1] - cell[0] + (cell[3] - cell[2]) + (cell[5] - cell[4]) + (cell[7] - cell[6]);
            float dy = cell[2] - cell[0] + (cell[3] - cell[1]) + (cell[6] - cell[4]) + (cell[7] - cell[5]);
            float dz = cell[4] - cell[0] + (cell[5] - cell[1]) + (cell[6] - cell[2]) + (cell[7] - cell[3]);
            float3 normal = new(dx, dy, dz);
            return math.normalize(normal);
        }

        private static byte GetCornerMask(float* cell, float isoLevel)
        {
            byte mask = 0;

            for (int i = 0; i < 8; i++)
            {
                if (cell[i] > isoLevel)
                    mask |= (byte)(1 << i);
            }

            return mask;
        }

        private static bool HasEdgeCrossing(int edgeMask, int axis)
        {
            return (edgeMask & (1 << axis)) != 0;
        }

        #endregion
    }
}