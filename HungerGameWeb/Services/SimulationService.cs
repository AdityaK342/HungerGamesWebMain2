using Microsoft.JSInterop;
using SimulationCore.AI;
using SimulationCore.Arena;
using SimulationCore.Engine;
using SimulationCore.Math;
using System.Timers;

namespace HungerGameWeb.Services;

/// <summary>
/// Service that manages the simulation and provides data to UI components
/// </summary>
public class SimulationService : IDisposable
{
    private readonly IJSRuntime jsRuntime;
    private SimulationLoop? simulation;
    private System.Timers.Timer? timer;
    private bool disposed = false;

    public event Action<FrameData>? FrameUpdated;
    public event Action<SimulationStats>? StatsUpdated;

    public bool IsRunning => simulation?.IsRunning ?? false;
    public SimulationStats CurrentStats { get; private set; } = new();

    public SimulationService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initialize a new simulation
    /// </summary>
    public async Task InitializeSimulation(float width = 50f, float height = 50f, int? seed = null)
    {
        simulation = new SimulationLoop(width, height, seed);
        simulation.FrameUpdated += OnFrameUpdated;

        // Generate a simple arena with obstacles
        simulation.GenerateSimpleArena();

        // Add some hares and lynxes
        await AddDefaultAnimals();

        UpdateStats();
    }

    /// <summary>
    /// Add default animals to the simulation
    /// </summary>
    private async Task AddDefaultAnimals()
    {
        if (simulation == null) return;

        var random = simulation.Arena.Random;

        // Add hares with simple intelligence
        for (int i = 0; i < 20; i++)
        {
            var hareIntelligence = new HareIntelligence(random);
            simulation.AddHare($"Hare_{i}", hareIntelligence);
        }

        // Add lynxes with simple intelligence
        for (int i = 0; i < 5; i++)
        {
            var lynxIntelligence = new LynxIntelligence(random);
            simulation.AddLynx($"Lynx_{i}", lynxIntelligence);
        }
    }

    /// <summary>
    /// Start the simulation
    /// </summary>
    public async Task StartSimulation()
    {
        if (simulation == null) return;

        simulation.Start();
        
        // Set up timer for 60 FPS
        timer = new System.Timers.Timer(16.67); // ~60 FPS
        timer.Elapsed += OnTimerElapsed;
        timer.AutoReset = true;
        timer.Start();
    }

    /// <summary>
    /// Pause the simulation
    /// </summary>
    public async Task PauseSimulation()
    {
        simulation?.Pause();
        timer?.Stop();
    }

    /// <summary>
    /// Reset the simulation
    /// </summary>
    public async Task ResetSimulation()
    {
        timer?.Stop();
        simulation?.Reset();
        await InitializeSimulation();
        UpdateStats();
    }

    /// <summary>
    /// Set simulation speed multiplier
    /// </summary>
    public void SetSpeed(float speedMultiplier)
    {
        if (simulation != null)
        {
            simulation.DeltaTime = 0.01f * speedMultiplier;
        }
    }

    /// <summary>
    /// Set render options for the canvas
    /// </summary>
    public async Task SetRenderOptions(RenderOptions options)
    {
        await jsRuntime.InvokeVoidAsync("canvasInterop.setRenderOptions", options);
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (simulation?.IsRunning == true)
        {
            simulation.Step();
            
            // Check if simulation should end
            if (simulation.ShouldEnd())
            {
                _ = Task.Run(PauseSimulation);
            }
        }
    }

    private void OnFrameUpdated(FrameData frameData)
    {
        FrameUpdated?.Invoke(frameData);
        UpdateStats();
        
        // Update canvas
        _ = Task.Run(async () =>
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("canvasInterop.drawFrame", frameData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error drawing frame: {ex.Message}");
            }
        });
    }

    private void UpdateStats()
    {
        if (simulation == null) return;

        var totalHaresCaught = simulation.Arena.Lynxes.Sum(l => l.HaresCaught);

        CurrentStats = new SimulationStats
        {
            Time = simulation.Arena.Time,
            AliveHares = simulation.Arena.AliveHareCount,
            AliveLynxes = simulation.Arena.AliveLynxCount,
            TotalHaresCaught = totalHaresCaught,
            IsRunning = simulation.IsRunning
        };

        StatsUpdated?.Invoke(CurrentStats);
    }

    public void Dispose()
    {
        if (disposed) return;
        
        timer?.Dispose();
        simulation = null;
        disposed = true;
    }
}

/// <summary>
/// Current simulation statistics
/// </summary>
public record SimulationStats
{
    public float Time { get; init; }
    public int AliveHares { get; init; }
    public int AliveLynxes { get; init; }
    public int TotalHaresCaught { get; init; }
    public bool IsRunning { get; init; }
}

/// <summary>
/// Rendering options for the canvas
/// </summary>
public record RenderOptions
{
    public bool ShowVisionCones { get; init; }
    public bool ShowVelocityVectors { get; init; }
    public bool ShowIds { get; init; }
} 