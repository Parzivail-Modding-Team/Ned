using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ned
{
    public class Rectangle : IShape
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public Rectangle(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public bool Pick(float x, float y)
        {
            return x > Math.Min(X, X + Width) && x < Math.Max(X, X + Width) && y > Math.Min(Y, Y + Height) && y < Math.Max(Y, Y + Height);
        }
    }
}
