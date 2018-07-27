using System;
using System.Collections.Generic;
using System.Linq;
using Ned;
using OpenTK;

namespace Sandbox
{
    class SelectionHandler
    {
        private readonly MainWindow _window;

        public Rectangle SelectionRectangle;
        public readonly List<Node> SelectedNodes = new List<Node>();
        private readonly List<Node> _copiedNodes = new List<Node>();
        private readonly List<Vector2> _copiedNodeOffsets = new List<Vector2>();

        public Node SingleSelectedNode  => SelectedNodes.Count == 1 ? SelectedNodes[0] : null;
        public bool IsClipboardEmpty => _copiedNodes.Count == 0;
        public bool OneOrNoneSelected => SelectedNodes.Count <= 1;

        public SelectionHandler(MainWindow window)
        {
            _window = window;
        }

        public void Select(Graph graph, SelectionMode mode)
        {
            _window.DialogEditor.ChangeSelectionTo(null);

            switch (mode)
            {
                case SelectionMode.Normal:
                    SelectedNodes.Clear();
                    SelectedNodes.AddRange(graph.Where(node => SelectionRectangle.Pick(node.X, node.Y)));
                    break;
                case SelectionMode.Additive:
                    SelectedNodes.AddRange(graph.Where(node => SelectionRectangle.Pick(node.X, node.Y)));
                    break;
                case SelectionMode.Subtractive:
                    SelectedNodes.RemoveAll(node => SelectionRectangle.Pick(node.X, node.Y));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public void Select(Node node)
        {
            SelectedNodes.Clear();
            SelectedNodes.Add(node);
            _window.DialogEditor.ChangeSelectionTo(node);
        }

        public void Delete()
        {
            _window.Graph.RemoveAll(node => SelectedNodes.Contains(node) && node.Type == NodeType.Flow);
        }

        public void Copy()
        {
            _copiedNodes.Clear();
            _copiedNodeOffsets.Clear();

            _copiedNodes.AddRange(SelectedNodes.Select(node => new Node(node)));

            var minX = _copiedNodes.Min(node => node.X);
            var minY = _copiedNodes.Min(node => node.Y);

            _copiedNodeOffsets.AddRange(_copiedNodes.Select(node => new Vector2(node.X - minX, node.Y - minY)));
        }

        public void Cut()
        {
            Copy();
            Delete();
        }

        public void Paste(float x, float y)
        {
            var v = _window.ScreenToCanvasSpace(new Vector2(x, y));

            for (var i = 0; i < _copiedNodes.Count; i++)
            {
                var copiedNode = _copiedNodes[i];
                var offset = _copiedNodeOffsets[i];

                copiedNode.X = v.X + offset.X;
                copiedNode.Y = v.Y + offset.Y;

                _window.Graph.Add(new Node(copiedNode));
            }
        }
    }
}
