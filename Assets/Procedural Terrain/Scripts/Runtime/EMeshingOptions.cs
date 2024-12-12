using System;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    [Flags]
    public enum EMeshingOptions : byte
    {
        None = 0,
        SmoothVertices = 1,
        UseNormalsFromJob = 2,
        GenerateSameLodSeams = 4,
        GenerateCrossLodSeams = 8,
    }
}