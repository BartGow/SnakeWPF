using System.Collections.Generic;
using System.Linq;

namespace WPFSnake
{
    public class Population
    {
        public List<Snake> snakes;
        public List<Snake2> snakes2;
        
        public double populationFitness;
        public int size = 200;
        public double FitnessAVG {
            get
            {
                double sum = 0;
                for (int i = 0; i < size; i++)
                {
                    sum += snakes[i].fitness;
                }
                return sum / size;
            }
        }
        public double FitnessAVG2
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < size; i++)
                {
                    sum += snakes2[i].fitness;
                }
                return sum / size;
            }
        }
        public double FitnessMAX
        {
            get
            {
                return snakes.Max(x => x.fitness);
            }
        }
        public Snake BestSnake
        {
            get
            {
                return snakes.First(x => x.fitness == snakes.Max(y => y.fitness));
            }
        }
        public Snake2 BestSnake2
        {
            get
            {
                return snakes2.First(x => x.fitness2 == snakes2.Max(y => y.fitness2));
            }
        }
        public Snake BestOverall
        {
            get;
            set;
        }
        public Snake2 BestOverall2
        {
            get;
            set;
        }
        public Population()
        {
            snakes = new List<Snake>();
        }
        public Population(Snake best)
        {
            snakes2 = new List<Snake2>();
            for (int i = 0; i < size; i++)
            {
                snakes2.Add(new Snake2(best));
            }
        }

        public void UseGeneticOperations()
        {
            Crossover();
            Mutate();
            ResetThem();
        }

        public void UseGeneticOperations2()
        {
            Crossover2();
            Mutate2();
            ResetThem2();
        }
        public void Crossover()
        {
            int howManyBest = 5;
            List<Snake> best = ChooseBest(howManyBest);
            best.Add(new Snake(BestOverall));
            List<Snake> childs = new List<Snake>();
            Snake[] dzieci = new Snake[2];
            for (int k = 0; k < (size-20)/60; k++) //180
            {
                for (int i = 0; i < howManyBest+1; i++)
                {
                    for (int j = 0; j < howManyBest+1; j++)
                    {
                        if (i != j)
                        {
                            dzieci = best[i].Crossover(best[j]);
                            childs.AddRange(dzieci);
                        }
                    }
                } 
            }
            for (int i = 0; i < 2; i++) //10
            {
                childs.AddRange(best); 
            }
            for (int i = 0; i < 10; i++) //10
            {
                childs.Add(new Snake());
            }
            snakes = childs;
        }
        public void Crossover2()
        {
            int howManyBest = 5;
            List<Snake2> best = ChooseBest2(howManyBest);
            best.Add(new Snake2(BestOverall2));
            List<Snake2> childs = new List<Snake2>();
            Snake2[] dzieci = new Snake2[2];
            for (int k = 0; k < (size - 20) / 60; k++) //180
            {
                for (int i = 0; i < howManyBest + 1; i++)
                {
                    for (int j = 0; j < howManyBest + 1; j++)
                    {
                        if (i != j)
                        {
                            dzieci = best[i].Crossover(best[j]);
                            childs.AddRange(dzieci);
                        }
                    }
                }
            }
            for (int i = 0; i < 2; i++) //10
            {
                childs.AddRange(best);
            }
            for (int i = 0; i < 10; i++) //10
            {
                childs.Add(new Snake2());
            }
            for (int i = 0; i < snakes2.Count; i++)
            {
                snakes2[i].brain2 = childs[i].CopyBrain(childs[i].brain2);
            }
        }
        public void Mutate()
        {
            foreach (Snake snake in snakes)
            {
                snake.Mutate2();
            }
        }
        public void Mutate2()
        {
            foreach (Snake2 snake in snakes2)
            {
                snake.Mutate2();
            }
        }
        public List<Snake> ChooseBest(int howMany)
        {
            List<Snake> a = new List<Snake>(snakes);
            List<Snake> best= new List<Snake>();
            Snake temp;
            for (int i = 0; i < howMany; i++)
            {
                temp = a.First(x=>x.fitness == a.Max(y => y.fitness));
                best.Add(new Snake(temp));
                a.Remove(temp);
            }
            return best;
        }
        public List<Snake2> ChooseBest2(int howMany)
        {
            List<Snake2> a = new List<Snake2>(snakes2);
            List<Snake2> best = new List<Snake2>();
            Snake2 temp;
            for (int i = 0; i < howMany; i++)
            {
                temp = a.First(x => x.fitness2 == a.Max(y => y.fitness2));
                best.Add(new Snake2(temp));
                a.Remove(temp);
            }
            return best;
        }
        public void ResetThem()
        {
            for (int i = 0; i < size; i++)
            {
                snakes[i] = new Snake(snakes[i]);
            }
        }
        public void ResetThem2()
        {
            for (int i = 0; i < size; i++)
            {
                snakes2[i] = new Snake2(snakes2[i]);
            }
        }

    }
}
