using DongUtility;

namespace NeuralNet
{
    /// <summary>
    /// A general perceptron class with multiple hidden layers and arbitrary arrangement of activation functions
    /// </summary>
    /// <remarks>
    /// Primary, most flexible constructor. All Layers can be custom-made.  You have to make all the connections yourself.
    /// </remarks>
    public class Perceptron(Layer input, List<Layer> hiddenLayers, Layer output)
    {
        static public Random Random { get; } = new();

        /// <summary>
        /// Number of inputs and outputs expected
        /// </summary>
        public int NInputs => InputLayer.UsableNodes;
        public int NOutputs => OutputLayer.UsableNodes;

        private Layer InputLayer { get; init; } = input;
        private Layer OutputLayer { get; init; } = output;
        private List<Layer> HiddenLayers { get; init; } = hiddenLayers;

        /// <summary>
        /// More practical constructor allowing multiple hidden layers with a fixed activation function
        /// </summary>
        public Perceptron(int nInputs, int nOutputs, List<int> hiddenNodes, Node.ActivationFunctionChoice activationFunction, double parameter = 0, bool hasBiasNode = false) :
            this(new Layer(nInputs, Node.ActivationFunctionChoice.None, 0, hasBiasNode), MakeLayers(hiddenNodes, activationFunction, parameter).ToList(), new Layer(nOutputs))
        {
            // Connect all the layers
            if (HiddenLayers.Count == 0)
            {
                InputLayer.ConnectToLayer(OutputLayer);
            }
            else
            {
                InputLayer.ConnectToLayer(HiddenLayers[0]);
                for (int i = 0; i < HiddenLayers.Count; ++i)
                {
                    var nextLayer = i == HiddenLayers.Count - 1 ? OutputLayer : HiddenLayers[i + 1];
                    HiddenLayers[i].ConnectToLayer(nextLayer);
                }
            }
        }

        /// <summary>
        /// Constructor for a simple single hidden layer
        /// </summary>
        public Perceptron(int nInputs, int nOutputs, int nHidden, Node.ActivationFunctionChoice activationFunction, double parameter = 0, bool hasBiasNode = false) :
            this(nInputs, nOutputs, [nHidden], activationFunction, parameter, hasBiasNode)
        { }

        /// <summary>
        /// Constructor for single-layer perceptron
        /// </summary>
        public Perceptron(int nInputs, int nOutputs, bool hasBiasNode = false) :
            this(nInputs, nOutputs, [], Node.ActivationFunctionChoice.None, 0, hasBiasNode)
        { }

        static private IEnumerable<Layer> MakeLayers(List<int> nHiddenNodes, Node.ActivationFunctionChoice activationFunction, double parameter)
        {
            foreach (var nNodes in nHiddenNodes)
            {
                yield return new Layer(nNodes, activationFunction, parameter);
            }
        }

        /// <summary>
        /// Gets all the layers, including input, hidden, and output
        /// </summary>
        private IEnumerable<Layer> Layers
        {
            get
            {
                yield return InputLayer;
                foreach (var layer in HiddenLayers)
                {
                    yield return layer;
                }
                yield return OutputLayer;
            }
        }

