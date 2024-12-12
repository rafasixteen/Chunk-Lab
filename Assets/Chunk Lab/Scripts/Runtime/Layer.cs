using Unity.Mathematics;

namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class Layer<TLayer, TChunk> : LayerBase
        where TLayer : Layer<TLayer, TChunk>, new()
        where TChunk : Chunk<TChunk, TLayer>, new()
    {
        public bool TryGetChunk(ChunkId chunkId, out TChunk chunk)
        {
            TryGetChunk(chunkId, out ChunkBase chunkBase);
            chunk = chunkBase as TChunk;
            return chunk != null;
        }

        public bool TryGetChunk(int3 chunkCoords, out TChunk chunk)
        {
            ChunkId chunkId = new(Id, chunkCoords, Settings.ChunkSize);
            return TryGetChunk(chunkId, out chunk);
        }

        public new TChunk GetChunk(ChunkId chunkId)
        {
            return base.GetChunk(chunkId) as TChunk;
        }

        public new TChunk GetChunk(int3 chunkCoords)
        {
            ChunkId chunkId = new(Id, chunkCoords, Settings.ChunkSize);
            return GetChunk(chunkId);
        }

        public bool TryGetComponent<T>(out T component)
        {
            return ChunkLabManager.TryGetComponent(out component);
        }

        public T GetComponent<T>()
        {
            return ChunkLabManager.GetComponent<T>();
        }

        private protected sealed override ChunkBase InstantiateChunk()
        {
            ChunkBase chunk = new TChunk();
            chunk.Name = typeof(TChunk).Name;
            chunk.Layer = this;
            return chunk;
        }
    }
}