using Rafasixteen.Runtime.ChunkLab;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rafasixteen.Editor.ChunkLab
{
    public class ChunkLabManagerWindow : EditorWindow
    {
        private LayerGraphView _graphView;
        private InspectorView _inspectorView;

        [SerializeField] private VisualTreeAsset _visualTreeAsset = default;

        public static void Show(LayerGraphData graphData)
        {
            const float k_aspectRatio = 16f / 9f;
            const int k_minWidth = 900;
            const float k_minHeight = k_minWidth / k_aspectRatio;

            ChunkLabManagerWindow window = GetWindow<ChunkLabManagerWindow>();
            window.minSize = new Vector2(k_minWidth, k_minHeight);
            window._graphView.Load(graphData);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            _visualTreeAsset.CloneTree(root);

            _graphView = root.Q<LayerGraphView>();
            _inspectorView = root.Q<InspectorView>();

            StyleSheet styleSheet = LoadStyleSheetByGuid("8932b384ddf663941953d5d14b9bc08c");
            _graphView.styleSheets.Add(styleSheet);

            _graphView.OnSelectionChanged += OnGraphViewSelectionChanged;
        }

        private void OnGraphViewSelectionChanged(List<ISelectable> selection)
        {
            if (selection.Count > 1)
            {
                _inspectorView.Inspect(null);
                _inspectorView.ShowNotSupportedMessage();
                return;
            }

            if (selection.Count == 0)
            {
                _inspectorView.Inspect(null);
                return;
            }

            ISelectable selectable = selection.First();

            if (selectable is LayerNode node)
            {
                _inspectorView.Inspect(node.Data.LayerSettings);
            }
            else
            {
                _inspectorView.Inspect(null);
            }
        }

        private StyleSheet LoadStyleSheetByGuid(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);

            if (styleSheet == null)
                throw new ArgumentException("Style sheet with GUID " + guid + " could not be loaded from path: " + path);

            return styleSheet;
        }
    }
}