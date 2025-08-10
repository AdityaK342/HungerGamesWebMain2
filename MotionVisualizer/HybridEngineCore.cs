using GraphData;
using VisualizerBaseClasses;

namespace MotionVisualizer
{
    class HybridEngineCore<TVisualizer, TCommand> : EngineCore<TVisualizer, TCommand>
        where TCommand : ICommand<TVisualizer>
    {
        private FromFileEngineCore<TVisualizer, TCommand> fileCore;
        private RealTimeEngineCore<TVisualizer, TCommand> realTimeCore;

        public HybridEngineCore(string filename, ICommandFileReader<TVisualizer> factory, IEngine<TVisualizer, TCommand> engine, GraphDataManager manager)
        {
            fileCore = new FromFileEngineCore<TVisualizer, TCommand>(filename, factory);
            realTimeCore = new RealTimeEngineCore<TVisualizer, TCommand>(engine, manager);
        }

        public override bool Continue => shouldContinue;
        private bool shouldContinue = true;

        public override IGraphDataInterface Initialize(TVisualizer visualizer)
        {
            var fileResponse = fileCore.Initialize(visualizer);
            var realTimeResponse = realTimeCore.Initialize(visualizer);
            return new HybridGraphDataInterface(fileResponse, realTimeResponse);
        }

        public override PackagedCommands<TVisualizer>? NextCommand(double newTime)
        {
            // Note that we ignore newTime, since the time comes from the file

            var fileCommandSet = fileCore.NextCommand(newTime);
            if (fileCommandSet == null)
            {
                shouldContinue = false;
                return null;
            }
            double currentTime = fileCommandSet.Time;
            var realTimeCommandSet = realTimeCore.NextCommand(currentTime);
            return PackagedCommands<TVisualizer>.Combine(fileCommandSet, realTimeCommandSet);
        }
    }
}
