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
        public Node ConnectedNode { get; set; }

        public Connection(Node parentNode, NodeSide side, int connectionIndex, string text)
        {
            ParentNode = parentNode;
            Side = side;
            ConnectionIndex = connectionIndex;
            Text = text;
        }

        public void ConnectTo(Node other)
        {
            ConnectedNode = other;
        }
    }

    public enum NodeSide
    {
        Input,
        Output
    }
}