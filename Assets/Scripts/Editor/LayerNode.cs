using Rafasixteen.Runtime.ChunkLab;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Rafasixteen.Editor.ChunkLab
{
    public class LayerNode : Node
    {
        private LayerNode(LayerNodeData data)
        {
            Data = data;
            viewDataKey = data.NodeGuid;
            title = data.LayerReference.Type.Name;
            SetPosition(new(data.NodePosition, Vector2.zero));
        }

        public LayerNodeData Data { get; }

        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        public static LayerNode Load(LayerNodeData data)
        {
            LayerNode node = new(data);

            if (data.IsLeafNode)
            {
                node.CreateOutputPort();
            }
            else
            {
                node.CreateInputPort();
                node.CreateOutputPort();
            }

            return node;
        }

        public void ConnectTo(LayerNode node)
        {
            Data.ConnectedNodes.Add(node.Data);
            Data.LayerDependencies.Add(LayerDependencySettings.Create(Data, node.Data));
        }

        public void DisconnectFrom(LayerNode node)
        {
            Data.ConnectedNodes.Remove(node.Data);

            LayerDependencySettings layerDependency = Data.LayerDependencies.FirstOrDefault(dependency => dependency.Dependency == node.Data.LayerReference);
            
            Data.LayerDependencies.Remove(layerDependency);
            LayerDependencySettings.Destroy(layerDependency);
        }

        private void CreateInputPort()
        {
            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            InputPort.portName = "Input";
            inputContainer.Add(InputPort);
        }

        private void CreateOutputPort()
        {
            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
            OutputPort.portName = "Output";
            outputContainer.Add(OutputPort);
        }
    }
}