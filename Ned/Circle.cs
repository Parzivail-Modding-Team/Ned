using System;

namespace Ned
{
    public class Circle : IShape
    {
        public float X { get; }
        public float Y { get; }
        public float Radius { get; }

        public Circle(float x, float y, float radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public bool Pick(float x, float y)
        {
            return Math.Pow(x - X, 2) + Math.Pow(y - Y, 2) < Math.Pow(Radius, 2);
        }
    }
}