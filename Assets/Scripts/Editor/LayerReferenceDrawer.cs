using Rafasixteen.Runtime.ChunkLab;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rafasixteen.Editor.ChunkLab
{
    [CustomPropertyDrawer(typeof(LayerReference))]
    public class LayerReferenceDrawer : PropertyDrawer
    {
        private static IEnumerable<Type> _availableLayerTypes;
        private static IEnumerable<string> _layerTypeFullNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _availableLayerTypes = TypeCache.GetTypesDerivedFrom<LayerBase>()
                .Where(type => !type.IsAbstract)
                .OrderBy(type => type.Name);

            _layerTypeFullNames = _availableLayerTypes.Select(t => t.FullName);

            float dropdownWidth = position.width * 0.75f;
            float labelWidth = position.width - dropdownWidth;

            Rect labelRect = new(position.x, position.y, labelWidth, position.height);
            Rect dropdownRect = new(position.x + labelWidth, position.y, dropdownWidth, position.height);

            EditorGUI.PrefixLabel(labelRect, GUIUtility.GetControlID(FocusType.Passive), label);

            SerializedProperty classNameProperty = property.FindPropertyRelative("_layerTypeFullName");
            int currentIndex = Array.IndexOf(_layerTypeFullNames.ToArray(), classNameProperty.stringValue);
            int selectedIndex = EditorGUI.Popup(dropdownRect, currentIndex, _availableLayerTypes.Select(t => t.Name).ToArray());

            if (selectedIndex >= 0 && selectedIndex != currentIndex)
                classNameProperty.stringValue = _availableLayerTypes.ElementAt(selectedIndex).FullName;
        }
    }
}