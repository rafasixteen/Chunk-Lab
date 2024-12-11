using System;
using System.Linq;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    [Serializable]
    public class LayerDependencyReference : ISerializationCallbackReceiver, IEquatable<LayerDependencyReference>
    {
        #region FIELDS

        [NonSerialized] private Type _dependencyType;
        [SerializeField] private string _dependencyTypeFullName;

        #endregion

        #region CONSTRUCTORS

        public LayerDependencyReference(Type dependencyType)
        {
            if (!typeof(LayerDependencyV2).IsAssignableFrom(dependencyType))
                throw new ArgumentException($"The provided type is not a valid dependency type. Ensure the provided type is a subclass of {nameof(LayerDependencyV2)}.");

            Type = dependencyType;
        }

        public LayerDependencyReference()
        {

        }

        #endregion

        #region PROPERTIES

        public Type Type
        {
            get => _dependencyType;
            set
            {
                _dependencyType = value;
                _dependencyTypeFullName = value.FullName;
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
            return obj is LayerDependencyReference other && Equals(other);
        }

        public bool Equals(LayerDependencyReference other)
        {
            return Type.Equals(other.Type);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(_dependencyTypeFullName))
            {
                _dependencyType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(type => type.FullName == _dependencyTypeFullName);
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_dependencyType != null)
                _dependencyTypeFullName = _dependencyType.FullName;
        }

        #endregion

        #region OPERATORS

        public static bool operator ==(LayerDependencyReference a, LayerDependencyReference b)
        {
            return a.Type == b.Type;
        }

        public static bool operator !=(LayerDependencyReference a, LayerDependencyReference b)
        {
            return a.Type != b.Type;
        }

        public static implicit operator Type(LayerDependencyReference LayerDependencyReference)
        {
            return LayerDependencyReference.Type;
        }

        public static implicit operator LayerDependencyReference(Type layerType)
        {
            return new(layerType);
        }

        #endregion
    }
}