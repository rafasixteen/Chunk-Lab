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
    [UxmlElement("LayerGraphView")]
    public partial class LayerGraphView : GraphView
    {
        private List<ISelectable> _lastSelection;

        private LayerGraphData _graphData;

        public LayerGraphView()
        {
            _lastSelection = new();

            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            graphViewChanged += OnGraphViewChanged;
            schedule.Execute(CheckSelectionChanged).Every(20);
        }

        public event Action<List<ISelectable>> OnSelectionChanged;

        public void Load(LayerGraphData graphData)
        {
            _graphData = graphData;

            foreach (LayerNodeData nodeData in graphData.Nodes)
                AddElement(LayerNode.Load(nodeData));

            foreach (LayerNodeData nodeData in graphData.Nodes)
            {
                LayerNode fromNode = GetNodeByGuid(nodeData.NodeGuid) as LayerNode;

                foreach (LayerNodeData connectedNode in nodeData.ConnectedNodes)
                {
                    LayerNode toNode = GetNodeByGuid(connectedNode.NodeGuid) as LayerNode;
                    Edge edge = fromNode.OutputPort.ConnectTo(toNode.InputPort);
                    AddElement(edge);
                }
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            bool hasLeafLayer = _graphData.HasLeafLayer();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<LayerBase>().OrderBy(type => type.Name))
            {
                if (type.IsAbstract)
                    continue;

                bool isLeafNode = type.GetCustomAttributes(typeof(LeafLayerAttribute), true).Length > 0;
                string category = isLeafNode ? "Add Leaf Layer" : "Add Layer";

                DropdownMenuAction.Status status = _graphData.IsUsedType(type)
                    ? DropdownMenuAction.Status.Disabled
                    : DropdownMenuAction.Status.Normal;

                evt.menu.AppendAction($"{category}/{type.Name}", action =>
                {
                    Vector2 position = contentViewContainer.WorldToLocal(action.eventInfo.localMousePosition);
                    AddElement(CreateNode(type, position, isLeafNode));
                }, status);
            }

            if (hasLeafLayer)
                evt.menu.AppendAction("Add Leaf Layer", null, DropdownMenuAction.Status.Disabled);

            base.BuildContextualMenu(evt);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            ports.ForEach(port =>
            {
                if (startPort == port || startPort.node == port.node || startPort.direction == port.direction)
                    return;

                LayerNode fromNode = startPort.node as LayerNode;
                LayerNode toNode = port.node as LayerNode;

                if (WouldCreateCycle(fromNode, toNode))
                    return;

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private bool WouldCreateCycle(LayerNode fromNode, LayerNode toNode)
        {
            Stack<LayerNode> stack = new();
            HashSet<LayerNode> visited = new();

            stack.Push(toNode);

            while (stack.Count > 0)
            {
                LayerNode currentNode = stack.Pop();

                if (currentNode == fromNode)
                    return true;

                if (!visited.Add(currentNode))
                    continue;

                foreach (LayerNodeData node in currentNode.Data.ConnectedNodes)
                {
                    LayerNode layerNode = GetNodeByGuid(node.NodeGuid) as LayerNode;
                    stack.Push(layerNode);
                }
            }

            return false;
        }

        private void CheckSelectionChanged()
        {
            List<ISelectable> currentSelection = new(selection);

            if (!IsSelectionEqual(_lastSelection, currentSelection))
            {
                _lastSelection = currentSelection;
                OnSelectionChanged?.Invoke(currentSelection);
            }
        }

        private bool IsSelectionEqual(List<ISelectable> list1, List<ISelectable> list2)
        {
            if (list1.Count != list2.Count)
                return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                    return false;
            }

            return true;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    if (element is LayerNode node)
                        _graphData.DestroyNodeData(node.Data);

                    if (element is Edge edge)
                        OnEdgeDisconnected(edge);
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                    OnEdgeConnected(edge);
            }

            if (graphViewChange.movedElements != null)
            {
                foreach (GraphElement element in graphViewChange.movedElements)
                {
                    if (element is LayerNode node)
                        OnNodeMoved(node);
                }
            }

            return graphViewChange;
        }

        private void OnEdgeConnected(Edge edge)
        {
            LayerNode from = edge.output.node as LayerNode;
            LayerNode to = edge.input.node as LayerNode;
            from.ConnectTo(to);
        }

        private void OnEdgeDisconnected(Edge edge)
        {
            LayerNode from = edge.output.node as LayerNode;
            LayerNode to = edge.input.node as LayerNode;
            from.DisconnectFrom(to);
        }

        private void OnNodeMoved(LayerNode node)
        {
            node.Data.NodePosition = node.GetPosition().position;
        }

        private LayerNode CreateNode(LayerReference layerReference, Vector2 position, bool isleafNode)
        {
            LayerNodeData nodeData = _graphData.CreateNodeData(layerReference, isleafNode);
            nodeData.NodePosition = position;
            LayerNode node = LayerNode.Load(nodeData);
            return node;
        }
    }
}