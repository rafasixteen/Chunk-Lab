using Rafasixteen.Runtime.ChunkLab;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Editor.ChunkLab
{
    [CustomEditor(typeof(ChunkLabManager))]
    public class ChunkLabManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _layerGraph;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button($"Open Layer Manager Window"))
                OpenLayerManagerWindow();
        }

        private void OnEnable()
        {
            _layerGraph = serializedObject.FindProperty("_layerGraphData");
        }

        private void OpenLayerManagerWindow()
        {
            LayerGraphData layerGraphData = _layerGraph.objectReferenceValue as LayerGraphData;

            if (layerGraphData == null)
            {
                layerGraphData = LayerGraphData.Create();
                _layerGraph.objectReferenceValue = layerGraphData;
                serializedObject.ApplyModifiedProperties();
            }

            ChunkLabManagerWindow.Show(layerGraphData);
        }
    }
}