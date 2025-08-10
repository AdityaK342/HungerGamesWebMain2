using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HungerGames
{
    public class GraphStorage
    {
        private struct ColorList
        {
            public Color Color { get; set; }
            public Dictionary<int, int> Data { get; set; }
        }

        private Dictionary<string, ColorList> data = new Dictionary<string, ColorList>();

        public GraphStorage()
        { }

        public void AddAnimal(string animal, Color color)
        {
            data.Add(animal, new ColorList { Color = color, Data = new Dictionary<int, int>() });
        }

        public void AddDataPoint(string animal, int time, int number)
        {
            if (!data[animal].Data.ContainsKey(time))
            {
                data[animal].Data.Add(time, number);
            }
        }

        public void WriteToFile(string filename)
        {
            var bw = new BinaryWriter(File.Create(filename));

            bw.Write("Hunger Games graph data storage v1");

            bw.Write(data.Count);
            foreach (var dat in data)
            {
                bw.Write(dat.Key);
                var color = dat.Value.Color;
                bw.Write(color.R);
                bw.Write(color.G);
                bw.Write(color.B);

                bw.Write(dat.Value.Data.Count);
                foreach (var value in dat.Value.Data)
                {
                    bw.Write(value.Key);
                    bw.Write(value.Value);
                }
            }
        }

        public GraphStorage(string filename)
        {
            var br = new BinaryReader(File.OpenRead(filename));
            string header = br.ReadString();

            if (header != "Hunger Games graph data storage v1")
            {
                throw new FileFormatException("Wrong file format passed to GraphStorage");
            }

            int count = br.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                string key = br.ReadString();
                Color color = new Color();
                color.R = br.ReadByte();
                color.G = br.ReadByte();
                color.B = br.ReadByte();

                AddAnimal(key, color);

                var dataCount = br.ReadInt32();
                for (int j = 0; j < dataCount; ++j)
                {
                    int time = br.ReadInt32();
                    int number = br.ReadInt32();
                    AddDataPoint(key, time, number);
                }
            }
        }

        public int GetDataPoint(string animal, int time)
        {
            return data[animal].Data[time];
        }
    }
}
