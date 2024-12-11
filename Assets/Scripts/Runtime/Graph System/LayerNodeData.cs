using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerNodeData : ScriptableObject
    {
        [field: SerializeField] public LayerReference LayerReference { get; private set; }
        [field: SerializeField] public LayerSettings LayerSettings { get; private set; }
        [field: SerializeField] public List<LayerDependencySettings> LayerDependencies { get; private set; }

        [field: SerializeField] public Vector2 NodePosition { get; set; }
        [field: SerializeField] public string NodeGuid { get; private set; }
        [field: SerializeField] public bool IsLeafNode { get; private set; }
        [field: SerializeField] public List<LayerNodeData> ConnectedNodes { get; private set; }

#if UNITY_EDITOR
        public static LayerNodeData Create(LayerReference layerReference, bool isLeaf = false)
        {
            LayerNodeData node = CreateInstance<LayerNodeData>();

            node.LayerReference = layerReference;
            node.LayerSettings = LayerSettings.CreateFrom(layerReference);
            node.LayerDependencies = new();

            node.name = layerReference.ToString();
            node.NodePosition = Vector2.zero;
            node.NodeGuid = GUID.Generate().ToString();
            node.IsLeafNode = isLeaf;
            node.ConnectedNodes = new();

            return node;
        }
#endif
    }
}