using Rafasixteen.Runtime.ChunkLab;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Editor.ChunkLab
{
    [CustomPropertyDrawer(typeof(LayerDependencyReference))]
    public class LayerDependencyReferenceDrawer : PropertyDrawer
    {
        private static IEnumerable<Type> _availableDependencyTypes;
        private static IEnumerable<string> _dependencyTypeFullNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _availableDependencyTypes = TypeCache.GetTypesDerivedFrom<LayerDependencyV2>()
                .Where(type => !type.IsAbstract)
                .OrderBy(type => type.Name);

            _dependencyTypeFullNames = _availableDependencyTypes.Select(t => t.FullName);

            float dropdownWidth = position.width * 0.75f;
            float labelWidth = position.width - dropdownWidth;

            Rect labelRect = new(position.x, position.y, labelWidth, position.height);
            Rect dropdownRect = new(position.x + labelWidth, position.y, dropdownWidth, position.height);

            EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty classNameProperty = property.FindPropertyRelative("_dependencyTypeFullName");
            int currentIndex = Array.IndexOf(_dependencyTypeFullNames.ToArray(), classNameProperty.stringValue);
            int selectedIndex = EditorGUI.Popup(dropdownRect, currentIndex, _availableDependencyTypes.Select(t => t.Name).ToArray());

            if (selectedIndex >= 0 && selectedIndex != currentIndex)
                classNameProperty.stringValue = _availableDependencyTypes.ElementAt(selectedIndex).FullName;
        }
    }
}