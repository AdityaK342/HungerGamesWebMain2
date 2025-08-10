using VisualizerBaseClasses;

namespace VisualizerControl
{
    /// <summary>
    /// Provides an interface for a kinematics engine or world class to work with the visualizer.
    /// This class should keep track of projectiles and forces and do all updates when told to.
    /// </summary>
    public interface IVisualization : IEngine<Visualizer, VisualizerCommand>
    {
    }
}
