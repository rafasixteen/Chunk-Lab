namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class Layer<TLayer, TChunk> : LayerBase
        where TLayer : Layer<TLayer, TChunk>, new()
        where TChunk : Chunk<TChunk, TLayer>, new()
    {
        private protected sealed override ChunkBase InstantiateChunk()
        {
            return new TChunk()
            {
                Name = typeof(TChunk).Name,
                Layer = this,
            };
        }
    }
}