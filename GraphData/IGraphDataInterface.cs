namespace GraphData
{
    public interface IGraphDataInterface
    {
        public GraphDataPacket GetData();
        public IEnumerable<IGraphPrototype> Graphs { get; }
    }
}
