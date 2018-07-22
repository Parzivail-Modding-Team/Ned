using System;
using System.Collections.Generic;

namespace Ned
{
    public class Node
    {
        public Connection Input { get; set; }
        public List<Connection> Outputs { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; } = 180;
        public float Height => (Math.Max(Outputs.Count, 1) + 1) * 24 + 18;

        public string Name { get; }

        public Node(float x, float y, string name)
        {
            X = x;
            Y = y;
            Name = name;

            Input = new Connection(this, NodeSide.Input, 0, "Input");
            Outputs = new List<Connection>
            {
                new Connection(this, NodeSide.Output, 0, "DebugOutput 1"),
                new Connection(this, NodeSide.Output, 1, "DebugOutput 2"),
                new Connection(this, NodeSide.Output, 2, "DebugOutput 3")
            };
        }
    }
}