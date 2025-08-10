namespace GraphData
{
    public class FileGraphDataInterface(BinaryReader br) : IGraphDataInterface
    {
        public IEnumerable<IGraphPrototype> Graphs
        {
            get
            {
                int nGraphs = br.ReadInt32();
                for (int i = 0; i < nGraphs; ++i)
                {
                    yield return IGraphPrototype.ReadFromFile(br);
                }
            }
        }

        public GraphDataPacket GetData()
        {
            return new GraphDataPacket(br);
        }
    }
}
