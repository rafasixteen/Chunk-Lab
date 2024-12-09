using System;
using Unity.Mathematics;

namespace Rafasixteen.Runtime.ChunkLab
{
    public readonly struct ChunkId : IEquatable<ChunkId>
    {
        public ChunkId(LayerId layerId, int3 coords, int3 size)
        {
            LayerId = layerId;
            Coords = coords;
            Size = size;
        }

        public LayerId LayerId { get; }

        public int3 Coords { get; }

        public int3 Size { get; }

        public bool Equals(ChunkId other)
        {
            return LayerId.Equals(other.LayerId) && Coords.Equals(other.Coords) && Size.Equals(other.Size);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ChunkId other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return LayerId.GetHashCode() ^ Coords.GetHashCode() ^ Size.GetHashCode();
        }

        public override readonly string ToString()
        {
            return $"Layer: {LayerId} | Coords: {Coords} | Size: {Size}";
        }

        public static bool operator ==(ChunkId left, ChunkId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkId left, ChunkId right)
        {
            return !left.Equals(right);
        }
    }
}