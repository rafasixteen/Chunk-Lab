namespace Rafasixteen.Runtime.ChunkLab
{
    public enum EChunkState : byte
    {
        None,
        Loaded,
        Unloaded,
        Loading,
        Unloading,
        AwaitingLoading,
        AwaitingUnloading,
    }
}