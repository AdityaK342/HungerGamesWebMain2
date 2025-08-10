namespace NeuralNet
{
    /// <summary>
    /// A connector between two nodes in a neural network
    /// </summary>
    public class Connector
    {
        /// <summary>
        /// The weight of the connection
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// The node that the connector points to
        /// </summary>
        public Node Node { get; set; }

        public Connector(Node node, double weight)
        {
            Node = node;
            Weight = weight;
        }

        public void Write(BinaryWriter bw, Node sourceNode, Dictionary<Node, int> nodeDictionary)
        {
            bw.Write(nodeDictionary[sourceNode]);
            bw.Write(nodeDictionary[Node]);
            bw.Write(Weight);
        }

        public Connector(BinaryReader br, Dictionary<int, Node> nodeDictionary, out Node sourceNode)
        {
            int sourceNodeNum = br.ReadInt32();
            sourceNode = nodeDictionary[sourceNodeNum];
            int nodeNum = br.ReadInt32();
            var node = nodeDictionary[nodeNum];
            double weight = br.ReadDouble();

            Node = node;
            Weight = weight;
        }
    }
}
