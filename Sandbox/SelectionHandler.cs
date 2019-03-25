using System;
using System.Collections.Generic;
using System.Linq;
using Ned;
using OpenTK;
using OpenTK.Input;

namespace Sandbox
{
    public class SelectionHandler
    {
        private readonly List<Vector2> _copiedNodeOffsets = new List<Vector2>();
        private readonly List<Node> _copiedNodes = new List<Node>();
        private readonly MainWindow _window;
        public readonly List<Node> SelectedNodes = new List<Node>();
        private Rectangle _selectionRectangle;

        public bool CreatingConnectedNode;
        public Connection DraggingConnection;
        public Connection HoveringConnection;
        public bool IsDraggingNode;

        public Rectangle SelectionRectangle
        {
            get => _selectionRectangle;
            set
            {
                _selectionRectangle = value;
                if (value != null)
                    Select();
            }
        }

        public Node SingleSelectedNode => SelectedNodes.Count == 1 ? SelectedNodes[0] : null;
        public bool IsClipboardEmpty => _copiedNodes.Count == 0;
        public bool OneOrNoneSelected => SelectedNodes.Count <= 1;

        public bool IsSpecialSelectActive => _window.Keyboard[Key.ControlLeft];

        public SelectionHandler(MainWindow window)
        {
            _window = window;
        }

        public void Clear()
        {
            SelectedNodes.Clear();
        }

        public void Copy()
        {
            _copiedNodes.Clear();
            _copiedNodeOffsets.Clear();

            _copiedNodes.AddRange(SelectedNodes.Where(node => node.NodeInfo.CanEditNode).Select(node =>
            {
                var n = new Node(node);
                foreach (var connection in n.Outputs)
                    connection.ConnectedNode = null;
                return n;
            }));

            if (_copiedNodes.Count == 0)
                return;

            var minX = _copiedNodes.Min(node => node.X);
            var minY = _copiedNodes.Min(node => node.Y);

            _copiedNodeOffsets.AddRange(_copiedNodes.Select(node => new Vector2(node.X - minX, node.Y - minY)));
        }

        public void Cut()
        {
            Copy();
            Delete();
        }

        public void Delete()
        {
            var nodes = _window.Graph.Where(node => SelectedNodes.Contains(node) && node.NodeInfo.CanEditNode)
                .ToList();
            foreach (var node in nodes)
                _window.Graph.Remove(node);
        }

        public void Paste(float x, float y, bool snap, float snapPitch)
        {
            var v = new Vector2(x, y);

            for (var i = 0; i < _copiedNodes.Count; i++)
            {
                var copiedNode = _copiedNodes[i];
                var offset = _copiedNodeOffsets[i];

                copiedNode.X = v.X + offset.X;
                copiedNode.Y = v.Y + offset.Y;

                if (snap)
                {
                    copiedNode.X = (int) (Math.Floor(copiedNode.X / snapPitch) * snapPitch);
                    copiedNode.Y = (int) (Math.Floor(copiedNode.Y / snapPitch) * snapPitch);
                }

                _window.Graph.Add(new Node(copiedNode));
            }
        }

        public void Select()
        {
            var mode = IsSpecialSelectActive ? SelectionMode.Additive : SelectionMode.Normal;
            switch (mode)
            {
                case SelectionMode.Normal:
                    Clear();
                    SelectedNodes.AddRange(_window.Graph.Where(node =>
                        SelectionRectangle.Intersects(node.GetBounds())));
                    break;
                case SelectionMode.Additive:
                    SelectedNodes.AddRange(_window.Graph.Where(node =>
                        SelectionRectangle.Intersects(node.GetBounds())));
                    break;
                case SelectionMode.Subtractive:
                    SelectedNodes.RemoveAll(node => SelectionRectangle.Intersects(node.GetBounds()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public void Select(Node node)
        {
            Clear();
            SelectedNodes.Add(node);
        }
    }
}