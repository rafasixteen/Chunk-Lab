using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerDependencySettings : ScriptableObject
    {
        [SerializeField] private LayerDependencyReference _dependencyReference;

        [field: SerializeField] public LayerReference Dependent { get; private set; }
        [field: SerializeField] public LayerReference Dependency { get; private set; }

        public LayerDependencyReference LayerDependencyReference => _dependencyReference;

#if UNITY_EDITOR
        public static LayerDependencySettings Create(LayerNodeData dependentNodeData, LayerNodeData dependencyNodeData)
        {
            LayerDependencySettings layerDependency = CreateInstance<LayerDependencySettings>();
            layerDependency.Dependent = dependentNodeData.LayerReference;
            layerDependency.Dependency = dependencyNodeData.LayerReference;

            layerDependency.name = $"{layerDependency.Dependent} -> {layerDependency.Dependency}";

            AssetDatabase.AddObjectToAsset(layerDependency, dependentNodeData);
            AssetDatabase.SaveAssets();

            return layerDependency;
        }

        public static void Destroy(LayerDependencySettings layerDependency)
        {
            AssetDatabase.RemoveObjectFromAsset(layerDependency);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}