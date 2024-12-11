using System;
using Unity.Mathematics;
using UnityEngine;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerSettings : ScriptableObject
    {
        #region FIELDS

        [SerializeField] private int3 _chunkSize;

        [Header("Visualization Settings"), Space(10)]
        [SerializeField] private bool _enableVisualization = true;
        [SerializeField] private EVisualizationSettings _visualizationSettings = EVisualizationSettings.Default;

        [Header("Chunk Colors"), Space(10)]
        [SerializeField] private Color _color = Color.green;
        [SerializeField] private Color _creating = Color.blue;
        [SerializeField] private Color _pendingCreation = Color.yellow;
        [SerializeField] private Color _pendingDestruction = Color.red;

        #endregion

        #region PROPERTIES

        public int3 ChunkSize => _chunkSize;

        public bool EnableVisualization => _enableVisualization;
        public EVisualizationSettings VisualizationSettings => _visualizationSettings;

        public Color ChunkColor => _color;
        public Color ChunkColorCreating => _creating;
        public Color ChunkColorPendingCreation => _pendingCreation;
        public Color ChunkColorPendingDestruction => _pendingDestruction;

        #endregion

        #region METHODS

        public static LayerSettings CreateFrom(LayerReference layerReference)
        {
            Type settingsType = GetSettingsType(layerReference);
            LayerSettings settings = CreateInstance(settingsType) as LayerSettings;
            settings.name = $"{layerReference} Settings";
            return settings;
        }

        private static Type GetSettingsType(LayerReference layerReference)
        {
            const int k_settingsTypeArgumentIndex = 2;
            Type layerType = layerReference.Type;

            while (layerType != null)
            {
                if (layerType.IsGenericType)
                {
                    Type[] genericArgs = layerType.GetGenericArguments();

                    if (genericArgs.Length > k_settingsTypeArgumentIndex)
                        return genericArgs[k_settingsTypeArgumentIndex];
                }

                layerType = layerType.BaseType;
            }

            return typeof(LayerSettings);
        }

        #endregion
    }
}