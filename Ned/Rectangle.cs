using System;

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
            return x > Math.Min(X, X + Width) && x < Math.Max(X, X + Width) && y > Math.Min(Y, Y + Height) &&
                   y < Math.Max(Y, Y + Height);
        }

        public bool Intersects(Rectangle other)
        {
            return Math.Max(other.X, other.X + other.Width) > Math.Min(X, X + Width) &&
                   Math.Min(other.X, other.X + other.Width) < Math.Max(X, X + Width) &&
                   Math.Max(other.Y, other.Y + other.Height) > Math.Min(Y, Y + Height) &&
                   Math.Min(other.Y, other.Y + other.Height) < Math.Max(Y, Y + Height);
        }
    }
}