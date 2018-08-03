using System;
using System.Collections.Generic;

namespace Ned
{
    public class Connection
    {
        public readonly Guid Id = Guid.NewGuid();

        private Guid? _cachedLoadingConnection;
        private string _text;

        public NodeSide Side { get; }
        public int ConnectionIndex { get; set; }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                ParentNode?.RecalculateWidth();
            }
        }

        public bool CanEditName { get; }

        public Node ParentNode { get; set; }
        public Connection ConnectedNode { get; set; }

        public Connection(Node parentNode, NodeSide side, int connectionIndex, string text, bool canEditName = true)
        {
            ParentNode = parentNode;
            Side = side;
            ConnectionIndex = connectionIndex;
            Text = text;
            CanEditName = canEditName;
        }

        internal Connection(Graph graph, SavedConnection connection)
        {
            Id = connection.Id;
            Side = connection.Side;
            ConnectionIndex = connection.ConnectionIndex;
            Text = connection.Text;
            CanEditName = connection.CanEditName;

            ParentNode = graph.GetNode(connection.ParentNode);

            _cachedLoadingConnection = connection.ConnectedNode;
        }

        public Connection(Node parent, Connection other)
        {
            Side = other.Side;
            ConnectionIndex = other.ConnectionIndex;
            Text = other.Text;
            ParentNode = parent;
            ConnectedNode = other.ConnectedNode == null ? null : new Connection(parent, other.ConnectedNode);
            CanEditName = other.CanEditName;
        }

        public void ConnectTo(Connection other)
        {
            if (other.Side == Side || other.ParentNode == ParentNode)
                return;

            if (Side == NodeSide.Output)
                ConnectedNode = other;
            else
                other.ConnectedNode = this;
        }

        public override bool Equals(object obj)
        {
            return obj is Connection connection &&
                   Id.Equals(connection.Id);
        }

        internal void FinishLoading(Graph graph)
        {
            ConnectedNode = _cachedLoadingConnection.HasValue
                ? graph.GetConnection(_cachedLoadingConnection.Value)
                : null;

            _cachedLoadingConnection = null;
        }

        public Circle GetBounds()
        {
            switch (Side)
            {
                case NodeSide.Input:
                    return new Circle(ParentNode.X, ParentNode.Y + (ConnectionIndex + 1) * 20 + 20, 6);
                case NodeSide.Output:
                    return new Circle(ParentNode.X + ParentNode.Width, ParentNode.Y + (ConnectionIndex + 1) * 20 + 20,
                        6);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<Guid>.Default.GetHashCode(Id);
        }

        public static bool operator ==(Connection connection1, Connection connection2)
        {
            return EqualityComparer<Connection>.Default.Equals(connection1, connection2);
        }

        public static bool operator !=(Connection connection1, Connection connection2)
        {
            return !(connection1 == connection2);
        }

        internal SavedConnection Save()
        {
            return new SavedConnection
            {
                Side = Side,
                ConnectedNode = ConnectedNode?.Id,
                ConnectionIndex = ConnectionIndex,
                Id = Id,
                ParentNode = ParentNode.Id,
                Text = Text,
                CanEditName = CanEditName
            };
        }
    }
}