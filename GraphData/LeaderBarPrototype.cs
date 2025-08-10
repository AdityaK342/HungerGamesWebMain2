using System.Drawing;

namespace GraphData
{
    public class LeaderBarPrototype(string name, Color color)
    {
        public string Name { get; } = name;
        public Color Color { get; } = color;
    }
}
