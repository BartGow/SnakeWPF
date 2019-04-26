using AForge.Neuro;
using System;
using System.Collections.Generic;
using System.Linq;
namespace WPFSnake
{
    public class Snake
    {
        public static Random r = new Random();
        public const int mapSize = 20;
        public const int inputL = 12;
        public const int a = 16;
        public const int hiddenL = 8;// a * 8;
        //public const int hidden2L = 6;// a * 4;
        //public const int hidden3L = 4;// a * 2;
        public const int outputL = 4;
        // public const int range = mapSize;
        public const int howManyLives = 1;
        public const int howManyMapsToDie = 8;

        public ActivationNetwork brain;
        public Position headPosition;
        public List<Position> tailPosition;
        public Position velocity;
        public Position foodPosition;
        public int timeOfLife;
        public long fitness;
        public bool alive;
        public double[] vision;
        public int eatenFood;
        public int timetoDie;
        public double previousDistance;
        public Position previousHeadPosition;
        public int livesLeft;

        //public ActivationNetwork brain { get => brain; set => brain = value; }
        //public Position headPosition { get => headPosition; set => headPosition = value; }
        //public List<Position> tailPosition { get => tailPosition; set => tailPosition = value; }
        //public Position Velocity { get => velocity; set => velocity = value; }
        //public Position FoodPosition { get => foodPosition; set => foodPosition = value; }
        //public int TimeOfLife { get => timeOfLife; set => timeOfLife = value; }
        //public long fitness { get => fitness; set => fitness = value; }
        //public bool Alive { get => alive; set => alive = value; }
        //public double[] Vision { get => vision; set => vision = value; }
        //public int EatenFood { get => eatenFood; set => eatenFood = value; }
        //public int TimetoDie { get => timetoDie; set => timetoDie = value; }
        //public double PreviousDistance { get => previousDistance; set => previousDistance = value; }
        //public int livesLeft { get => livesLeft; set => livesLeft = value; }

        public Snake()
        {
            headPosition = new Position(r.Next(5, mapSize - 5), r.Next(5, mapSize - 5));
            velocity = new Position(1, 0);
            tailPosition = new List<Position>();
            tailPosition.Add(new Position(headPosition.X - 1, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 2, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 3, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 4, headPosition.Y));
            brain = new ActivationNetwork(new BipolarSigmoidFunction(), inputL, hiddenL,/* hidden2L, */outputL);
            timeOfLife = 0;
            fitness = 0;
            eatenFood = 0;
            alive = true;
            timetoDie = howManyMapsToDie * mapSize;
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
            timetoDie = howManyMapsToDie * mapSize;
            SpawnFood();
            previousHeadPosition = new Position(-1, -1);
        }
        public Snake(Snake origin)
        {
            headPosition = new Position(r.Next(5, mapSize - 5), r.Next(5, mapSize - 5));
            velocity = new Position(1, 0);
            tailPosition = new List<Position>();
            tailPosition.Add(new Position(headPosition.X - 1, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 2, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 3, headPosition.Y));
            tailPosition.Add(new Position(headPosition.X - 4, headPosition.Y));
            brain = CopyBrain(origin.brain);
            timeOfLife = 0;
            fitness = origin.fitness;
            eatenFood = 0;
            alive = true;
            timetoDie = howManyMapsToDie * mapSize;
            livesLeft = howManyLives;
            SpawnFood();
            previousHeadPosition = new Position(-1, -1);
        }

