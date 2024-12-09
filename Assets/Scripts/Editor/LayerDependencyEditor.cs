using Rafasixteen.Runtime.ChunkLab;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Editor.ChunkLab
{
    [CustomEditor(typeof(LayerDependency))]
    public class LayerDependencyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            LayerDependency layerDependency = (LayerDependency)target;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Dependency Relationship", EditorStyles.label);
            EditorGUILayout.LabelField($"{layerDependency.Dependent} -> {layerDependency.Dependency}", EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            Vector3Int value = new(layerDependency.Padding.x, layerDependency.Padding.y, layerDependency.Padding.z);
            value = EditorGUILayout.Vector3IntField("Padding", value);
            layerDependency.Padding = new(value.x, value.y, value.z);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                $"The padding ensures that chunks from {layerDependency.Dependency} are properly generated around the {layerDependency.Dependent} chunks.",
                MessageType.Info);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}