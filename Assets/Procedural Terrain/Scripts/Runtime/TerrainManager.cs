using Rafasixteen.Runtime.ChunkLab;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    using ChunkPool = UnityEngine.Pool.ObjectPool<ChunkObject>;

    public class TerrainManager : MonoBehaviour
    {
        #region FIELDS

        private NativeArray<byte> _cubeEdgesTable;
        private NativeArray<ushort> _edgeTable;

        private ChunkPool _chunkObjectPool;
        private Dictionary<ChunkId, ChunkObject> _chunkObjects;

        [SerializeField] private ComputeShader _voxelGenerator;
        [SerializeField] private RenderTexture _readbackTexture;
        [SerializeField] private ChunkObject _chunkPrefab;
        [SerializeField] private EMeshingOptions _meshingOptions;
        [SerializeField] private float _isoLevel;
        [SerializeField] private bool _drawGizmos;

        #endregion

        #region PROPERTIES

        public int3 Resolution => new(_readbackTexture.width, _readbackTexture.height, _readbackTexture.volumeDepth);

        public int Volume => Resolution.x * Resolution.y * Resolution.z;

        public EMeshingOptions MeshingOptions => _meshingOptions;

        public float IsoLevel => _isoLevel;

        public NativeArray<byte>.ReadOnly CubeEdgesTable => _cubeEdgesTable.AsReadOnly();

        public NativeArray<ushort>.ReadOnly EdgeTable => _edgeTable.AsReadOnly();

        #endregion

        #region METHODS

        #region LIFECYCLE

        private void Start()
        {
            InitializeVoxelGenerator();

            _cubeEdgesTable = AllocateCubeEdgesTable(Allocator.Persistent);
            _edgeTable = AllocateEdgeTable(_cubeEdgesTable, Allocator.Persistent);
            _chunkObjectPool = new(() => Instantiate(_chunkPrefab));
            _chunkObjects = new();
        }

        private void OnDestroy()
        {
            AsyncGPUReadback.WaitAllRequests();

            _cubeEdgesTable.Dispose();
            _edgeTable.Dispose();
            _chunkObjectPool.Dispose();
        }

        #endregion

        public void DispatchVoxelGenerator(ChunkId chunkId)
        {
            int kernelIndex = _voxelGenerator.FindKernel("Generate");
            _voxelGenerator.SetInt3("chunkCoords", chunkId.Coords);
            _voxelGenerator.SetInt3("chunkSize", chunkId.Size);
            _voxelGenerator.Dispatch(kernelIndex, (uint3)Resolution);
        }

        public void RequestDensityData(NativeArray<float> output, Action<AsyncGPUReadbackRequest> onComplete)
        {
            AsyncGPUReadback.RequestIntoNativeArray(ref output, _readbackTexture, 0, onComplete);
        }

        public ChunkObject GetChunkObject(ChunkId chunkId)
        {
            if (_chunkObjects.TryGetValue(chunkId, out ChunkObject chunkObject))
                return chunkObject;

            chunkObject = _chunkObjectPool.Get();
            chunkObject.Initialize(chunkId, transform);
            _chunkObjects.Add(chunkId, chunkObject);
            return chunkObject;
        }

        public void ReleaseChunkObject(ChunkId chunkId)
        {
            if (_chunkObjects.TryGetValue(chunkId, out ChunkObject chunkObject))
            {
                _chunkObjects.Remove(chunkId);
                _chunkObjectPool.Release(chunkObject);
                chunkObject.ResetState();
            }
        }

        private void InitializeVoxelGenerator()
        {
            int kernelIndex = _voxelGenerator.FindKernel("Generate");
            _voxelGenerator.SetTexture(kernelIndex, "densities", _readbackTexture);
            _voxelGenerator.SetInt3("chunkResolution", Resolution);
        }

        private static NativeArray<byte> AllocateCubeEdgesTable(Allocator allocator)
        {
            NativeArray<byte> cubeEdges = new(24, allocator);

            int k = 0;
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 1; j <= 4; j <<= 1)
                {
                    int p = i ^ j;
                    if (i <= p)
                    {
                        cubeEdges[k++] = (byte)i;
                        cubeEdges[k++] = (byte)p;
                    }
                }
            }

            return cubeEdges;
        }

        private static NativeArray<ushort> AllocateEdgeTable(NativeArray<byte> cubeEdges, Allocator allocator)
        {
            NativeArray<ushort> edgeTable = new(256, allocator);

            for (int i = 0; i < 256; ++i)
            {
                ushort em = 0;
                for (int j = 0; j < 24; j += 2)
                {
                    bool a = Convert.ToBoolean(i & (1 << cubeEdges[j]));
                    bool b = Convert.ToBoolean(i & (1 << cubeEdges[j + 1]));
                    em |= a != b ? (ushort)(1 << (j >> 1)) : (ushort)0;
                }
                edgeTable[i] = em;
            }

            return edgeTable;
        }

        #endregion
    }
}