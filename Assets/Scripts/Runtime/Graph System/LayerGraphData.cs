using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerGraphData : ScriptableObject
    {
        [field: SerializeField] public List<LayerNodeData> Nodes { get; private set; }

#if UNITY_EDITOR
        public static LayerGraphData Create()
        {
            LayerGraphData layerGraph = CreateInstance<LayerGraphData>();
            layerGraph.Nodes = new();

            string folderPath = EnsureFolderExists("Assets", "Chunk Lab");
            string assetPath = Path.Combine(folderPath, $"{nameof(LayerGraphData)}.asset");

            AssetDatabase.CreateAsset(layerGraph, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            layerGraph.CreateNodeData(typeof(LeafLayer), true);

            return layerGraph;
        }

        public LayerNodeData CreateNodeData(LayerReference layerReference, bool isLeaf = false)
        {
            LayerNodeData nodeData = LayerNodeData.Create(layerReference, isLeaf);
            Nodes.Add(nodeData);

            AssetDatabase.AddObjectToAsset(nodeData, this);
            AssetDatabase.AddObjectToAsset(nodeData.LayerSettings, this);
            AssetDatabase.SaveAssets();

            return nodeData;
        }

        public void DestroyNodeData(LayerNodeData nodeData)
        {
            Nodes.Remove(nodeData);

            AssetDatabase.RemoveObjectFromAsset(nodeData);
            AssetDatabase.RemoveObjectFromAsset(nodeData.LayerSettings);
            AssetDatabase.SaveAssets();
        }

        public bool IsUsedType(LayerReference layerReference)
        {
            return Nodes.Any(nodeData => nodeData.LayerReference == layerReference);
        }

        private static string EnsureFolderExists(string parentFolder, string childFolder)
        {
            string folderPath = Path.Combine(parentFolder, childFolder);

            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(parentFolder, childFolder);

            return folderPath;
        }
#endif
    }
}