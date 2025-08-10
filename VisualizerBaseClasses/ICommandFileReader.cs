namespace VisualizerBaseClasses
{
    public interface ICommandFileReader<TVisualizer>
    {
        public ICommand<TVisualizer> ReadCommand(BinaryReader br);
    }
}
