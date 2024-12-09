using System;
using Unity.Collections;

namespace Rafasixteen.Runtime.ChunkLab
{
    public struct BitReference : IDisposable, IEquatable<BitReference>
    {
        private NativeReference<byte> _reference;

        public BitReference(Allocator allocator)
        {
            _reference = new(allocator);
        }

        public bool IsSet => _reference.IsCreated && _reference.Value == 1;

        public void Set()
        {
            _reference.Value = 1;
        }

        public void Reset()
        {
            _reference.Value = 0;
        }

        public void Dispose()
        {
            if (_reference.IsCreated)
                _reference.Dispose();
        }

        public bool Equals(BitReference other)
        {
            return _reference.Equals(other._reference);
        }

        public override bool Equals(object obj)
        {
            return obj is BitReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _reference.GetHashCode();
        }

        public override string ToString()
        {
            return $"Bit Reference ({IsSet})";
        }

        public static bool operator ==(BitReference left, BitReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BitReference left, BitReference right)
        {
            return !left.Equals(right);
        }

        public static implicit operator bool(BitReference reference)
        {
            return reference.IsSet;
        }
    }
}