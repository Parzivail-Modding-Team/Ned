using System;

namespace Ned
{
    public class Connection
    {
        public readonly Guid Id = Guid.NewGuid();

        public NodeSide Side { get; }
        public int ConnectionIndex { get; set; }
        public string Text { get; set; }

        public Node ParentNode { get; set; }
        public Connection ConnectedNode { get; set; }

        public Connection(Node parentNode, NodeSide side, int connectionIndex, string text)
        {
            ParentNode = parentNode;
            Side = side;
            ConnectionIndex = connectionIndex;
            Text = text;
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

        public void ReleaseConnection()
        {
            if (Side == NodeSide.Output)
                ConnectedNode = null;
        }

        public Circle GetBounds()
        {
            switch (Side)
            {
                case NodeSide.Input:
                    return new Circle(ParentNode.X, ParentNode.Y + (ConnectionIndex + 1) * 24 + 18, 6);
                case NodeSide.Output:
                    return new Circle(ParentNode.X + ParentNode.Width, ParentNode.Y + (ConnectionIndex + 1) * 24 + 18, 6);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}