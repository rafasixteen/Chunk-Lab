using Rafasixteen.Runtime.ChunkLab;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Rafasixteen.Runtime.ProceduralTerrain
{
    public readonly struct ChunkData : IEquatable<ChunkData>
    {
        public ChunkData(NativeArray<float>.ReadOnly densities, ChunkId chunkId)
        {
            Densities = densities;
            Id = chunkId;
        }

        public NativeArray<float>.ReadOnly Densities { get; }

        public ChunkId Id { get; }

        public int3 Coords => Id.Coords;

        public int3 Size => Id.Size;

        public int3 Position => Coords * Size;

        public override string ToString()
        {
            return Id.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is ChunkData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(ChunkData other)
        {
            return Id.Equals(other.Id);
        }

        public static bool operator ==(ChunkData left, ChunkData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkData left, ChunkData right)
        {
            return !left.Equals(right);
        }
    }
}