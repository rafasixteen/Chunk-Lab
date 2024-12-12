namespace Rafasixteen.Runtime.ChunkLab
{
    public abstract class Chunk<TChunk, TLayer> : ChunkBase
       where TChunk : Chunk<TChunk, TLayer>, new()
       where TLayer : Layer<TLayer, TChunk>, new()
    {
        public new TLayer Layer => base.Layer as TLayer;
    }
}