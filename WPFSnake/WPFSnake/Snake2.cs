using System.Collections.Generic;
using System.Linq;
using AForge.Neuro;
namespace WPFSnake
{
    public class Snake2 : Snake
    {
        public ActivationNetwork brain2;
        public bool usedSecondBrain;
        public long fitness2;

        public Snake2()
        {
            headPosition = new Position(r.Next(5, mapSize - 5), r.Next(5, mapSize - 5));
            velocity = new Position(1, 0);
            tailPosition = new List<Position>();
            tailPosition.Add(new Position(headPosition.X - 1, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 2, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 3, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 4, headPosition.Y));
            brain = new ActivationNetwork(new BipolarSigmoidFunction(), inputL, hiddenL,/* hidden2L, */outputL);
            brain2 = new ActivationNetwork(new BipolarSigmoidFunction(), inputL, hiddenL, outputL);
            usedSecondBrain = false;
            fitness2 = 0;
            timeOfLife = 0;
            fitness = 0;
            eatenFood = 0;
            alive = true;
            timetoDie = 4 * mapSize;
            livesLeft = howManyLives;
            SpawnFood();
            previousHeadPosition = new Position(-1, -1);
        }
        private void Reset()
        {
            headPosition = new Position(r.Next(5, mapSize - 5), r.Next(5, mapSize - 5));
            velocity = new Position(1, 0);
            tailPosition = new List<Position>();
            tailPosition.Add(new Position(headPosition.X - 1, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 2, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 3, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 4, headPosition.Y));
            timeOfLife = 0;
            alive = true;
            timetoDie = 4 * mapSize;
            SpawnFood();
            previousHeadPosition = new Position(-1, -1);
        }
        public Snake2(Snake2 origin)
        {
            headPosition = new Position(r.Next(5, mapSize - 5), r.Next(5, mapSize - 5));
            velocity = new Position(1, 0);
            tailPosition = new List<Position>();
            tailPosition.Add(new Position(headPosition.X - 1, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 2, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 3, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 4, headPosition.Y));
            brain = CopyBrain(origin.brain);
            brain2 = CopyBrain(origin.brain2);
            usedSecondBrain = false;
            fitness2 = 0;
            timeOfLife = 0;
            fitness = origin.fitness;
            eatenFood = 0;
            alive = true;
            timetoDie = 4 * mapSize;
            livesLeft = howManyLives;
            SpawnFood();
            previousHeadPosition = new Position(-1, -1);
        }
        public Snake2(Snake origin)
        {
            headPosition = new Position(r.Next(5, mapSize - 5), r.Next(5, mapSize - 5));
            velocity = new Position(1, 0);
            tailPosition = new List<Position>();
            tailPosition.Add(new Position(headPosition.X - 1, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 2, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 3, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 4, headPosition.Y));
            brain = CopyBrain(origin.brain);
            brain2 = new ActivationNetwork(new BipolarSigmoidFunction(), inputL, hiddenL, outputL);
            usedSecondBrain = false;
            fitness2 = 0;
            timeOfLife = 0;
            fitness = origin.fitness;
            eatenFood = 0;
            alive = true;
            timetoDie = 4 * mapSize;
            livesLeft = howManyLives;
            SpawnFood();
            previousHeadPosition = new Position(-1, -1);
        }
        

