using DongUtility;
using System.Drawing;

namespace GraphData
{
    public class TimelinePrototype
    {
        public string Name { get; }
        public Color Color { get; }

        public TimelinePrototype(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public void WriteToFile(BinaryWriter bw)
        {
            bw.Write(Name);
            bw.Write(Color);
        }

        internal TimelinePrototype(BinaryReader br)
        {
            Name = br.ReadString();
            Color = br.ReadColor();
        }
    }
}