        public ActivationNetwork CopyBrain(ActivationNetwork oBrain)
        {
            ActivationNetwork copy = new ActivationNetwork(new BipolarSigmoidFunction(), inputL, hiddenL,/* hidden2L,*/ outputL);
            for (int i = 0; i < oBrain.Layers.Count(); i++)
            {
                for (int j = 0; j < oBrain.Layers[i].Neurons.Count(); j++)
                {
                    (copy.Layers[i].Neurons[j] as ActivationNeuron).Threshold = (oBrain.Layers[i].Neurons[j] as ActivationNeuron).Threshold;
                    for (int k = 0; k < oBrain.Layers[i].Neurons[j].Weights.Length; k++)
                    {
                        copy.Layers[i].Neurons[j].Weights[k] = oBrain.Layers[i].Neurons[j].Weights[k];
                    }
                }
            }
            return copy;
        }
        public virtual void Move()
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
            }
            if (Ate())
            {
                AddTailPiece(headPosition);
                eatenFood++;
                timetoDie += 10 * mapSize;
                SpawnFood();
            }

        }
        public void MoveHead()
        {
            headPosition += velocity;
        }
        public void MoveTail()
        {
            for (int i = tailPosition.Count - 1; i > 0; i--)
            {
                tailPosition[i].X = tailPosition[i - 1].X;
                tailPosition[i].Y = tailPosition[i - 1].Y;
            }
            tailPosition[0].X = headPosition.X - velocity.X;
            tailPosition[0].Y = headPosition.Y - velocity.Y;
        }
        public void AddTailPiece(Position p)
        {
            tailPosition.Add(p);
        }
        public bool Died()
        {
            if (headPosition.X < 0 || headPosition.X >= mapSize || headPosition.Y < 0 || headPosition.Y >= mapSize)
            {
                return true;
            }
            if (tailPosition.Any(p => p.X == headPosition.X && p.Y == headPosition.Y))
            {
                return true;
            }
            return false;
        }
        public bool Ate()
        {
            if ((headPosition).Equals(foodPosition))
            {
                return true;
            }
            return false;
        }
        public void SpawnFood()
        {
            bool jest = false;

            do
            {
                jest = false;
                foodPosition = new Position(r.Next(mapSize), r.Next(mapSize));

                for (int i = 0; i < tailPosition.Count; i++)
                {
                    if (tailPosition[i].X == foodPosition.X && tailPosition[i].Y == foodPosition.Y)
                    {
                        jest = true;
                        break;
                    }
                }
                if (jest)
                {
                    continue;
                }
                if (!(foodPosition.X == headPosition.X && foodPosition.Y == headPosition.Y))
                {
                    break;
                }
                //if (!tailPosition.Any(p => p.X == foodPosition.X && p.Y == foodPosition.Y))
                //{
                //    break;
                //}
            } while (true);
        }
        public void Look()
        {
            vision = new double[0];
            //Tu jest zmiana odległości
            //double distanceToFood = Math.Abs(headPosition - foodPosition) / mapSize;
            //vision[0] = (previousDistance - distanceToFood);
            //previousDistance = (headPosition - foodPosition) / mapSize;

            //stare wersje
            //vision[0] = distanceToFood;
            //vision[1] = Math.Tan(Math.Atan2(foodPosition.Y - headPosition.Y, foodPosition.X - headPosition.X));
            //vision[1] = Math.Tan(Math.Atan2(foodPosition.Y - headPosition.Y + velocity.Y, foodPosition.X - headPosition.X + velocity.X));
            //vision[1] = Math.Tan((180/Math.PI)*Math.Atan2(foodPosition.Y - headPosition.Y, foodPosition.X - headPosition.X));
            //vision[1] = Math.Tan((180/Math.PI)*Math.Atan2(foodPosition.Y - headPosition.Y + velocity.Y, foodPosition.X - headPosition.X + velocity.X));
            //vision[1] = Math.Atan2(foodPosition.Y - headPosition.Y, foodPosition.X - headPosition.X)/(Math.PI*2);
            //vision[1] = Math.Atan2(foodPosition.Y - headPosition.Y, foodPosition.X - headPosition.X);

            vision = vision.Concat(Angle2()).ToArray();
            vision = vision.Concat(TailDetect()).ToArray();
            vision = vision.Concat(WallDetect()).ToArray();

            //vision[3] = LookInDirection(new Position(1, 0));
            //vision[4] = LookInDirection(new Position(-1, 0));
            //vision[5] = LookInDirection(new Position(0, 1));
            //vision[6] = LookInDirection(new Position(0, -1));
            //vision[5] = Velocity.X;
            //vision[6] = Velocity.Y;
            //vision[3] = CheckForTail(new Position(1, 0));
            //vision[4] = CheckForTail(new Position(-1, 0));
            //vision[5] = CheckForTail(new Position(0, 1));
            //vision[6] = CheckForTail(new Position(0, -1));
            //Position temp = headPosition + new Position(1, 0);
            //if (temp.X == 0 || temp.Y == 0 || temp.X == mapSize || temp.Y == mapSize)
            //{
            //    vision[3] = 1;
            //}
            //temp = headPosition + new Position(-1, 0);
            //if (temp.X == 0 || temp.Y == 0 || temp.X == mapSize || temp.Y == mapSize)
            //{
            //    vision[4] = 1;
            //}
            //temp = headPosition + new Position(0, 1);
            //if (temp.X == 0 || temp.Y == 0 || temp.X == mapSize || temp.Y == mapSize)
            //{
            //    vision[5] = 1;
            //}
            //temp = headPosition + new Position(0, -1);
            //if (temp.X == 0 || temp.Y == 0 || temp.X == mapSize || temp.Y == mapSize)
            //{
            //    vision[6] = 1;
            //}
        }

        //public double[] Angle()
        //{
        //    // new double[] {lewo, góra, prawo, dół};
        //    Position temp = new Position(headPosition.X, headPosition.Y);
        //    //temp = velocity;
        //    if (foodPosition.X < temp.X && foodPosition.Y < temp.Y) //jedzenie jest w lewo i do góry
        //    {
        //        return new double[] { 0.5, 0.5, 0, 0 };
        //    }
        //    if (foodPosition.X > temp.X && foodPosition.Y > temp.Y) //jedzenie jest w prawo i na dół
        //    {
        //        return new double[] { 0, 0, 0.5, 0.5 };
        //    }
        //    if (foodPosition.X < temp.X && foodPosition.Y > temp.Y) //jedzenie jest w lewo i na dół
        //    {
        //        return new double[] { 0.5, 0, 0, 0.5 };
        //    }
        //    if (foodPosition.X > temp.X && foodPosition.Y < temp.Y) //jedzenie jest w prawo i do góry
        //    {
        //        return new double[] { 0, 0.5, 0.5, 0 };
        //    }
        //    if (foodPosition.X == temp.X && foodPosition.Y < temp.Y) //w górę
        //    {
        //        return new double[] { 0, 1, 0, 0 };
        //    }
        //    if (foodPosition.X == temp.X && foodPosition.Y > temp.Y) //w dół
        //    {
        //        return new double[] { 0, 0, 0, 1 };
        //    }
        //    if (foodPosition.X < temp.X && foodPosition.Y == temp.Y) //w lewo
        //    {
        //        return new double[] { 1, 0, 0, 0 };
        //    }
        //    if (foodPosition.X > temp.X && foodPosition.Y == temp.Y) //w prawo
        //    {
        //        return new double[] { 0, 0, 1, 0 };
        //    }
        //    return new double[] { 0, 0, 0, 0 };
        //}
        public double[] Angle2()
        {
            // new double[] {lewo, góra, prawo, dół};
            Position temp = new Position(headPosition.X, headPosition.Y);
            //temp = velocity;
            //double x = 2 / (double)(Math.Abs(foodPosition.X - temp.X) + 1);
            //double y = 2 / (double)(Math.Abs(foodPosition.Y - temp.Y) + 1);
            double x = 1- (double)(Math.Abs(foodPosition.X - temp.X)/mapSize);
            double y = 1- (double)(Math.Abs(foodPosition.Y - temp.Y)/mapSize);
            //double suma = x + y;
            //x /= suma;
            //y /= suma;
            if (foodPosition.X < temp.X && foodPosition.Y < temp.Y) //jedzenie jest w lewo i do góry
            {
                return new double[] { x, y, 0, 0 };
            }
            if (foodPosition.X > temp.X && foodPosition.Y > temp.Y) //jedzenie jest w prawo i na dół
            {
                return new double[] { 0, 0, x, y };
            }
            if (foodPosition.X < temp.X && foodPosition.Y > temp.Y) //jedzenie jest w lewo i na dół
            {
                return new double[] { x, 0, 0, y };
            }
            if (foodPosition.X > temp.X && foodPosition.Y < temp.Y) //jedzenie jest w prawo i do góry
            {
                return new double[] { 0, y, x, 0 };
            }
            if (foodPosition.X == temp.X && foodPosition.Y < temp.Y) //w górę
            {
                return new double[] { 0, y, 0, 0 };
            }
            if (foodPosition.X == temp.X && foodPosition.Y > temp.Y) //w dół
            {
                return new double[] { 0, 0, 0, y };
            }
            if (foodPosition.X < temp.X && foodPosition.Y == temp.Y) //w lewo
            {
                return new double[] { x, 0, 0, 0 };
            }
            if (foodPosition.X > temp.X && foodPosition.Y == temp.Y) //w prawo
            {
                return new double[] { 0, 0, x, 0 };
            }
            return new double[] { 0, 0, 0, 0 };
        }
        //public double[] Angle3()
        //{
        //    Position temp = new Position(headPosition.X, headPosition.Y);
        //    //legora,pwo,rawo,dol,l-g,g-p,p-d,d-l
        //    if (foodPosition.X < temp.X && foodPosition.Y < temp.Y) //jedzenie jest w lewo i do góry
        //    {
        //        return new double[] { 0, 0, 0, 0, 1, 0, 0, 0 };
        //    }
        //    if (foodPosition.X > temp.X && foodPosition.Y > temp.Y) //jedzenie jest w prawo i na dół
        //    {
        //        return new double[] { 0, 0, 0, 0, 0, 0, 1, 0 };
        //    }
        //    if (foodPosition.X < temp.X && foodPosition.Y > temp.Y) //jedzenie jest w lewo i na dół
        //    {
        //        return new double[] { 0, 0, 0, 0, 0, 0, 0, 1 };
        //    }
        //    if (foodPosition.X > temp.X && foodPosition.Y < temp.Y) //jedzenie jest w prawo i do góry
        //    {
        //        return new double[] { 0, 0, 0, 0, 0, 1, 0, 0 };
        //    }
        //    if (foodPosition.X == temp.X && foodPosition.Y < temp.Y) //w górę
        //    {
        //        return new double[] { 0, 1, 0, 0, 0, 0, 0, 0 };
        //    }
        //    if (foodPosition.X == temp.X && foodPosition.Y > temp.Y) //w dół
        //    {
        //        return new double[] { 0, 0, 0, 1, 0, 0, 0, 0 };
        //    }
        //    if (foodPosition.X < temp.X && foodPosition.Y == temp.Y) //w lewo
        //    {
        //        return new double[] { 1, 0, 0, 0, 0, 0, 0, 0 };
        //    }
        //    if (foodPosition.X > temp.X && foodPosition.Y == temp.Y) //w prawo
        //    {
        //        return new double[] { 0, 0, 1, 0, 0, 0, 0, 0 };
        //    }
        //    return new double[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        //}
        //public void TailDetect(double[] vis)
        //{
        //    double val = 0.05;
        //    Position temp;
        //    List<Position> tempList = new List<Position>(tailPosition);
        //    tempList.Remove(tempList[0]);
        //    temp = new Position(headPosition.X - 1, headPosition.Y);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[1] += val;
        //        vis[2] += val;
        //        vis[3] += val;
        //    }
        //    temp = new Position(headPosition.X, headPosition.Y - 1);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[0] += val;
        //        vis[2] += val;
        //        vis[3] += val;
        //    }
        //    temp = new Position(headPosition.X + 1, headPosition.Y);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[1] += val;
        //        vis[0] += val;
        //        vis[3] += val;
        //    }
        //    temp = new Position(headPosition.X, headPosition.Y + 1);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[1] += val;
        //        vis[2] += val;
        //        vis[0] += val;
        //    }
        //    //////////////////////////////////////////
        //    temp = new Position(headPosition.X - 1, headPosition.Y);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[0] = -1;
        //    }
        //    temp = new Position(headPosition.X, headPosition.Y - 1);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[1] = -1;
        //    }
        //    temp = new Position(headPosition.X + 1, headPosition.Y);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[2] = -1;
        //    }
        //    temp = new Position(headPosition.X, headPosition.Y + 1);
        //    if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //    {
        //        vis[3] = -1;
        //    }
        //}
        public double[] TailDetect()
        {
            double[] vis = new double[4];
            Position temp;
            List<Position> tempList = new List<Position>(tailPosition);
            tempList.Remove(tempList[0]);
            temp = new Position(headPosition.X - 1, headPosition.Y);
            if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
            {
                vis[0] = -1;
            }
            temp = new Position(headPosition.X, headPosition.Y - 1);
            if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
            {
                vis[1] = -1;
            }
            temp = new Position(headPosition.X + 1, headPosition.Y);
            if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
            {
                vis[2] = -1;
            }
            temp = new Position(headPosition.X, headPosition.Y + 1);
            if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
            {
                vis[3] = -1;
            }
            return vis;
        }
        //public double[] TailDetect2()
        //{
        //    double[] vis = new double[8];
        //    Position temp;
        //    List<Position> tempList = new List<Position>(tailPosition);
        //    List<Position> listOfPositions = new List<Position>() { new Position(-1, 0), new Position(0, -1), new Position(1, 0), new Position(0, 1), new Position(-1, -1), new Position(-1, 1), new Position(1, -1), new Position(1, 1) };
        //    //tempList.Remove(tempList[0]);
        //    double distance;
        //    for (int j = 0; j < vis.Length; j++)
        //    {
        //        distance = 0;
        //        temp = new Position(headPosition.X, headPosition.Y);
        //        for (int i = 0; i < mapSize; i++)
        //        {
        //            temp += listOfPositions[j];
        //            distance++;
        //            if (tempList.Exists(x => x.X == temp.X && x.Y == temp.Y))
        //            {
        //                if (temp.X == tempList[0].X && temp.Y == tempList[0].Y)
        //                {
        //                    vis[j] = 0;
        //                    break;
        //                }
        //                vis[j] = 1 / distance;
        //                break;
        //            }
        //        }
        //    }
        //    return vis;
        //}
        public double[] WallDetect()
        {
            double[] vis = new double[4];
            double distance = 0;
            List<Position> listOfPositions = new List<Position>() { new Position(-1, 0), new Position(0, -1), new Position(1, 0), new Position(0, 1) };
            //List<Position> listOfPositions = new List<Position>() { new Position(-1, 0), new Position(0, -1), new Position(1, 0), new Position(0, 1), new Position(-1, -1), new Position(-1, 1), new Position(1, -1), new Position(1, 1) };
            Position temp = new Position(headPosition.X, headPosition.Y);
            for (int i = 0; i < 4; i++)
            {
                //temp = new Position(headPosition.X, headPosition.Y);
                //distance = 0;
                //for (int j = 0; j < mapSize; j++)
                //{
                //    temp += listOfPositions[i];
                //    distance++;
                //    if (temp.X == -1 || temp.Y == -1 || temp.X == mapSize || temp.Y == mapSize)
                //    {
                //        vis[i] = 1 / distance;
                //        break;
                //    }
                //}
                temp = new Position(headPosition.X, headPosition.Y);
                temp += listOfPositions[i];
                if (temp.X == -1 || temp.Y == -1 || temp.X == mapSize || temp.Y == mapSize)
                {
                    vis[i] = 1;
                }
            }
            return vis;
        }
        //public double LookInDirection(Position direction)
        //{
        //    Position p = new Position(headPosition.X, headPosition.Y);
        //    double distance = 0;

        //    p += direction;
        //    distance++;

        //    while (p.X > 0 && p.X < mapSize - 1 && p.Y > 0 && p.Y < mapSize - 1)
        //    {
        //        p += direction;
        //        distance++;
        //    }
        //    if (distance > range)
        //    {
        //        return 0;
        //    }
        //    else
        //    {
        //        return 1 - (distance / range);
        //    }
        //}

        //public double CheckForTail(Position direction)
        //{
        //    Position p = new Position(headPosition.X, headPosition.Y);
        //    p += direction;
        //    if (tailPosition.Exists(x => x.X == p.X && x.Y == p.Y) && p.X != previousHeadPosition.X && p.Y != previousHeadPosition.Y)
        //    {
        //        return 1;
        //    }
        //    return 0;
        //}

        //public void SetVelocity()
        //{
        //    double[] outputs = brain.Compute(vision);
        //    if (outputs.Max() == outputs[0]) //idz w prawo
        //    {
        //        if (velocity.X != -1)
        //        {
        //            velocity = new Position(1, 0);
        //        }
        //    }
        //    if (outputs.Max() == outputs[1]) //idz w lewo
        //    {
        //        if (velocity.X != 1)
        //        {
        //            velocity = new Position(-1, 0);
        //        }
        //    }
        //    if (outputs.Max() == outputs[2]) //idz w gora
        //    {
        //        if (velocity.Y != 1)
        //        {
        //            velocity = new Position(0, -1);
        //        }
        //    }
        //    if (outputs.Max() == outputs[3]) //idz w doł
        //    {
        //        if (velocity.Y != -1)
        //        {
        //            velocity = new Position(0, 1);
        //        }
        //    }
        //}
        public virtual void SetVelocity3()
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
        }
        //public void SetVelocity2()
        //{
        //    double[] outputs = brain.Compute(vision);
        //    int x, y;
        //    if (outputs.Max() == outputs[0]) //idz w prawo //1,0=>0,1   -1,0 => 0,-1  0,1 => -1,0   0,-1=>1,0
        //    {
        //        x = velocity.X;
        //        y = velocity.Y;
        //        if (y == 0)
        //        {
        //            velocity.X = y;
        //            velocity.Y = x;
        //        }
        //        else
        //        {
        //            velocity.X = -y;
        //            velocity.Y = -x;
        //        }
        //    }
        //    if (outputs.Max() == outputs[1]) //idz w lewo 
        //    {
        //        x = velocity.X;
        //        y = velocity.Y;
        //        if (x == 0)
        //        {
        //            velocity.X = y;
        //            velocity.Y = x;
        //        }
        //        else
        //        {
        //            velocity.X = -y;
        //            velocity.Y = -x;
        //        }
        //    }
        //}
        public virtual void CalculateFitness()
        {
            //fitness = (long)Math.Floor(timeOfLife/100.0 * eatenFood);
            //fitness = timeOfLife * (eatenFood+1);
            //fitness = timeOfLife;
            //fitness = timeOfLife * timeOfLife * (long)Math.Pow(2, eatenFood);
            fitness = (eatenFood / (howManyLives));
            //fitness *= (secondBrainFitness / (howManyLives*10));
            //secondBrainFitness /= howManyLives;
            //fitness = (timeOfLife / 10) * (long)Math.Pow(3, eatenFood + 1);
            //fitness = timeOfLife + (eatenFood * 200);
        }

        //public void Mutate()
        //{
        //    for (int j = 0; j < 4; j++)
        //    {
        //        for (int i = 0; i < brain.Layers[j].Neurons.Count(); i++)
        //        {
        //            double rand = r.NextDouble();
        //            if (rand < 0.05)
        //            {
        //                double m = (double)r.Next(-100, 100) / (double)1000;
        //                (brain.Layers[j].Neurons[i] as ActivationNeuron).Threshold += m;
        //                if ((brain.Layers[j].Neurons[i] as ActivationNeuron).Threshold < -1)
        //                {
        //                    (brain.Layers[j].Neurons[i] as ActivationNeuron).Threshold = -1;
        //                }
        //                if ((brain.Layers[j].Neurons[i] as ActivationNeuron).Threshold > 1)
        //                {
        //                    (brain.Layers[j].Neurons[i] as ActivationNeuron).Threshold = 1;
        //                }
        //            }
        //            for (int k = 0; k < brain.Layers[j].Neurons[i].Weights.Count(); k++)
        //            {
        //                rand = r.NextDouble();
        //                if (rand < 0.05)
        //                {
        //                    double m = (double)r.Next(-100, 100) / (double)1000;
        //                    brain.Layers[j].Neurons[i].Weights[k] += m;
        //                    if (brain.Layers[j].Neurons[i].Weights[k] < -1)
        //                    {
        //                        brain.Layers[j].Neurons[i].Weights[k] = -1;
        //                    }
        //                    if (brain.Layers[j].Neurons[i].Weights[k] > 1)
        //                    {
        //                        brain.Layers[j].Neurons[i].Weights[k] = 1;
        //                    }
        //                }
        //            }

        //        }
        //    }
        //}
        public virtual void Mutate2()
        {
            int warstwa = r.Next(0, brain.Layers.Count() - 2);
            int neuron = r.Next(0, brain.Layers[warstwa].Neurons.Count() - 1);
            for (int k = 0; k < brain.Layers[warstwa].Neurons[neuron].Weights.Count(); k++)
            {
                double rand = r.NextDouble();

                if (rand < 0.05)
                {
                    double m = (double)r.Next(-100, 100) / (double)1000;
                    brain.Layers[warstwa].Neurons[neuron].Weights[k] += m;
                    if (brain.Layers[warstwa].Neurons[neuron].Weights[k] < -1)
                    {
                        brain.Layers[warstwa].Neurons[neuron].Weights[k] = -1;
                    }
                    if (brain.Layers[warstwa].Neurons[neuron].Weights[k] > 1)
                    {
                        brain.Layers[warstwa].Neurons[neuron].Weights[k] = 1;
                    }
                }
            }
        }
        public Snake[] Crossover(Snake s)
        {
            Snake[] tab = new Snake[2];
            Snake baby1 = new Snake();
            Snake baby2 = new Snake();
            int warstwa = r.Next(0, brain.Layers.Count() - 2);
            int neuron = r.Next(0, brain.Layers[warstwa].Neurons.Count() - 1);
            bool przed = true;
            for (int i = 0; i < brain.Layers.Count() - 1; i++)
            {
                for (int j = 0; j < brain.Layers[i].Neurons.Count(); j++)
                {
                    if (j < neuron && warstwa == i)
                    {
                        przed = false;
                    }
                    if (przed)
                    {
                        for (int k = 0; k < brain.Layers[i].Neurons[j].Weights.Count(); k++)
                        {
                            baby1.brain.Layers[i].Neurons[j].Weights[k] = brain.Layers[i].Neurons[j].Weights[k];
                            baby2.brain.Layers[i].Neurons[j].Weights[k] = s.brain.Layers[i].Neurons[j].Weights[k];
                        }
                    }
                    else
                    {
                        for (int k = 0; k < brain.Layers[i].Neurons[j].Weights.Count(); k++)
                        {
                            baby2.brain.Layers[i].Neurons[j].Weights[k] = brain.Layers[i].Neurons[j].Weights[k];
                            baby1.brain.Layers[i].Neurons[j].Weights[k] = s.brain.Layers[i].Neurons[j].Weights[k];
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