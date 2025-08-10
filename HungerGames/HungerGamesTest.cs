using Arena;
using ArenaVisualizer;
using DongUtility;
using GraphData;
using HungerGames.Animals;
using HungerGames.Interface;
using HungerGamesCore.Interface;
using NeuralNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungerGames
{
    // This is an example of a custom choose that you can use to train different animals against each other
    class CustomChooser : LocationChooserTemplateIntermediate<HarePerceptronIntelligenceAditya, LynxIntelligenceDefault>
    { }

    class HungerGamesTest
    {
        private const double hareToLynxRatio = 10;

        private const int nLynx = 10;
        private const int nHare = (int)(nLynx * hareToLynxRatio);

        private const int arenaHeight = 50;
        private const int arenaWidth = 50;

        private const double maxTime = 100;
        private const double timeStep = .01;

        public static void Run()
        {
            WPFUtility.ConsoleManager.ShowConsoleWindow();
          // TrainPerceptrons();
            Display();
        }

        public static void Display()
        {
            var arena = new HungerGamesArena(arenaWidth, arenaHeight);

            var master = new GameMaster(arena);

            //master.AddChooser(new CustomChooser());
            master.AddChooser(new ChooserAditya());
            master.AddChooser(new ChooserDefault());

            master.AddAllAnimals(nHare, nLynx);

            var sim = new HungerGamesTestWindow(arena);

            sim.Manager.AddLeaderBoard(GetLeaderBars(master, true),
                () => GetLeaderBoardScores(arena, master));
            sim.Manager.AddLeaderBoard(GetLeaderBars(master, false),
                () => GetLynxScores(arena, master));

            sim.Show();
        }

        public static void TrainPerceptrons()
        {
            /* ── configuration ── */
            const int INPUTS = 15;
            const int OUTPUTS = 2;
            var hidden = new List<int> { 18, 12 };

            const int MAX_TRIES = 400;   // brute-force pool
            const int TARGET_SURV = 75;    // stop early if reached

            double bestFit = double.MinValue;
            int bestSurv = 0;
            Perceptron bestHare = null;

            double Fitness(double t, int s) => s * 2000 + t * 10;

            (double t, int s) SafeRun(Perceptron hare, Perceptron lynx)
            {
                try
                {
                    var tup = RunArena(hare, lynx);      // System.Tuple<double,int>
                    return (tup.Item1, tup.Item2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"•  Trial skipped: {ex.Message}");
                    return (-1, 0);                      // signals “bad run”
                }
            }

            var rng = new Random();

            for (int i = 0; i < MAX_TRIES; ++i)
            {
                /* ---- build random hare net ---- */
                var hare = new Perceptron(INPUTS, OUTPUTS, hidden,
                                          Node.ActivationFunctionChoice.Tanh);
                hare.RandomWeights(5);

                /* ---- fresh dummy lynx each trial ---- */
                var lynx = new Perceptron(4, 2);
                lynx.RandomWeights(5);

                var (time, survivors) = SafeRun(hare, lynx);

                if (time < 0 || double.IsNaN(time) || double.IsInfinity(time))
                    continue;                            // bad trial → skip

                double fit = Fitness(time, survivors);

                if (fit > bestFit)
                {
                    bestFit = fit;
                    bestSurv = survivors;
                    bestHare = hare;

                    Console.WriteLine($"▶ new best  |  {survivors} survivors  |  t={time:F1}s");

                    if (survivors >= TARGET_SURV)
                        break;                           // early success
                }
            }

            /* ---- save results ---- */
            string dir = FileUtilities.GetMainProjectDirectory() + "Perceptrons/";
            bestHare?.WriteToFile(dir + "AdityaHarePerceptron_Final.pcp");

            Console.WriteLine($"\n✔ Finished search – best run had {bestSurv} survivors.");
        }

        public static Tuple<double, int> RunArena(Perceptron harePerceptron, Perceptron lynxPerceptron)
        {
            var arena = new HungerGamesArena(arenaWidth, arenaHeight);

            var master = new GameMaster(arena);

            //master.AddChooser(new CustomChooser());
            master.AddChooser(new ChooserAditya());
            master.AddChooser(new ChooserDefault());

            master.AddAllAnimals(nHare, nLynx, harePerceptron, lynxPerceptron);

            var hareName = master.Choosers[0].GetName(true);

            while (arena.Continue && arena.Time < maxTime)
            {
                arena.Tick(arena.Time + timeStep);
            }

            Console.WriteLine($"Time: {arena.Time}  Hares remaining: {arena.GetObjects(hareName).Count()}");
            return new Tuple<double, int>(arena.Time, arena.GetObjects(hareName).Count());
        }

        static private List<LeaderBarPrototype> GetLeaderBars(GameMaster gm, bool hare)
        {
            var leaderBars = new List<LeaderBarPrototype>();
            foreach (var chooser in gm.Choosers)
            {
                var color = chooser.MakeOrganism(null, hare).Color;
                var bar = new LeaderBarPrototype(chooser.GetName(hare), color);
                leaderBars.Add(bar);
            }
            return leaderBars;
        }

        static private List<double> GetLeaderBoardScores(ArenaEngine arena, GameMaster gm)
        {
            var data = new List<double>();
            foreach (var chooser in gm.Choosers)
            {
                var list = arena.GetObjects(chooser.GetName(true));
                data.Add(list.Count());
            }
            return data;
        }

        static private List<double> GetLynxScores(ArenaEngine arena, GameMaster gm)
        {
            var data = new List<double>();
            foreach (var chooser in gm.Choosers)
            {
                var sum = ((HungerGamesArena)arena).GetHaresEaten(chooser.GetName(false));
                data.Add(sum);
            }
            return data;
        }
    }
}