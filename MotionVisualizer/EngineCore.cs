using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    internal abstract class EngineCore<TVisualizer, TCommand>
        where TCommand : ICommand<TVisualizer>
    {
        public abstract IGraphDataInterface Initialize(TVisualizer visualizer);

        public abstract PackagedCommands<TVisualizer>? NextCommand(double newTime);

        public abstract bool Continue { get; }
    }
}
