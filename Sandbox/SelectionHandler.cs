using System;
using System.Collections.Generic;
using System.Linq;
using Ned;
using OpenTK;

namespace Sandbox
{
    class SelectionHandler
    {
        private MainWindow _window;

        public Rectangle SelectionRectangle;
        public readonly List<Node> SelectedNodes = new List<Node>();
        private readonly List<Node> CopiedNodes = new List<Node>();
        private readonly List<Vector2> CopiedNodeOffsets = new List<Vector2>();

        public Node SingleSelectedNode  => SelectedNodes.Count == 1 ? SelectedNodes[0] : null;
        public bool IsClipboardEmpty => CopiedNodes.Count == 0;
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
            CopiedNodes.Clear();
            CopiedNodeOffsets.Clear();

            CopiedNodes.AddRange(SelectedNodes.Select(node => new Node(node)));

            var minX = CopiedNodes.Min(node => node.X);
            var minY = CopiedNodes.Min(node => node.Y);

            CopiedNodeOffsets.AddRange(CopiedNodes.Select(node => new Vector2(node.X - minX, node.Y - minY)));
        }

        public void Cut()
        {
            Copy();
            Delete();
        }

        public void Paste(float x, float y)
        {
            var v = _window.ScreenToCanvasSpace(new Vector2(x, y));

            for (var i = 0; i < CopiedNodes.Count; i++)
            {
                var copiedNode = CopiedNodes[i];
                var offset = CopiedNodeOffsets[i];

                copiedNode.X = v.X + offset.X;
                copiedNode.Y = v.Y + offset.Y;

                _window.Graph.Add(new Node(copiedNode));
            }
        }
    }
}