        /// <summary>
        /// Gets all the nodes in all the layers
        /// </summary>
        private IEnumerable<Node> Nodes
        {
            get
            {
                foreach (var layer in Layers)
                {
                    foreach (var node in layer.Nodes)
                    {
                        yield return node;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all the connectors from all the layers
        /// </summary>
        public IEnumerable<Connector> Connectors
        {
            get
            {
                foreach (var node in Nodes)
                {
                    foreach (var connector in node.Connectors)
                    {
                        yield return connector;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an input value to a specified input node
        /// </summary>
        public void AddInput(int index, double value)
        {
            InputLayer.AddData(value, index);
        }

        /// <summary>
        /// Adds input values to all the input nodes
        /// </summary>
        public void AddInputs(params double[] values)
        {
            if (values.Length != NInputs)
            {
                throw new ArgumentException("Wrong number of arguments passed to AddInputs()!");
            }
            for (int i = 0; i < values.Length; ++i)
            {
                AddInput(i, values[i]);
            }
        }

        /// <summary>
        /// Resets all node values to zero
        /// </summary>
        public virtual void Reset()
        {
            foreach (var layer in Layers)
            {
                layer.Reset();
            }
        }

        /// <summary>
        /// Runs the neural network
        /// </summary>
        public void Run()
        {
            InputLayer.FeedForward();
            foreach (var layer in HiddenLayers)
            {
                layer.FeedForward();
            }
        }

        /// <summary>
        /// Creates a clone of the network, preserving all the weights in the connectors
        /// </summary>
        public Perceptron Clone()
        {
            // Need to keep track of the old nodes and new nodes
            var nodeDict = new Dictionary<Node, Node>(); // old to new
            var clonedHiddenLayers = new List<Layer>();
            foreach (var layer in HiddenLayers)
            {
                var newLayer = layer.Clone();
                for (int i = 0; i < newLayer.Nodes.Count; ++i)
                {
                    nodeDict.Add(layer.Nodes[i], newLayer.Nodes[i]);
                }
                clonedHiddenLayers.Add(newLayer);

            }
            var clonedInputLayer = InputLayer.Clone();
            var clonedOutputLayer = OutputLayer.Clone();

            for (int i = 0; i < InputLayer.Nodes.Count; ++i)
            {
                nodeDict.Add(InputLayer.Nodes[i], clonedInputLayer.Nodes[i]);
            }
            for (int i = 0; i < OutputLayer.Nodes.Count; ++i)
            {
                nodeDict.Add(OutputLayer.Nodes[i], clonedOutputLayer.Nodes[i]);
            }

            // Copy connectors
            foreach (var node in Nodes)
                foreach (var connector in node.Connectors)
                {
                    var newConnector = new Connector(nodeDict[connector.Node], connector.Weight);
                    nodeDict[node].Connectors.Add(newConnector);
                }

            return new Perceptron(clonedInputLayer, clonedHiddenLayers, clonedOutputLayer);
        }

        /// <summary>
        /// Creates a clone with each weight varied according to a Gaussian of width standardDeviation
        /// </summary>
        public Perceptron RandomClone(double standardDeviation)
        {
            var clone = Clone();

            clone.RandomWeights(standardDeviation);

            return clone;
        }

        /// <summary>
        /// Gets the final output of the network
        /// </summary>
        public double GetOutput(int index)
        {
            return OutputLayer.GetOutput(index);
        }

        /// <summary>
        /// Gets all the outputs of the network
        /// </summary>
        public IEnumerable<double> GetOutputs()
        {
            for (int i = 0; i < OutputLayer.Nodes.Count; ++i)
            {
                yield return OutputLayer.GetOutput(i);
            }
        }

        /// <summary>
        /// Randomly fluctuates the weights by multiplying by a number distributed by a Gaussian
        /// </summary>
        public void RandomWeights(double standardDeviation)
        {
            foreach (var connector in Connectors)
            {
                connector.Weight *= Random.NextGaussian(0, standardDeviation);
            }
        }

        private const string header = "Perceptron - Hunger Games 2023";

        /// <summary>
        /// Writes the perceptron to file
        /// </summary>
        public void WriteToFile(string filename)
        {
            using var bw = new BinaryWriter(File.Create(filename));
            bw.Write(header);

            // Make the node dictionary
            var nodeDictionary = new Dictionary<Node, int>();
            int counter = 0;
            foreach (var node in Nodes)
            {
                nodeDictionary.Add(node, counter);
                ++counter;
            }

            // Write the nodes
            bw.Write(Nodes.Count());
            foreach (var node in Nodes)
            {
                node.Write(bw);
            }

            // Write the connectors
            bw.Write(Connectors.Count());
            foreach (var node in Nodes)
            {
                foreach (var connector in node.Connectors)
                {
                    connector.Write(bw, node, nodeDictionary);
                }
            }

            // Write the layers
            InputLayer.Write(bw, nodeDictionary);
            bw.Write(HiddenLayers.Count);
            foreach (var layer in HiddenLayers)
            {
                layer.Write(bw, nodeDictionary);
            }
            OutputLayer.Write(bw, nodeDictionary);
        }

        /// <summary>
        /// Reads the perceptron from a file
        /// </summary>
        static public Perceptron ReadFromFile(string filename)
        {
            using var br = new BinaryReader(File.OpenRead(filename));

            var checkHeader = br.ReadString();
            if (checkHeader != header)
            {
                throw new FormatException("Invalid perceptron format!");
            }

            // Read nodes
            int nNodes = br.ReadInt32();
            var nodeDictionary = new Dictionary<int, Node>();
            for (int i = 0; i < nNodes; ++i)
            {
                var newNode = new Node(br);
                nodeDictionary.Add(i, newNode);
            }

            // Read connectors
            int nConnectors = br.ReadInt32();
            for (int i = 0; i < nConnectors; ++i)
            {
                var newConnector = new Connector(br, nodeDictionary, out Node sourceNode);
                sourceNode.Connectors.Add(newConnector);
            }

            // Read layers
            var inputLayer = new Layer(br, nodeDictionary);
            var hiddenLayers = new List<Layer>();
            int nHiddenLayers = br.ReadInt32();
            for (int i = 0; i < nHiddenLayers; ++i)
            {
                hiddenLayers.Add(new Layer(br, nodeDictionary));
            }
            var outputLayer = new Layer(br, nodeDictionary);

            // Create perceptron
            return new Perceptron(inputLayer, hiddenLayers, outputLayer);
        }
    }
}
