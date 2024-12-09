using System;
using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public readonly struct LayerId : IEquatable<LayerId>
    {
        public LayerId(LayerReference layerReference)
        {
            Name = new(layerReference.Type.Name);
        }

        public FixedString64Bytes Name { get; }

        public bool Equals(LayerId other)
        {
            return Name.Equals(other.Name);
        }

        public override readonly bool Equals(object obj)
        {
            return obj is LayerId other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override readonly string ToString()
        {
            return Name.ToString();
        }

        public static bool operator ==(LayerId left, LayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerId left, LayerId right)
        {
            return !left.Equals(right);
        }
    }
}