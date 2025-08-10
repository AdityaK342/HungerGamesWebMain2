using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    internal class RealTimeEngineCore<TVisualizer, TCommand> : EngineCore<TVisualizer, TCommand>
        where TCommand : ICommand<TVisualizer>
    {
        private readonly IEngine<TVisualizer, TCommand> engine;
        private readonly GraphDataManager manager;
        private RealTimeGraphDataInterface graphInterface;

        public RealTimeEngineCore(IEngine<TVisualizer, TCommand> engine, GraphDataManager manager)
        {
            this.engine = engine;
            this.manager = manager;
        }

        public override bool Continue => engine.Continue;

        public double Time => engine.Time;

        public override IGraphDataInterface Initialize(TVisualizer visualizer)
        {
            engine.Initialization().ProcessAll(visualizer);
            graphInterface = new RealTimeGraphDataInterface(manager);
            return graphInterface;
        }

        public override PackagedCommands<TVisualizer> NextCommand(double newTime)
        {
            var commands = engine.Tick(newTime);
            var data = graphInterface.GetData();
            return new PackagedCommands<TVisualizer>(commands, data, newTime);
        }
    }
}
