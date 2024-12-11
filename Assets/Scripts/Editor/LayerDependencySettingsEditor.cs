using Rafasixteen.Runtime.ChunkLab;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Editor.ChunkLab
{
    [CustomEditor(typeof(LayerDependencySettings))]
    public class LayerDependencySettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _dependencyReference;

        private void OnEnable()
        {
            _dependencyReference = serializedObject.FindProperty("_dependencyReference");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LayerDependencySettings layerDependency = (LayerDependencySettings)target;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Dependency Relationship", EditorStyles.label);
            EditorGUILayout.LabelField($"{layerDependency.Dependent} -> {layerDependency.Dependency}", EditorStyles.helpBox);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_dependencyReference, new GUIContent("Dependency Reference"));

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}