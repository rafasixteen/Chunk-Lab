using UnityEngine;
using UnityEngine.UIElements;

namespace Rafasixteen.Editor.ChunkLab
{
    [UxmlElement("InspectorView")]
    public partial class InspectorView : VisualElement
    {
        private UnityEditor.Editor _editor;

        public void Inspect(ScriptableObject scriptableObject)
        {
            Clear();

            if (_editor != null)
                Object.DestroyImmediate(_editor);

            if (scriptableObject != null)
            {
                _editor = UnityEditor.Editor.CreateEditor(scriptableObject);
                IMGUIContainer settingsContainer = new(() => _editor.OnInspectorGUI());
                Add(settingsContainer);
            }
        }

        public void ShowNotSupportedMessage()
        {
            Label notSupportedLabel = new("Multi-object editing is not supported.");
            notSupportedLabel.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);

            const int k_padding = 5;
            notSupportedLabel.style.paddingTop = k_padding;
            notSupportedLabel.style.paddingBottom = k_padding;
            notSupportedLabel.style.paddingLeft = k_padding;
            notSupportedLabel.style.paddingRight = k_padding;

            Add(notSupportedLabel);
        }
    }
}