        public override void Move()
        {
            int temp = livesLeft;
            timeOfLife++;
            timetoDie--;
            MoveHead();
            MoveTail();
            if (Died() || timetoDie == 0)
            {
                livesLeft--;
                if (livesLeft == 0)
                {
                    alive = false;
                }
                else
                {
                    Reset();
                }
                return;
            }
            if (usedSecondBrain)
            {
                usedSecondBrain = false;
                fitness2++;
            }
            if (Ate())
            {
                AddTailPiece(headPosition);
                eatenFood++;
                timetoDie = 6 * mapSize;
                SpawnFood();
            }

        }
        public override void SetVelocity3()
        {
            double[] outputs = brain.Compute(vision);
            List<Position> listOfPositions = new List<Position>() { new Position(-1, 0), new Position(0, -1), new Position(1, 0), new Position(0, 1) };
            //lewo, góra, prawo, dół
            for (int i = 0; i < 4; i++)
            {
                if (outputs.Max() == outputs[i])
                {
                    if (-velocity.X == listOfPositions[i].X && -velocity.Y == listOfPositions[i].Y)
                    {
                        outputs[i] = -1;
                        i = -1;
                    }
                    else
                    {
                        velocity = listOfPositions[i];
                        break;
                    }
                }
            }
            Position temp = headPosition + velocity;
            if ((temp.X < 0 || temp.X >= mapSize || temp.Y < 0 || temp.Y >= mapSize) || (tailPosition.Any(p => p.X == temp.X && p.Y == temp.Y)))
            {
                usedSecondBrain = true;
                outputs = brain2.Compute(vision);
                for (int i = 0; i < 4; i++)
                {
                    if (outputs.Max() == outputs[i])
                    {
                        if (-velocity.X == listOfPositions[i].X && -velocity.Y == listOfPositions[i].Y)
                        {
                            outputs[i] = -1;
                            i = -1;
                        }
                        else
                        {
                            velocity = listOfPositions[i];
                            break;
                        }
                    }
                }
            }
        }
        public override void CalculateFitness()
        {
            fitness = (fitness2 / howManyLives);
        }
        public override void Mutate2()
        {
            int warstwa = r.Next(0, brain2.Layers.Count() - 2);
            int neuron = r.Next(0, brain2.Layers[warstwa].Neurons.Count() - 1);
            for (int k = 0; k < brain2.Layers[warstwa].Neurons[neuron].Weights.Count(); k++)
            {
                double rand = r.NextDouble();

                if (rand < 0.05)
                {
                    double m = (double)r.Next(-100, 100) / (double)1000;
                    brain2.Layers[warstwa].Neurons[neuron].Weights[k] += m;
                    if (brain2.Layers[warstwa].Neurons[neuron].Weights[k] < -1)
                    {
                        brain2.Layers[warstwa].Neurons[neuron].Weights[k] = -1;
                    }
                    if (brain2.Layers[warstwa].Neurons[neuron].Weights[k] > 1)
                    {
                        brain2.Layers[warstwa].Neurons[neuron].Weights[k] = 1;
                    }
                }
            }
        }
        public Snake2[] Crossover(Snake2 s)
        {
            Snake2[] tab = new Snake2[2];
            Snake2 baby1 = new Snake2();
            Snake2 baby2 = new Snake2();
            int warstwa = r.Next(0, brain2.Layers.Count() - 2);
            int neuron = r.Next(0, brain2.Layers[warstwa].Neurons.Count() - 1);
            bool przed = true;
            for (int i = 0; i < brain2.Layers.Count() - 1; i++)
            {
                for (int j = 0; j < brain2.Layers[i].Neurons.Count(); j++)
                {
                    if (j < neuron && warstwa == i)
                    {
                        przed = false;
                    }
                    if (przed)
                    {
                        for (int k = 0; k < brain2.Layers[i].Neurons[j].Weights.Count(); k++)
                        {
                            baby1.brain2.Layers[i].Neurons[j].Weights[k] = brain2.Layers[i].Neurons[j].Weights[k];
                            baby2.brain2.Layers[i].Neurons[j].Weights[k] = s.brain2.Layers[i].Neurons[j].Weights[k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < brain2.Layers[i].Neurons[j].Weights.Count(); k++)
                        {
                            baby2.brain2.Layers[i].Neurons[j].Weights[k] = brain2.Layers[i].Neurons[j].Weights[k];
                            baby1.brain2.Layers[i].Neurons[j].Weights[k] = s.brain2.Layers[i].Neurons[j].Weights[k];
                        }
                    }
                }
            }
            tab[0] = baby1;
            tab[1] = baby2;
            return tab;
        }
        
    }
}