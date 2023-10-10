using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            setup();
        }

        private void setup()
        {
            gameWidth = ((int)MainCanvas.Width) / cellSize;
            gameHeight = ((int)MainCanvas.Height) / cellSize;
            gameBoard = new bool[gameWidth * gameHeight]; // Initialize backend array to proper size
            evolutions = 0;

            randomlyPopulate();
            drawFromArray();
        }

        // Graphical methohds
        private int drawFromArray() // Draws all living cells, and return number of alive cells
        {
            int aliveCells = 0;
            MainCanvas.Children.Clear();
            MainCanvas.Background = deadColor;
            for (int i = 0; i < gameWidth * gameHeight; i++)
            {
                if (gameBoard[i])
                {
                    drawCell(i % gameWidth, i / gameWidth);
                    aliveCells++;
                }
            }
            AliveCells_Count.Content = aliveCells;
            PercentLive_Count.Content = Math.Round((double)aliveCells * 100/(double)(gameWidth * gameHeight), 2);
            return aliveCells;
        }
        private bool drawCell(int x, int y)
        {
            if (x < 0 || x > MainCanvas.Width - 1 || y < 0 || y > MainCanvas.Height - 1){
                return false;
            }

            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Fill = aliveColor;
            rect.Width = rect.Height = cellSize;
            Canvas.SetLeft(rect, x * cellSize);
            Canvas.SetTop(rect, y * cellSize);
            MainCanvas.Children.Add(rect);
            return true;
        }

        // Gamerule methods
        private void applyRules()
        {
            Dictionary<int, bool> changes = new Dictionary<int, bool>();
            for(int i = 0; i < gameWidth * gameHeight; i++)
            {
                int neighbors = getNeighbors(i);
                if (gameBoard[i]) // For alive cells
                {
                    if (neighbors < 2 || neighbors > 3)
                    {
                        changes.Add(i, false);
                    }
                }
                else // For dead cells
                {
                    if(neighbors == 3)
                    {
                        changes.Add(i, true);
                    }
                }
            }

            foreach(KeyValuePair<int, bool> change in changes)
            {
                gameBoard[change.Key] = change.Value;
            }
            evolutions++;
        }
        private int getNeighbors(int i)
        {
            int x = i % gameWidth;
            int y = i / gameWidth;
            int neighbors = 0;

            if(x > 0) // Has left neighbors
            {
                if (gameBoard[i - 1])
                    neighbors++;
                if (y > 0 && gameBoard[i - 1 - gameWidth])
                    neighbors++;
                if (y < gameHeight - 1 && gameBoard[i - 1 + gameWidth])
                    neighbors++;
            }
            if (x < gameWidth - 1) // Has right neighbors
            {
                if (gameBoard[i + 1])
                    neighbors++;
                if (y > 0 && gameBoard[i + 1 - gameWidth])
                    neighbors++;
                if (y < gameHeight - 1 && gameBoard[i + 1 + gameWidth])
                    neighbors++;
            }
            if (y > 0 && gameBoard[i - gameWidth])
                neighbors++;
            if (y < gameHeight - 1 && gameBoard[i + gameWidth])
                neighbors++;

            return neighbors;
        }
        public void startSimulation()
        {
            updateTimer = new System.Windows.Threading.DispatcherTimer();
            updateTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            updateTimer.Interval = TimeSpan.FromSeconds(1.0/(double)ticksPerSecond);
            updateTimer.Start();
        }

        // Util methods
        private void randomlyPopulate(int density = 30)
        {
            Random rnd = new Random();
            for (int i = 0; i < gameWidth * gameHeight; i++)
            {
                gameBoard[i] = rnd.Next(0,100) < density;
            }

        }

        private SolidColorBrush colorFromString(string color)
        {
            switch (color)
            {
                case "Black":
                    return new SolidColorBrush(Colors.Black);
                case "White":
                    return new SolidColorBrush(Colors.White);
                case "Red":
                    return new SolidColorBrush(Colors.Red);
                case "Blue":
                    return new SolidColorBrush(Colors.Blue);
                case "Green":
                    return new SolidColorBrush(Colors.Green);
                case "Yellow":
                    return new SolidColorBrush(Colors.Yellow);
            }
            return null;
        }

        private bool[] gameBoard;
        private int gameWidth, gameHeight, evolutions;
        private int ticksPerSecond = 5;
        private int cellSize = 5; // Pixel size of game cells
        private bool isRunning = false;
        private SolidColorBrush aliveColor = new SolidColorBrush(Colors.Black);
        private SolidColorBrush deadColor = new SolidColorBrush(Colors.White);
        private DispatcherTimer updateTimer;

        // Signals
        private void Simulation_Begin_Button(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                startSimulation();
                Apply_Button.IsEnabled = false;
                Begin_Button.Content = "Restart Simulation";
                isRunning = true;
                Stop_Button.IsEnabled = true;
            }
            else
            {
                setup();
            }
        }

        private void Simulation_Stop_Button(object sender, RoutedEventArgs e)
        {
            updateTimer.Stop();
            isRunning = false;
            Stop_Button.IsEnabled = false;
            Begin_Button.Content = "Begin Simulation";
            Apply_Button.IsEnabled = true;
        }

        private void ApplySettings_Button(object sender, RoutedEventArgs e)
        {
            ticksPerSecond = int.Parse(UpdateDelta_Textbox.Text);
            cellSize = int.Parse(CellSize_Textbox.Text);
            gameWidth = ((int)MainCanvas.Width) / cellSize;
            gameHeight = ((int)MainCanvas.Height) / cellSize;
            gameBoard = new bool[gameWidth * gameHeight]; // Initialize backend array to proper size
            evolutions = 0;
            aliveColor = colorFromString(LiveColor_CBox.Text);
            deadColor = colorFromString(DeadColor_CBox.Text);
            randomlyPopulate();
            drawFromArray();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) // Update ran every game tick
        {
            applyRules();
            Evolution_Count.Content = evolutions.ToString();
            drawFromArray();
        }

    }

}
