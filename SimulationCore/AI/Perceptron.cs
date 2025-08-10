using SimulationCore.Math;

namespace SimulationCore.AI;

/// <summary>
/// A simple perceptron neural network for animal AI
/// </summary>
public class Perceptron
{
    private readonly float[][] weights;
    private readonly float[] biases;
    private readonly int[] layerSizes;
    private readonly Random random;

    public Perceptron(int[] layerSizes, Random? random = null)
    {
        this.layerSizes = layerSizes;
        this.random = random ?? new Random();
        
        // Initialize weights and biases
        weights = new float[layerSizes.Length - 1][];
        biases = new float[layerSizes.Length - 1];
        
        for (int i = 0; i < layerSizes.Length - 1; i++)
        {
            weights[i] = new float[layerSizes[i] * layerSizes[i + 1]];
            
            // Initialize with small random values
            for (int j = 0; j < weights[i].Length; j++)
            {
                weights[i][j] = (float)(this.random.NextDouble() * 2 - 1) * 0.5f;
            }
            
            biases[i] = (float)(this.random.NextDouble() * 2 - 1) * 0.1f;
        }
    }

    /// <summary>
    /// Run input through the network and get output
    /// </summary>
    public float[] Process(float[] inputs)
    {
        if (inputs.Length != layerSizes[0])
            throw new ArgumentException($"Expected {layerSizes[0]} inputs, got {inputs.Length}");

        var current = inputs;
        
        for (int layer = 0; layer < weights.Length; layer++)
        {
            var next = new float[layerSizes[layer + 1]];
            
            for (int output = 0; output < layerSizes[layer + 1]; output++)
            {
                float sum = biases[layer];
                
                for (int input = 0; input < layerSizes[layer]; input++)
                {
                    int weightIndex = input * layerSizes[layer + 1] + output;
                    sum += current[input] * weights[layer][weightIndex];
                }
                
                // Apply activation function (tanh for hidden layers, linear for output)
                next[output] = layer < weights.Length - 1 ? MathF.Tanh(sum) : sum;
            }
            
            current = next;
        }
        
        return current;
    }

    /// <summary>
    /// Create a copy of this perceptron with mutations
    /// </summary>
    public Perceptron Mutate(float mutationRate = 0.1f, float mutationStrength = 0.1f)
    {
        var mutated = new Perceptron(layerSizes, random);
        
        for (int layer = 0; layer < weights.Length; layer++)
        {
            for (int i = 0; i < weights[layer].Length; i++)
            {
                if (random.NextSingle() < mutationRate)
                {
                    mutated.weights[layer][i] = weights[layer][i] + 
                        (float)(random.NextDouble() * 2 - 1) * mutationStrength;
                }
                else
                {
                    mutated.weights[layer][i] = weights[layer][i];
                }
            }
            
            if (random.NextSingle() < mutationRate)
            {
                mutated.biases[layer] = biases[layer] + 
                    (float)(random.NextDouble() * 2 - 1) * mutationStrength;
            }
            else
            {
                mutated.biases[layer] = biases[layer];
            }
        }
        
        return mutated;
    }

    /// <summary>
    /// Save perceptron weights to a simple format
    /// </summary>
    public string Serialize()
    {
        var lines = new List<string>();
        lines.Add(string.Join(",", layerSizes));
        
        for (int layer = 0; layer < weights.Length; layer++)
        {
            lines.Add(string.Join(",", weights[layer]));
            lines.Add(biases[layer].ToString());
        }
        
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Load perceptron from serialized format
    /// </summary>
    public static Perceptron Deserialize(string data)
    {
        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) throw new ArgumentException("Invalid perceptron data");
        
        var layerSizes = lines[0].Split(',').Select(int.Parse).ToArray();
        var perceptron = new Perceptron(layerSizes);
        
        int lineIndex = 1;
        for (int layer = 0; layer < perceptron.weights.Length; layer++)
        {
            var weightValues = lines[lineIndex++].Split(',').Select(float.Parse).ToArray();
            for (int i = 0; i < weightValues.Length; i++)
            {
                perceptron.weights[layer][i] = weightValues[i];
            }
            
            perceptron.biases[layer] = float.Parse(lines[lineIndex++]);
        }
        
        return perceptron;
    }
} 