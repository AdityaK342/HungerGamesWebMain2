using Arena;
using ArenaVisualizer;
using GraphControl;
using GraphData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HungerGames
{
    /// <summary>
    /// Interaction logic for HungerGamesTestWindow.xaml
    /// </summary>
    public partial class HungerGamesTestWindow : Window
    {
        private MainArenaVisualizer arena;

        public HungerGamesTestWindow(ArenaEngine engine)
        {
            var display = new ArenaVisualizerStandalone(engine);
            arena = new MainArenaVisualizer(engine, display);
            InitializeComponent();

            ArenaSpot.Content = arena.Content;        

            TimeIncrementSlider.Text = arena.TimeIncrement.ToString();

            ContentRendered += WireUpDisplay;
            ContentRendered += arena.LinkManager;

            arena.UpdatedTime += Arena_UpdatedTime;
        }

        private void Arena_UpdatedTime(object? sender, EventArgs e)
        {
            TimeText.Text = Math.Round(arena.DisplayTime, 2).ToString();
        }

        public GraphDataManager Manager => arena.Manager;

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.Current.Shutdown();
        }
        private void WireUpDisplay(object sender, EventArgs e)
        {
            arena.Visualizer = arena.Display;
            InvalidateVisual();
        }

        private void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (arena.IsRunning)
            {
                Start_Button.Content = "Resume";
                arena.IsRunning = false;
            }
            else
            {
                Start_Button.Content = "Pause";
                arena.IsRunning = true;

                arena.StartAll();
            }
        }

        private void TimeIncrementSlider_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TimeIncrementSlider.Text, out double result))
            {
                arena.TimeIncrement = result;
            }
        }
     

        private void SlowDrawCheckBox_Click (object sender, RoutedEventArgs e)
        {
            arena.SlowDraw = SlowDrawCheckBox.IsChecked == true;
        }
    }
}
