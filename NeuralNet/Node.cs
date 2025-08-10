namespace NeuralNet
{
    /// <summary>
    /// A node in a neural network
    /// </summary>
    public class Node
    {
        public List<Connector> Connectors { get; set; } = [];

        private double total = 0;

        public void Reset()
        {
            total = 0;
        }

        public void AddData(double value, double weight = 1)
        {
            total += value * weight;
        }

        public void FeedForward()
        {
            Connectors.ForEach((x) => x.Node.AddData(GetValue(), x.Weight));
        }

        public enum ActivationFunctionChoice
        {
            None, Bias, Step, Sigmoid, Tanh, RectifiedLinear, ParametricReLU, ExponentialLinear,
            Swish, GaussianError, OneOverX
        };

        public delegate double ActivationFunctionDelegate(double input);
        private readonly ActivationFunctionDelegate activationFunction;
        private readonly double parameter;
        public ActivationFunctionChoice ActivationFunction { get; init; }

        public Node(ActivationFunctionChoice activationFunction, double parameter = 0)
        {
            ActivationFunction = activationFunction;
            this.parameter = parameter;
            this.activationFunction = FunctionFromEnum(activationFunction, parameter);
        }

        static public ActivationFunctionDelegate FunctionFromEnum(ActivationFunctionChoice activationFunction, double parameter = 0)
        {
            return activationFunction switch
            {
                ActivationFunctionChoice.None => (x) => x,

                ActivationFunctionChoice.Bias => (x) => 1,

                ActivationFunctionChoice.Step => (x) => x < 0 ? 0 : 1,

                ActivationFunctionChoice.Sigmoid => (x) => 1 / (1 + Math.Exp(-x)),

                ActivationFunctionChoice.Tanh => (x) => Math.Tanh(x),

                ActivationFunctionChoice.RectifiedLinear => (x) => Math.Max(0, x),

                ActivationFunctionChoice.ParametricReLU => (x) => Math.Max(parameter * x, x),

                ActivationFunctionChoice.ExponentialLinear => (x) => x < 0 ? parameter * (Math.Exp(x) - 1) : x,

                ActivationFunctionChoice.Swish => (x) => x / (1 + Math.Exp(-x)),

                ActivationFunctionChoice.GaussianError => (x) => 0.5 * x * (1 + Math.Tanh(Math.Sqrt(2 / Math.PI)
                    * (x + 0.044715 * DongUtility.UtilityFunctions.Pow(x, 3)))),

                ActivationFunctionChoice.OneOverX => (x) => 1 / x,

                _ => throw new NotImplementedException("Impossible value of ActivationFunction called!"),
            };
        }

        public double GetValue()
        {
            return activationFunction(total);
        }

        public Node ConnectorlessClone()
        {
            return new Node(ActivationFunction, parameter);
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((int)ActivationFunction);
            bw.Write(parameter);
        }

        public Node(BinaryReader br)
        {
            int activationFunctionNumber = br.ReadInt32();
            ActivationFunction = (ActivationFunctionChoice)activationFunctionNumber;
            var parameter = br.ReadDouble();
            this.parameter = parameter;
            activationFunction = FunctionFromEnum(ActivationFunction, this.parameter);
        }
    }
}
