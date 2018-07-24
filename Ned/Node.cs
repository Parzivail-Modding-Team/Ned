using System;
using System.Collections.Generic;

namespace Ned
{
    public class Node : IShape
    {
        public Connection Input { get; set; }
        public List<Connection> Outputs { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public string Name { get; }
        public NodeType Type { get; }
        public Actor Actor { get; }

        public float Width => Type == NodeType.Option ? 240 : 90;
        public float Height => (Math.Max(Outputs.Count, 1) + 1) * 24 + 18;

        public Node(NodeType type, string name, float x, float y) : this(type, Actor.None, x, y)
        {
            Name = name;
        }

        public Node(NodeType type, Actor actor, float x, float y)
        {
            X = x;
            Y = y;
            Name = actor.ToString();
            Type = type;
            Actor = actor;

            Outputs = new List<Connection>();

            if (type == NodeType.End)
                Input = new Connection(this, NodeSide.Input, 0, "");
            else if (type == NodeType.Start)
                AddOutput("");
            else
            {
                Input = new Connection(this, NodeSide.Input, 0, "");

                if (Actor == Actor.NPC)
                    AddOutput("NPC Dialogue");
                else if (Actor == Actor.Player)
                {
                    AddOutput("Dialogue Option 1");
                    AddOutput("Dialogue Option 2");
                    AddOutput("Dialogue Option 3");
                }
            }
        }

        private void AddOutput(string text)
        {
            Outputs.Add(new Connection(this, NodeSide.Output, Outputs.Count, text));
        }

        public bool Pick(float x, float y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }

        public void RemoveOutput(Connection connection)
        {
            Outputs.Remove(connection);
        }

        public void BuildConnections()
        {
            for (int i = 0; i < Outputs.Count; i++)
                Outputs[i].ConnectionIndex = i;
        }
    }
}