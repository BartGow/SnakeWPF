using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using AForge.Neuro;
using System.IO;
using Microsoft.Win32;

namespace WPFSnake
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int size = 20;
        static Rectangle[,] fields = new Rectangle[size, size];
        static Random r = new Random();
        static Snake actualSnake;
        static Snake2 actualSnake2;
        static int it = 0;
        static Population p;
        static int maxEpochs = 100;
        static int actualEpoch = 0;
        static List<String> wyniki = new List<string>();
        static System.Windows.Threading.DispatcherTimer dispatcherTimer;
        static bool Draw = false;
        public bool firstBrain = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            InitializeBoard();
            InitializePopulation();
            InitializeTimer();
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < size; i++)
            {
                RowDefinition row = new RowDefinition();
                Board.RowDefinitions.Add(row);
            }
            for (int i = 0; i < size; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                Board.ColumnDefinitions.Add(column);
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Rectangle r = new Rectangle();
                    fields[i, j] = r;
                    Board.Children.Add(r);
                    r.Fill = Brushes.Black;
                    Grid.SetRow(r, i);
                    Grid.SetColumn(r, j);
                }
            }
        }
        private void InitializeTimer()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1 / 100000);
        }
        private void InitializeSnakes()
        {
            for (int i = 0; i < p.size; i++)
            {
                p.snakes.Add(new Snake());
            }
            actualSnake = p.snakes[it];
            p.BestOverall = p.snakes[0];
        }
        private void InitializePopulation()
        {
            p = new Population();
            p.size = int.Parse(PopBOX.Text);
        }
        private void InitializeSecondPopulation()
        {
            p = new Population(p.BestOverall);
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (firstBrain)
            {
                if (actualSnake.alive)
                {
                    Play();
                }
                else
                {
                    actualSnake.CalculateFitness();
                    ActBlock.Text = actualSnake.fitness.ToString();
                    if (it < p.size)
                    {
                        actualSnake = p.snakes[it++];
                    }
                    else
                    {
                        if (actualEpoch < maxEpochs)
                        {
                            double max = p.BestSnake.fitness;
                            double maxFood = p.BestSnake.eatenFood;
                            EpochBlock.Text = (actualEpoch + 1).ToString();
                            MeanBlock.Text = Math.Truncate(p.FitnessAVG).ToString();
                            MaxBlock.Text = max.ToString();
                            wyniki.Add(String.Format(actualEpoch + ". AVG:" + Math.Truncate(p.FitnessAVG) + " MAX: " + max + " Zjedzone: " + maxFood));
                            if (p.BestSnake.fitness > p.BestOverall.fitness)
                            {
                                p.BestOverall = new Snake(p.BestSnake);
                            }
                            p.UseGeneticOperations();
                            it = 0;
                            actualSnake = p.snakes[it];
                            actualEpoch++;
                        }
                        else
                        {
                            //SaveResult();
                            Draw = true;
                            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);
                            PlayBest();
                            //MessageBox.Show(String.Format("Zjedzone: " + p.BestOverall.eatenFood));
                            //SaveWinner();

                            //firstBrain = false;
                            //it = 0;
                            //actualEpoch = 0;
                            //InitializeSecondPopulation();
                            //actualSnake2 = p.snakes2[it];
                            //p.BestOverall2 = p.snakes2[0];

                        }
                    }
                }
            }
            else //Kiedy mamy wyuczony pierwszy mózg...
            {
                if (actualSnake2.alive)
                {
                    Play2();
                }
                else
                {
                    actualSnake2.CalculateFitness();
                    ActBlock.Text = actualSnake2.fitness.ToString();
                    if (it < p.size)
                    {
                        actualSnake2 = p.snakes2[it++];
                    }
                    else
                    {
                        if (actualEpoch < maxEpochs)
                        {
                            double max = p.BestSnake2.fitness;
                            double maxFood = p.BestSnake2.eatenFood / Snake.howManyLives;
                            EpochBlock.Text = (actualEpoch + 1).ToString();
                            MeanBlock.Text = Math.Truncate(p.FitnessAVG2).ToString();
                            MaxBlock.Text = max.ToString();
                            wyniki.Add(String.Format(actualEpoch + ". AVG:" + Math.Truncate(p.FitnessAVG2) + " MAX: " + max + " Zjedzone: " + maxFood));
                            if (p.BestSnake2.fitness > p.BestOverall2.fitness)
                            {
                                p.BestOverall2 = new Snake2(p.BestSnake2);
                            }
                            p.UseGeneticOperations2();
                            it = 0;
                            actualSnake2 = p.snakes2[it];
                            actualEpoch++;
                        }
                        else
                        {
                            //SaveResult();
                            Draw = true;
                            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);
                            PlayBest2();
                            MessageBox.Show(String.Format("Zjedzone: " + p.BestOverall2.eatenFood));
                            //SaveWinner();
                        }
                    }
                }
            }
        }

        private void Play()
        {
            if (Draw)
            {
                DrawBoard();
                DrawFood();
                DrawSnake();
            }
            actualSnake.Look();
            actualSnake.SetVelocity3();
            if (Draw) { DrawBrain(); }
            actualSnake.Move();
        }
        private void Play2()
        {
            if (Draw)
            {
                DrawBoard();
                DrawFood2();
                DrawSnake2();
            }
            actualSnake2.Look();
            actualSnake2.SetVelocity3();
            if (Draw) { DrawBrain2(); }
            actualSnake2.Move();
        }

        private void DrawBoard()
        {
            foreach (Rectangle item in fields)
            {
                item.Fill = Brushes.Black;
            }
        }
        private void DrawFood()
        {
            fields[actualSnake.foodPosition.Y, actualSnake.foodPosition.X].Fill = Brushes.Green;
        }
        private void DrawSnake()
        {
            fields[actualSnake.headPosition.Y, actualSnake.headPosition.X].Fill = Brushes.White;
            for (int i = 0; i < actualSnake.tailPosition.Count; i++)
            {
                fields[actualSnake.tailPosition[i].Y, actualSnake.tailPosition[i].X].Fill = Brushes.White;
            }
        }
        private void DrawFood2()
        {
            fields[actualSnake2.foodPosition.Y, actualSnake2.foodPosition.X].Fill = Brushes.Green;
        }
        private void DrawSnake2()
        {
            fields[actualSnake2.headPosition.Y, actualSnake2.headPosition.X].Fill = Brushes.White;
            for (int i = 0; i < actualSnake2.tailPosition.Count; i++)
            {
                fields[actualSnake2.tailPosition[i].Y, actualSnake2.tailPosition[i].X].Fill = Brushes.White;
            }
        }
        private void DrawBrain()
        {
            if (actualSnake.vision[0] < 0)
            {
                Neuro1.Fill = Brushes.Blue;
                Neuro1.Opacity = Math.Abs(actualSnake.vision[0]);
            }
            else
            {
                Neuro1.Fill = Brushes.Red;
                Neuro1.Opacity = Math.Abs(actualSnake.vision[0]);
            }

            if (actualSnake.vision[1] < 0)
            {
                Neuro2.Fill = Brushes.Blue;
                Neuro2.Opacity = Math.Abs(actualSnake.vision[1]);
            }
            else
            {
                Neuro2.Fill = Brushes.Red;
                Neuro2.Opacity = Math.Abs(actualSnake.vision[1]);
            }

            if (actualSnake.vision[2] < 0)
            {
                Neuro3.Fill = Brushes.Blue;
                Neuro3.Opacity = Math.Abs(actualSnake.vision[2]);
            }
            else
            {
                Neuro3.Fill = Brushes.Red;
                Neuro3.Opacity = Math.Abs(actualSnake.vision[2]);
            }

            if (actualSnake.vision[3] < 0)
            {
                Neuro4.Fill = Brushes.Blue;
                Neuro4.Opacity = Math.Abs(actualSnake.vision[3]);
            }
            else
            {
                Neuro4.Fill = Brushes.Red;
                Neuro4.Opacity = Math.Abs(actualSnake.vision[3]);
            }

            if (actualSnake.vision[4] < 0)
            {
                Neuro5.Fill = Brushes.Blue;
                Neuro5.Opacity = Math.Abs(actualSnake.vision[4]);
            }
            else
            {
                Neuro5.Fill = Brushes.Red;
                Neuro5.Opacity = Math.Abs(actualSnake.vision[4]);
            }

            if (actualSnake.vision[5] < 0)
            {
                Neuro6.Fill = Brushes.Blue;
                Neuro6.Opacity = Math.Abs(actualSnake.vision[5]);
            }
            else
            {
                Neuro6.Fill = Brushes.Red;
                Neuro6.Opacity = Math.Abs(actualSnake.vision[5]);
            }

            if (actualSnake.vision[6] < 0)
            {
                Neuro7.Fill = Brushes.Blue;
                Neuro7.Opacity = Math.Abs(actualSnake.vision[6]);
            }
            else
            {
                Neuro7.Fill = Brushes.Red;
                Neuro7.Opacity = Math.Abs(actualSnake.vision[6]);
            }

            if (actualSnake.vision[7] < 0)
            {
                Neuro8.Fill = Brushes.Blue;
                Neuro8.Opacity = Math.Abs(actualSnake.vision[7]);
            }
            else
            {
                Neuro8.Fill = Brushes.Red;
                Neuro8.Opacity = Math.Abs(actualSnake.vision[7]);
            }

            if (actualSnake.vision[8] < 0)
            {
                Neuro9.Fill = Brushes.Blue;
                Neuro9.Opacity = Math.Abs(actualSnake.vision[8]);
            }
            else
            {
                Neuro9.Fill = Brushes.Red;
                Neuro9.Opacity = Math.Abs(actualSnake.vision[8]);
            }

            if (actualSnake.vision[9] < 0)
            {
                Neuro10.Fill = Brushes.Blue;
                Neuro10.Opacity = Math.Abs(actualSnake.vision[9]);
            }
            else
            {
                Neuro10.Fill = Brushes.Red;
                Neuro10.Opacity = Math.Abs(actualSnake.vision[9]);
            }

            if (actualSnake.vision[10] < 0)
            {
                Neuro11.Fill = Brushes.Blue;
                Neuro11.Opacity = Math.Abs(actualSnake.vision[10]);
            }
            else
            {
                Neuro11.Fill = Brushes.Red;
                Neuro11.Opacity = Math.Abs(actualSnake.vision[10]);
            }

            if (actualSnake.vision[11] < 0)
            {
                Neuro12.Fill = Brushes.Blue;
                Neuro12.Opacity = Math.Abs(actualSnake.vision[11]);
            }
            else
            {
                Neuro12.Fill = Brushes.Red;
                Neuro12.Opacity = Math.Abs(actualSnake.vision[11]);
            }


            if (actualSnake.brain.Output[0] < 0)
            {
                Neuro13.Fill = Brushes.Blue;
                Neuro13.Opacity = Math.Abs(actualSnake.brain.Output[0]);
            }
            else
            {
                Neuro13.Fill = Brushes.Red;
                Neuro13.Opacity = Math.Abs(actualSnake.brain.Output[0]);
            }

            if (actualSnake.brain.Output[1] < 0)
            {
                Neuro14.Fill = Brushes.Blue;
                Neuro14.Opacity = Math.Abs(actualSnake.brain.Output[1]);
            }
            else
            {
                Neuro14.Fill = Brushes.Red;
                Neuro14.Opacity = Math.Abs(actualSnake.brain.Output[1]);
            }

            if (actualSnake.brain.Output[2] < 0)
            {
                Neuro15.Fill = Brushes.Blue;
                Neuro15.Opacity = Math.Abs(actualSnake.brain.Output[2]);
            }
            else
            {
                Neuro15.Fill = Brushes.Red;
                Neuro15.Opacity = Math.Abs(actualSnake.brain.Output[2]);
            }

            if (actualSnake.brain.Output[3] < 0)
            {
                Neuro16.Fill = Brushes.Blue;
                Neuro16.Opacity = Math.Abs(actualSnake.brain.Output[3]);
            }
            else
            {
                Neuro16.Fill = Brushes.Red;
                Neuro16.Opacity = Math.Abs(actualSnake.brain.Output[3]);
            }

        }
        private void DrawBrain2()
        {
            if (actualSnake2.vision[0] < 0)
            {
                Neuro1.Fill = Brushes.Blue;
                Neuro1.Opacity = Math.Abs(actualSnake2.vision[0]);
            }
            else
            {
                Neuro1.Fill = Brushes.Red;
                Neuro1.Opacity = Math.Abs(actualSnake2.vision[0]);
            }

            if (actualSnake2.vision[1] < 0)
            {
                Neuro2.Fill = Brushes.Blue;
                Neuro2.Opacity = Math.Abs(actualSnake2.vision[1]);
            }
            else
            {
                Neuro2.Fill = Brushes.Red;
                Neuro2.Opacity = Math.Abs(actualSnake2.vision[1]);
            }

            if (actualSnake2.vision[2] < 0)
            {
                Neuro3.Fill = Brushes.Blue;
                Neuro3.Opacity = Math.Abs(actualSnake2.vision[2]);
            }
            else
            {
                Neuro3.Fill = Brushes.Red;
                Neuro3.Opacity = Math.Abs(actualSnake2.vision[2]);
            }

            if (actualSnake2.vision[3] < 0)
            {
                Neuro4.Fill = Brushes.Blue;
                Neuro4.Opacity = Math.Abs(actualSnake2.vision[3]);
            }
            else
            {
                Neuro4.Fill = Brushes.Red;
                Neuro4.Opacity = Math.Abs(actualSnake2.vision[3]);
            }

            if (actualSnake2.vision[4] < 0)
            {
                Neuro5.Fill = Brushes.Blue;
                Neuro5.Opacity = Math.Abs(actualSnake2.vision[4]);
            }
            else
            {
                Neuro5.Fill = Brushes.Red;
                Neuro5.Opacity = Math.Abs(actualSnake2.vision[4]);
            }

            if (actualSnake2.vision[5] < 0)
            {
                Neuro6.Fill = Brushes.Blue;
                Neuro6.Opacity = Math.Abs(actualSnake2.vision[5]);
            }
            else
            {
                Neuro6.Fill = Brushes.Red;
                Neuro6.Opacity = Math.Abs(actualSnake2.vision[5]);
            }

            if (actualSnake2.vision[6] < 0)
            {
                Neuro7.Fill = Brushes.Blue;
                Neuro7.Opacity = Math.Abs(actualSnake2.vision[6]);
            }
            else
            {
                Neuro7.Fill = Brushes.Red;
                Neuro7.Opacity = Math.Abs(actualSnake2.vision[6]);
            }

            if (actualSnake2.vision[7] < 0)
            {
                Neuro8.Fill = Brushes.Blue;
                Neuro8.Opacity = Math.Abs(actualSnake2.vision[7]);
            }
            else
            {
                Neuro8.Fill = Brushes.Red;
                Neuro8.Opacity = Math.Abs(actualSnake2.vision[7]);
            }

            if (actualSnake2.vision[8] < 0)
            {
                Neuro9.Fill = Brushes.Blue;
                Neuro9.Opacity = Math.Abs(actualSnake2.vision[8]);
            }
            else
            {
                Neuro9.Fill = Brushes.Red;
                Neuro9.Opacity = Math.Abs(actualSnake2.vision[8]);
            }

            if (actualSnake2.vision[9] < 0)
            {
                Neuro10.Fill = Brushes.Blue;
                Neuro10.Opacity = Math.Abs(actualSnake2.vision[9]);
            }
            else
            {
                Neuro10.Fill = Brushes.Red;
                Neuro10.Opacity = Math.Abs(actualSnake2.vision[9]);
            }

            if (actualSnake2.vision[10] < 0)
            {
                Neuro11.Fill = Brushes.Blue;
                Neuro11.Opacity = Math.Abs(actualSnake2.vision[10]);
            }
            else
            {
                Neuro11.Fill = Brushes.Red;
                Neuro11.Opacity = Math.Abs(actualSnake2.vision[10]);
            }

            if (actualSnake2.vision[11] < 0)
            {
                Neuro12.Fill = Brushes.Blue;
                Neuro12.Opacity = Math.Abs(actualSnake2.vision[11]);
            }
            else
            {
                Neuro12.Fill = Brushes.Red;
                Neuro12.Opacity = Math.Abs(actualSnake2.vision[11]);
            }


            if (actualSnake2.brain.Output[0] < 0)
            {
                Neuro13.Fill = Brushes.Blue;
                Neuro13.Opacity = Math.Abs(actualSnake2.brain.Output[0]);
            }
            else
            {
                Neuro13.Fill = Brushes.Red;
                Neuro13.Opacity = Math.Abs(actualSnake2.brain.Output[0]);
            }

            if (actualSnake2.brain.Output[1] < 0)
            {
                Neuro14.Fill = Brushes.Blue;
                Neuro14.Opacity = Math.Abs(actualSnake2.brain.Output[1]);
            }
            else
            {
                Neuro14.Fill = Brushes.Red;
                Neuro14.Opacity = Math.Abs(actualSnake2.brain.Output[1]);
            }

            if (actualSnake2.brain.Output[2] < 0)
            {
                Neuro15.Fill = Brushes.Blue;
                Neuro15.Opacity = Math.Abs(actualSnake2.brain.Output[2]);
            }
            else
            {
                Neuro15.Fill = Brushes.Red;
                Neuro15.Opacity = Math.Abs(actualSnake2.brain.Output[2]);
            }

            if (actualSnake2.brain.Output[3] < 0)
            {
                Neuro16.Fill = Brushes.Blue;
                Neuro16.Opacity = Math.Abs(actualSnake2.brain.Output[3]);
            }
            else
            {
                Neuro16.Fill = Brushes.Red;
                Neuro16.Opacity = Math.Abs(actualSnake2.brain.Output[3]);
            }

        }

        private void SaveResult()
        {
            StreamWriter sw = new StreamWriter("wyniki.txt");
            for (int i = 0; i < wyniki.Count; i++)
            {
                sw.WriteLine(wyniki[i]);
            }
            sw.Flush();
            sw.Close();
        }
        private void SaveWinner(string plik)
        {
            StreamWriter sw = new StreamWriter(plik);
            for (int i = 0; i < p.BestOverall.brain.Layers.Count(); i++)
            {
                for (int j = 0; j < p.BestOverall.brain.Layers[i].Neurons.Count(); j++)
                {
                    sw.Write((p.BestOverall.brain.Layers[i].Neurons[j] as ActivationNeuron).Threshold);
                    for (int k = 0; k < p.BestOverall.brain.Layers[i].Neurons[j].Weights.Length; k++)
                    {
                        sw.Write(";" + p.BestOverall.brain.Layers[i].Neurons[j].Weights[k]);
                    }
                    sw.WriteLine();
                }
                sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
        }
        private void SaveWinner2(string plik)
        {
            StreamWriter sw = new StreamWriter(plik);
            for (int i = 0; i < p.BestOverall2.brain.Layers.Count(); i++)
            {
                for (int j = 0; j < p.BestOverall2.brain.Layers[i].Neurons.Count(); j++)
                {
                    sw.Write((p.BestOverall2.brain.Layers[i].Neurons[j] as ActivationNeuron).Threshold);
                    for (int k = 0; k < p.BestOverall2.brain.Layers[i].Neurons[j].Weights.Length; k++)
                    {
                        sw.Write(";" + p.BestOverall2.brain.Layers[i].Neurons[j].Weights[k]);
                    }
                    sw.WriteLine();
                }
                sw.WriteLine();
            }
            sw.WriteLine("---");
            for (int i = 0; i < p.BestOverall2.brain2.Layers.Count(); i++)
            {
                for (int j = 0; j < p.BestOverall2.brain2.Layers[i].Neurons.Count(); j++)
                {
                    sw.Write((p.BestOverall2.brain2.Layers[i].Neurons[j] as ActivationNeuron).Threshold);
                    for (int k = 0; k < p.BestOverall2.brain2.Layers[i].Neurons[j].Weights.Length; k++)
                    {
                        sw.Write(";" + p.BestOverall2.brain2.Layers[i].Neurons[j].Weights[k]);
                    }
                    sw.WriteLine();
                }
                sw.WriteLine();
            }
            sw.Flush();
            sw.Close();
        }
        private void LoadWinner2(string plik)
        {
            actualEpoch = maxEpochs;
            it = p.size;
            string s = File.ReadAllText(plik);
            List<string> s2 = s.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            string[] s3;
            double[] d;
            ActivationNetwork brain = new ActivationNetwork(new BipolarSigmoidFunction(), 12, 8, 4);

            int row = 0;
            for (int i = 0; i < brain.Layers.Length; i++)
            {
                for (int j = 0; j < brain.Layers[i].Neurons.Length; j++)
                {
                    s3 = s2[row].Split(';'); //bierzemy wiersz
                    d = new double[s3.Length];
                    for (int k = 0; k < s3.Length; k++) //robimy z niego double
                    {
                        d[k] = Convert.ToDouble(s3[k]);
                    }
                    //Wpisujemy dane do neuronu
                    for (int k = 1; k < d.Length; k++)
                    {
                        brain.Layers[i].Neurons[j].Weights[k - 1] = d[k];
                    }
                    (brain.Layers[i].Neurons[j] as ActivationNeuron).Threshold = d[0];
                    row++;

                    while (s2[row] == "")
                    {
                        row++;
                        if (row == s2.Count)
                        {
                            break;
                        }
                    }
                }
            }
            //DRUGI MÓZG
            row++;
            ActivationNetwork brain2 = new ActivationNetwork(new BipolarSigmoidFunction(), 12, 8, 4);
            for (int i = 0; i < brain2.Layers.Length; i++)
            {
                for (int j = 0; j < brain2.Layers[i].Neurons.Length; j++)
                {
                    s3 = s2[row].Split(';'); //bierzemy wiersz
                    d = new double[s3.Length];
                    for (int k = 0; k < s3.Length; k++) //robimy z niego double
                    {
                        d[k] = Convert.ToDouble(s3[k]);
                    }
                    //Wpisujemy dane do neuronu
                    for (int k = 1; k < d.Length; k++)
                    {
                        brain2.Layers[i].Neurons[j].Weights[k - 1] = d[k];
                    }
                    (brain2.Layers[i].Neurons[j] as ActivationNeuron).Threshold = d[0];
                    row++;

                    while (s2[row] == "")
                    {
                        row++;
                        if (row == s2.Count)
                        {
                            break;
                        }
                    }
                }
            }
            actualSnake2 = new Snake2();
            actualSnake2.brain = actualSnake2.CopyBrain(brain);
            actualSnake2.brain2 = actualSnake2.CopyBrain(brain2);
            p.BestOverall2 = new Snake2(actualSnake2);
        }
        private void LoadWinner(string plik)
        {
            actualEpoch = maxEpochs;
            it = p.size;
            string s = File.ReadAllText(plik);
            List<string> s2 = s.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            string[] s3;
            double[] d;
            ActivationNetwork brain = new ActivationNetwork(new BipolarSigmoidFunction(), 12, 8, 4);

            int row = 0;
            for (int i = 0; i < brain.Layers.Length; i++)
            {
                for (int j = 0; j < brain.Layers[i].Neurons.Length; j++)
                {
                    s3 = s2[row].Split(';'); //bierzemy wiersz
                    d = new double[s3.Length];
                    for (int k = 0; k < s3.Length; k++) //robimy z niego double
                    {
                        d[k] = Convert.ToDouble(s3[k]);
                    }
                    //Wpisujemy dane do neuronu
                    for (int k = 1; k < d.Length; k++)
                    {
                        brain.Layers[i].Neurons[j].Weights[k - 1] = d[k];
                    }
                    (brain.Layers[i].Neurons[j] as ActivationNeuron).Threshold = d[0];
                    row++;

                    while (s2[row] == "")
                    {
                        row++;
                        if (row == s2.Count)
                        {
                            break;
                        }
                    }
                }
            }
            actualSnake = new Snake();
            actualSnake.brain = actualSnake.CopyBrain(brain);
            p.BestOverall = new Snake(actualSnake);
        }

        private void PlaySecondPopulation()
        {
            firstBrain = false;
            it = 0;
            actualEpoch = 0;
            InitializeSecondPopulation();
            actualSnake2 = p.snakes2[it];
            p.BestOverall2 = p.snakes2[0];
        }
        private void PlayLoadedSnake(bool first)
        {
            firstBrain = first;
            actualEpoch = maxEpochs;
            it = p.size;
            Draw = true;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30);
            dispatcherTimer.Start();
        }
        private void PlayBest()
        {
            actualSnake = new Snake(p.BestOverall);
        }
        private void PlayBest2()
        {
            actualSnake2 = new Snake2(p.BestOverall2);
        }

        private void Reset()
        {
            dispatcherTimer.Stop();
            fields = new Rectangle[size, size];
            EpochBlock.Text = 0.ToString();
            MaxBlock.Text = 0.ToString();
            MeanBlock.Text = 0.ToString();
            fields = new Rectangle[size, size];
            r = new Random();
            it = 0;
            maxEpochs = 100;
            actualEpoch = 0;
            wyniki = new List<string>();
            Draw = false;
            Board.RowDefinitions.Clear();
            Board.ColumnDefinitions.Clear();
            Board.Children.Clear();

            InitializeBoard();
            InitializePopulation();
            InitializeTimer();
            p.size = int.Parse(PopBOX.Text);
            firstBrain = true;
        }

        private void TeachPopulation_Click(object sender, RoutedEventArgs e)
        {
            maxEpochs = int.Parse(EpochBox.Text);
            p.size = int.Parse(PopBOX.Text);
            InitializeSnakes();
            dispatcherTimer.Start();
            //LoadWinner();
        }
        private void LoadBest_Click(object sender, RoutedEventArgs e)
        {
            p.size = int.Parse(PopBOX.Text);
            maxEpochs = int.Parse(EpochBox.Text);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                LoadWinner(openFileDialog.FileName);
                PlayLoadedSnake(true);
            }
        }
        private void LoadSecondBest_Click(object sender, RoutedEventArgs e)
        {
            maxEpochs = int.Parse(EpochBox.Text);
            p.size = int.Parse(PopBOX.Text);
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                LoadWinner2(openFileDialog.FileName);
                PlayLoadedSnake(false);
            }
        }
        private void TeachPopulation2_Click(object sender, RoutedEventArgs e)
        {
            maxEpochs = int.Parse(EpochBox.Text);
            p.size = int.Parse(PopBOX.Text);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                LoadWinner(openFileDialog.FileName);
                string[] temp = openFileDialog.FileName.Split('\\');
                NameBox.Text = temp[temp.Length - 1];
                PlaySecondPopulation();
                dispatcherTimer.Start();
            }
        }
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            if (firstBrain)
            {
                SaveWinner(NameBox.Text + ".txt");
            }
            else
            {
                SaveWinner2(NameBox.Text + ".txt");
            }
        }
    }
}
