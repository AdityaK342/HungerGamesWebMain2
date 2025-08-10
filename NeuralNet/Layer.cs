namespace NeuralNet
{
    /// <summary>
    /// One layer of a neural network
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// All the nodes in the layer
        /// </summary>
        public List<Node> Nodes { get; init; } = [];

        public Layer(List<Node> nodes)
        {
            Nodes = nodes;
        }

        /// <summary>
        /// All non-bias nodes
        /// </summary>
        public int UsableNodes => Nodes.Count((x) => x.ActivationFunction != Node.ActivationFunctionChoice.Bias);

        /// <summary>
        /// Adds data to the node at the given index
        /// </summary>
        public void AddData(double value, int index)
        {
            Nodes[index].AddData(value);
        }

        /// <summary>
        /// Gets the result of a single node
        /// </summary>
        public double GetOutput(int index)
        {
            return Nodes[index].GetValue();
        }

        public Layer(int nNodes, Node.ActivationFunctionChoice activationFunction = Node.ActivationFunctionChoice.None, double parameter = 0, bool hasBiasNode = false)
        {
            for (int i = 0; i < nNodes; ++i)
            {
                Node node;
                if (i == nNodes - 1 && hasBiasNode)
                {
                    node = new Node(Node.ActivationFunctionChoice.Bias);
                }
                else
                {
                    node = new Node(activationFunction, parameter);
                }
                Nodes.Add(node);
            }
        }

        /// <summary>
        /// Connects this layer to the next layer
        /// Just connects every node in this layer to every node in the next layer
        /// </summary>
        public void ConnectToLayer(Layer nextLayer)
        {
            foreach (var node in Nodes)
            {
                foreach (var nextNode in nextLayer.Nodes)
                {
                    var connector = new Connector(nextNode, 1);
                    node.Connectors.Add(connector);
                }
            }
        }

        /// <summary>
        /// Runs the data in this layer through the nodes so the next layer can use it
        /// </summary>
        public void FeedForward()
        {
            foreach (var node in Nodes)
            {
                node.FeedForward();
            }
        }

        /// <summary>
        /// Resets all the values in the nodes
        /// </summary>
        public void Reset()
        {
            foreach (var node in Nodes)
            {
                node.Reset();
            }
        }

        /// <summary>
        /// Clones the layer
        /// </summary>
        public Layer Clone()
        {
            var nodes = new List<Node>();

            foreach (var node in Nodes)
            {
                var newNode = node.ConnectorlessClone();
                nodes.Add(newNode);
            }
            return new Layer(nodes);
        }

        /// <summary>
        /// Writes only the references to the nodes, which have been stored elsewhere
        /// </summary>
        public void Write(BinaryWriter bw, Dictionary<Node, int> nodeDictionary)
        {
            bw.Write(Nodes.Count);
            foreach (var node in Nodes)
            {
                bw.Write(nodeDictionary[node]);
            }
        }

        /// <summary>
        /// Creates a layer from a file
        /// Assume that the nodeDictionary is already filled
        /// </summary>
        public Layer(BinaryReader br, Dictionary<int, Node> nodeDictionary)
        {
            int nNodes = br.ReadInt32();

            for (int i = 0; i < nNodes; ++i)
            {
                int index = br.ReadInt32();
                Nodes.Add(nodeDictionary[index]);
            }
        }
    }
}