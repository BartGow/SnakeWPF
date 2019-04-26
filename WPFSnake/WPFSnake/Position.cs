using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFSnake
{
    public class Position
    {
        private int x;
        private int y;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Position operator +(Position a, Position b)
        {
            return new Position(a.X + b.X, a.Y + b.Y);
        }

        public static double operator -(Position a, Position b)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(a.X-b.X),2)+ Math.Pow(Math.Abs(a.Y - b.Y), 2));
        }

        public override bool Equals(object obj)
        {
            if (x == (obj as Position).X && y == (obj as Position).Y) { return true; }
            return false;
        }
    }
}
