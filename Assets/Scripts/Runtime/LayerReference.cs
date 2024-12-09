using System;
using System.Linq;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    [Serializable]
    public class LayerReference : ISerializationCallbackReceiver, IEquatable<LayerReference>
    {
        #region FIELDS

        [NonSerialized] private Type _layerType;
        [SerializeField] private string _layerTypeFullName;

        #endregion

        #region CONSTRUCTORS

        public LayerReference(Type layerType)
        {
            if (!typeof(LayerBase).IsAssignableFrom(layerType))
                throw new ArgumentException($"The provided type is not a valid layer type. Ensure the provided type is a subclass of {nameof(LayerBase)}.");

            Type = layerType;
        }

        public LayerReference()
        {

        }

        #endregion

        #region PROPERTIES

        public Type Type
        {
            get => _layerType;
            set
            {
                _layerType = value;
                _layerTypeFullName = value.FullName;
            }
        }

        #endregion

        #region METHODS

        public override string ToString()
        {
            return Type.Name;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is LayerReference other && Equals(other);
        }

        public bool Equals(LayerReference other)
        {
            return Type.Equals(other.Type);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_layerTypeFullName))
            {
                _layerType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(type => type.FullName == _layerTypeFullName);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_layerType != null)
                _layerTypeFullName = _layerType.FullName;
        }

        #endregion

        #region OPERATORS

        public static bool operator ==(LayerReference a, LayerReference b)
        {
            return a.Type == b.Type;
        }

        public static bool operator !=(LayerReference a, LayerReference b)
        {
            return a.Type != b.Type;
        }

        public static implicit operator Type(LayerReference layerReference)
        {
            return layerReference.Type;
        }

        public static implicit operator LayerReference(Type layerType)
        {
            return new(layerType);
        }

        #endregion
    }
}