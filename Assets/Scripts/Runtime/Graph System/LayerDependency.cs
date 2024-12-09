using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerDependency : ScriptableObject
    {
        [SerializeField] private int3 _padding;

        [field: SerializeField] public LayerReference Dependent { get; private set; }
        [field: SerializeField] public LayerReference Dependency { get; private set; }

        public int3 Padding
        {
            get => _padding;
            set => _padding = math.max(value, int3.zero);
        }

#if UNITY_EDITOR
        public static LayerDependency Create(LayerNodeData dependentNodeData, LayerNodeData dependencyNodeData)
        {
            LayerDependency layerDependency = CreateInstance<LayerDependency>();
            layerDependency.Dependent = dependentNodeData.LayerReference;
            layerDependency.Dependency = dependencyNodeData.LayerReference;
            layerDependency.Padding = int3.zero;

            layerDependency.name = $"{layerDependency.Dependent} -> {layerDependency.Dependency}";

            AssetDatabase.AddObjectToAsset(layerDependency, dependentNodeData);
            AssetDatabase.SaveAssets();

            return layerDependency;
        }

        public static void Destroy(LayerDependency layerDependency)
        {
            AssetDatabase.RemoveObjectFromAsset(layerDependency);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}