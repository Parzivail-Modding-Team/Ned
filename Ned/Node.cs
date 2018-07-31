using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ned
{
    public class Node
    {
        public static Func<Node, int> WidthCalculator = node => node.Type == NodeType.Flow ? 240 : 90;

        public readonly Guid Id = Guid.NewGuid();

        public Connection Input { get; set; }
        public List<Connection> Outputs { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Layer { get; set; }
        public string Name { get; }
        public NodeType Type { get; }
        public Actor Actor { get; }

        public float Width { get; private set; } = 90;
        public float Height => (Math.Max(Outputs.Count, 1) + 1) * 20 + 20;

        private SavedNode _cachedLoadingNode;

        public Node(NodeType type, string name, float x, float y) : this(type, Actor.None, x, y)
        {
            Name = name;
        }

        internal Node(SavedNode node)
        {
            Outputs = new List<Connection>();

            Id = node.Id;
            X = node.X;
            Y = node.Y;
            Name = node.Name;
            Type = node.Type;
            Actor = node.Actor;

            _cachedLoadingNode = node;

            RecalculateWidth();
        }

        public Node(NodeType type, Actor actor, float x, float y)
        {
            X = x;
            Y = y;
            Name = actor.ToString();
            Type = type;
            Actor = actor;

            Outputs = new List<Connection>();

            switch (type)
            {
                case NodeType.End:
                    Input = new Connection(this, NodeSide.Input, 0, "");
                    break;
                case NodeType.Start:
                    AddOutput("");
                    break;
                default:
                    Input = new Connection(this, NodeSide.Input, 0, "");

                    switch (Actor)
                    {
                        case Actor.NPC:
                            AddOutput("NPC Dialogue");
                            break;
                        case Actor.Player:
                            AddOutput("Dialogue Option 1");
                            AddOutput("Dialogue Option 2");
                            AddOutput("Dialogue Option 3");
                            break;
                    }

                    break;
            }

            RecalculateWidth();
        }

        public Node(Node other)
        {
            X = other.X;
            Y = other.Y;
            Layer = other.Layer;
            Name = other.Name;
            Type = other.Type;
            Actor = other.Actor;
            Input = other.Input == null ? null : new Connection(this, other.Input);
            Outputs = other.Outputs.Select(connection => new Connection(this, connection)).ToList();
            RecalculateWidth();
        }

        internal void FinishLoading(Graph graph)
        {
            if (_cachedLoadingNode.Input != null)
                Input = new Connection(graph, _cachedLoadingNode.Input);

            Outputs.AddRange(_cachedLoadingNode.Outputs.Select(connection => new Connection(graph, connection)));

            _cachedLoadingNode = null;
            RecalculateWidth();
        }

        public void MakeConnections(Graph graph)
        {
            Input?.FinishLoading(graph);

            foreach (var connection in Outputs)
                connection.FinishLoading(graph);
        }

        private void AddOutput(string text)
        {
            Outputs.Add(new Connection(this, NodeSide.Output, Outputs.Count, text));
            RecalculateWidth();
        }

        public void RecalculateWidth()
        {
            Width = WidthCalculator.Invoke(this);
        }

        public bool Pick(float x, float y)
        {
            return GetBounds().Pick(x, y);
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public void RemoveOutput(Connection connection)
        {
            Outputs.Remove(connection);
            RecalculateWidth();
            BuildConnections();
        }

        public void BuildConnections()
        {
            for (var i = 0; i < Outputs.Count; i++)
                Outputs[i].ConnectionIndex = i;
        }

        internal SavedNode Save()
        {
            return new SavedNode
            {
                Actor = Actor,
                Id = Id,
                Input = Input?.Save(),
                Outputs = Outputs.Select(connection => connection.Save()).ToList(),
                Type = Type,
                X = X,
                Y = Y,
                Layer = Layer,
                Name = Name
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Node node &&
                   Id.Equals(node.Id);
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<Guid>.Default.GetHashCode(Id);
        }

        public static bool operator ==(Node node1, Node node2)
        {
            return EqualityComparer<Node>.Default.Equals(node1, node2);
        }

        public static bool operator !=(Node node1, Node node2)
        {
            return !(node1 == node2);
        }
    }
}