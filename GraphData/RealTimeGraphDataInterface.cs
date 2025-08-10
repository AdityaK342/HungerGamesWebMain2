namespace GraphData
{
    public class RealTimeGraphDataInterface(GraphDataManager manager) : IGraphDataInterface
    {
        public GraphDataManager Manager { get; } = manager;
        public IEnumerable<IGraphPrototype> Graphs => Manager.Graphs;

        public GraphDataPacket GetData() => Manager.GetData();
    }
}
