using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public struct NeighborChunks : IDisposable
    {
        #region FIELDS

        [NativeDisableContainerSafetyRestriction]
        private NativeKeyValueArrays<ChunkData, int3> _sameLevelChunks;

        [NativeDisableContainerSafetyRestriction]
        private NativeKeyValueArrays<ChunkData, int3> _lowerLevelChunks;

        #endregion

        #region CONSTRUCTORS

        public NeighborChunks(int length, Allocator allocator)
        {
            _sameLevelChunks = new(length, allocator, NativeArrayOptions.ClearMemory);
            _lowerLevelChunks = new(length, allocator, NativeArrayOptions.ClearMemory);
        }

        #endregion

        #region PROPERTIES

        public NativeArray<ChunkData>.ReadOnly SameLevelChunks => _sameLevelChunks.Keys.AsReadOnly();
        public NativeArray<ChunkData>.ReadOnly LowerLevelChunks => _lowerLevelChunks.Keys.AsReadOnly();

        public NativeArray<int3>.ReadOnly SameLevelChunksOffsets => _sameLevelChunks.Values.AsReadOnly();
        public NativeArray<int3>.ReadOnly LowerLevelChunksOffsets => _lowerLevelChunks.Values.AsReadOnly();

        #endregion

        #region METHODS

        public void AddSameLevelChunk(int index, ChunkData chunk, int3 offset)
        {
            _sameLevelChunks.Keys[index] = chunk;
            _sameLevelChunks.Values[index] = offset;
        }

        public void AddLowerLevelChunk(int index, ChunkData chunk, int3 offset)
        {
            _lowerLevelChunks.Keys[index] = chunk;
            _lowerLevelChunks.Values[index] = offset;
        }

        public ChunkData GetSameLevelChunk(int index)
        {
            return _sameLevelChunks.Keys[index];
        }

        public ChunkData GetLowerLevelChunk(int index)
        {
            return _lowerLevelChunks.Keys[index];
        }

        public ChunkData GetSameLevelChunk(int3 offset)
        {
            if (!ContainsSameLevelChunk(offset))
                return default;

            return GetSameLevelChunk(_sameLevelChunks.Values.IndexOf(offset));
        }

        public ChunkData GetLowerLevelChunk(int3 offset)
        {
            if (!ContainsLowerLevelChunk(offset))
                return default;

            return GetSameLevelChunk(_lowerLevelChunks.Values.IndexOf(offset));
        }

        public readonly bool ContainsSameLevelChunk(int3 offset)
        {
            return _sameLevelChunks.Values.Contains(offset);
        }

        public readonly bool ContainsLowerLevelChunk(int3 offset)
        {
            return _lowerLevelChunks.Values.Contains(offset);
        }

        public readonly bool ContainsSameLevelChunk(ChunkData chunk)
        {
            return _sameLevelChunks.Keys.Contains(chunk);
        }

        public readonly bool ContainsLowerLevelChunk(ChunkData chunk)
        {
            return _lowerLevelChunks.Keys.Contains(chunk);
        }

        public void Dispose()
        {
            _sameLevelChunks.Dispose();
            _lowerLevelChunks.Dispose();
        }

        #endregion
    }
}