namespace GraphData
{
    public class HybridGraphDataInterface(IGraphDataInterface fileInterface, IGraphDataInterface realTimeInterface) : IGraphDataInterface
    {
        public IEnumerable<IGraphPrototype> Graphs
        {
            get
            {
                foreach (var graph in fileInterface.Graphs)
                {
                    yield return graph;
                }
                foreach (var graph in realTimeInterface.Graphs)
                {
                    yield return graph;
                }
            }
        }

        public GraphDataPacket GetData()
        {
            return GraphDataPacket.Combine(fileInterface.GetData(), realTimeInterface.GetData());
        }
    }
}
