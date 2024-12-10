using System;
using System.Collections.Generic;

namespace Rafasixteen.Runtime.ChunkLab
{
    public class LayerManager
    {
        private Dictionary<LayerId, LayerBase> _layersDictionary;
        private List<LayerBase> _layers;

        public LayerManager(LayerGraphData graphData)
        {
            _layers = InstantiateLayers(graphData);

            _layersDictionary = new();

            for (int i = 0; i < _layers.Count; i++)
            {
                LayerBase layer = _layers[i];
                _layersDictionary.Add(layer.Id, layer);
            }
        }

        public int Count => _layers.Count;

        public LayerBase this[int i] => _layers[i];

        internal LeafLayer LeafLayer { get; private set; }

        public LayerBase GetLayer(LayerId layerId)
        {
            return _layersDictionary[layerId];
        }

        public TLayer GetLayer<TLayer>() where TLayer : LayerBase
        {
            return GetLayer(typeof(TLayer)) as TLayer;
        }

        public LayerBase GetLayer(LayerReference layerReference)
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                LayerBase layer = _layers[i];

                if (layer.GetType() == layerReference.Type)
                    return layer;
            }

            throw new InvalidOperationException($"{layerReference.Type.Name} was not instantiated.");
        }

        private List<LayerBase> InstantiateLayers(LayerGraphData graphData)
        {
            List<LayerBase> layers = new();

            foreach (LayerNodeData nodeData in graphData.Nodes)
                layers.Add(InstantiateLayer(nodeData));

            return layers;
        }

        private LayerBase InstantiateLayer(LayerNodeData nodeData)
        {
            LayerBase layer = Activator.CreateInstance(nodeData.LayerReference) as LayerBase;

            layer.Settings = nodeData.LayerSettings;
            layer.Name = layer.GetType().Name;
            layer.Id = new(layer.GetType());

            if (nodeData.IsLeafNode)
            {
                if (LeafLayer != null)
                    throw new InvalidOperationException($"Cannot instantiate a leaf layer of type '{layer.GetType().Name}' because a leaf layer of type '{LeafLayer.GetType().Name}' already exists. The system allows only one leaf layer.");

                LeafLayer = new(layer);
            }

            return layer;
        }
    }
}
