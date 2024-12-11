using System;

namespace Rafasixteen.Runtime.ChunkLab
{
    [Flags]
    public enum EVisualizationSettings : byte
    {
        None = 0,
        ShowChunkBounds = 1,
        HighlightChunkBoundsByState = 2,
        Custom = 4,
        Default = ShowChunkBounds | HighlightChunkBoundsByState,
    }
}