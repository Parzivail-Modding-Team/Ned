using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Ned;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PFX.Util;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    internal class NodeRenderer
    {
        private readonly Dictionary<NodeInfo, Color> _colorMap = new Dictionary<NodeInfo, Color>();
        private readonly GridRenderer _grid;
        private readonly Color4 _placeholderTextColor = new Color4(1, 1, 1, 0.5f);
        private readonly MainWindow _window;

        public NodeRenderer(MainWindow window, GridRenderer grid)
        {
            _window = window;
            _grid = grid;

            _colorMap.Add(NodeInfo.None, Color.Black);

            _colorMap.Add(NodeInfo.Start, Color.LimeGreen);
            _colorMap.Add(NodeInfo.End, Color.IndianRed);

            _colorMap.Add(NodeInfo.NpcDialogue, Color.MediumPurple);
            _colorMap.Add(NodeInfo.PlayerDialogue, Color.LightSkyBlue);

            _colorMap.Add(NodeInfo.WaitForFlag, Color.Orange);
            _colorMap.Add(NodeInfo.SetFlag, Color.MediumSeaGreen);
            _colorMap.Add(NodeInfo.ClearFlag, Color.MediumVioletRed);

            _colorMap.Add(NodeInfo.HasQuest, Color.DarkOrange);
            _colorMap.Add(NodeInfo.StartQuest, Color.SteelBlue);
            _colorMap.Add(NodeInfo.CompleteQuest, Color.DarkOrchid);

            _colorMap.Add(NodeInfo.TriggerEvent, Color.DarkKhaki);
        }

        public int GetNodeWidth(Node node)
        {
            var width = 120;
            const int textPadding = 40;

            width = (int) Math.Max(_window.Font.MeasureString(node.Name).Width + textPadding, width);

            if (node.Input != null)
                width = (int) Math.Max(_window.Font.MeasureString(node.Input.Text).Width + textPadding, width);

            foreach (var connection in node.Outputs)
                width = (int) Math.Max(_window.Font.MeasureString(connection.Text).Width + textPadding, width);

            width = (int) (Math.Ceiling(width / (float) _grid.Pitch) * _grid.Pitch);

            return width;
        }

        private void RenderConnection(Connection connection, Connection end)
        {
            var b = end.GetBounds();
            RenderConnection(connection, new Vector2(b.X, b.Y));
        }

        public void RenderConnection(Connection connection, Vector2 end)
        {
            var v = new Vector2(200, 0);
            var bound = connection.GetBounds();
            var pos = new Vector2(bound.X, bound.Y);
            Fx.D2.CentripetalCatmullRomTo(connection.Side == NodeSide.Input ? pos + v : pos - v, pos, end,
                connection.Side == NodeSide.Input ? end - v : end + v);
        }

        public void RenderConnections(Node node)
        {
            if (!ScreenContains(node))
                return;
            GL.PushMatrix();
            GL.Translate(0, 0, -1);
            GL.Color3(Color.Gray);

            foreach (var connection in node.Outputs)
                if (connection.ConnectedNode != null)
                    RenderConnection(connection, connection.ConnectedNode);

            GL.PopMatrix();
        }

        private void RenderConnector(Connection connection)
        {
            GL.PushMatrix();

            var pickedForDeletion =
                _window.Selection.HoveringConnection == connection && _window.Keyboard[Key.ShiftLeft];
            var bound = connection.GetBounds();
            var r = bound.Radius;
            var twor = 2 * r;
            var halfr = r / 2;
            const int cxnBorderWidth = 2;

            GL.Color3(Color.White);
            GL.Enable(EnableCap.Texture2D);

            GL.PushMatrix();
            if (connection != TextBoxHandler.EditingConnection)
            {
                switch (connection.Side)
                {
                    case NodeSide.Input:
                        GL.Translate(bound.X + twor, bound.Y - r, 0.01);
                        RenderString(connection.Text);
                        break;
                    case NodeSide.Output:
                        var s = connection.Text;
                        GL.Translate(bound.X - twor - _window.Font.MeasureString(s).Width, bound.Y - r, 0.01);
                        RenderString(s);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                GL.Translate(0, 0, 0.01);
                TextBoxHandler.TextBox.RenderBackground();
                GL.Translate(0, 0, 0.01);
                TextBoxHandler.TextBox.RenderForeground();
            }

            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);

            GL.Color3(Color.DarkSlateGray);
            //Fx.D2.DrawSolidCircle(bound.X, bound.Y, r + 2);
            if (connection.Side == NodeSide.Input)
                Fx.D2.DrawSolidRoundRectangle(bound.X - r - cxnBorderWidth, bound.Y - r - cxnBorderWidth,
                    twor + 2 * cxnBorderWidth,
                    twor + 2 * cxnBorderWidth, r + cxnBorderWidth, 0, r + cxnBorderWidth, 0);
            else
                Fx.D2.DrawSolidRoundRectangle(bound.X - r - cxnBorderWidth, bound.Y - r - cxnBorderWidth,
                    twor + 2 * cxnBorderWidth,
                    twor + 2 * cxnBorderWidth, 0, r + cxnBorderWidth, 0, r + cxnBorderWidth);

            GL.Translate(0, 0, 0.01);

            GL.Color3(connection.Side == NodeSide.Input ? Color.DeepSkyBlue : Color.LimeGreen);
            Fx.D2.DrawSolidCircle(bound.X, bound.Y, r);

            if (_window.Selection.HoveringConnection != null && _window.Selection.DraggingConnection == connection &&
                _window.Selection.HoveringConnection.Side != _window.Selection.DraggingConnection.Side)
            {
                GL.PushMatrix();
                GL.Color3(Color.SlateGray);
                GL.Translate(0, 0, 0.01);
                Fx.D2.DrawSolidCircle(bound.X, bound.Y, halfr);
                GL.PopMatrix();
            }
            else if (connection.ConnectedNode != null)
            {
                GL.PushMatrix();
                GL.Color3(Color.DarkSlateGray);

                GL.Translate(0, 0, 0.01);
                Fx.D2.DrawSolidCircle(bound.X, bound.Y, halfr);
                GL.PopMatrix();
            }

            if (pickedForDeletion)
            {
                GL.PushMatrix();
                GL.Color3(Color.Red);

                GL.Translate(0, 0, 0.01);
                Fx.D2.DrawSolidCircle(bound.X, bound.Y, halfr);
                GL.PopMatrix();
            }

            GL.PopMatrix();
        }

        public void RenderNode(Node node)
        {
            if (!ScreenContains(node))
                return;
            const int borderRadius = 6;
            const int panelInset = 2;
            const float halfPanelInset = panelInset / 2f;

            var headerHeight = (int) (_window.Font.Common.LineHeight * 1.2f);
            var oneCanvasPixel = 1 / _window.Zoom;

            GL.PushMatrix();
            GL.Translate(0, 0, node.Layer);
            GL.Disable(EnableCap.Texture2D);

            if (_window.Selection.SelectedNodes.Contains(node))
            {
                GL.Color3(Color.White);
                Fx.D2.DrawSolidRoundRectangle(node.X - oneCanvasPixel, node.Y - oneCanvasPixel,
                    node.Width + 2 * oneCanvasPixel,
                    node.Height + 2 * oneCanvasPixel, borderRadius, borderRadius, borderRadius, borderRadius);
                GL.Translate(0, 0, 0.01);
                GL.Color3(Color.Black);
                MarchingAnts.Use();
                Fx.D2.DrawSolidRoundRectangle(node.X - oneCanvasPixel, node.Y - oneCanvasPixel,
                    node.Width + 2 * oneCanvasPixel,
                    node.Height + 2 * oneCanvasPixel, borderRadius, borderRadius, borderRadius, borderRadius);
                MarchingAnts.Release();
            }

            GL.Translate(0, 0, 0.01);

            GL.Color3(_colorMap.ContainsKey(node.NodeInfo) ? _colorMap[node.NodeInfo] : Color.Black);

            Fx.D2.DrawSolidRoundRectangle(node.X, node.Y, node.Width, node.Height, borderRadius, borderRadius,
                borderRadius, borderRadius);

            GL.Translate(0, 0, 0.01);

            GL.Color3(Color.DarkSlateGray);
            Fx.D2.DrawSolidRoundRectangle(node.X + panelInset, node.Y + headerHeight + panelInset,
                node.Width - 2 * panelInset, node.Height - headerHeight - 2 * panelInset,
                borderRadius - halfPanelInset, borderRadius - halfPanelInset, borderRadius - halfPanelInset,
                borderRadius - halfPanelInset);

            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);

            GL.PushMatrix();
            var headerOffset = (headerHeight + panelInset) / 2f - _window.Font.MeasureString(node.Name).Height / 2;
            GL.Translate((int) (node.X + 2 * panelInset), (int) (node.Y + headerOffset), 0.01);
            RenderString(node.Name);
            GL.PopMatrix();

            if (node.Input != null)
                RenderConnector(node.Input);

            foreach (var nodeOutput in node.Outputs)
                RenderConnector(nodeOutput);
            GL.PopMatrix();
        }

        private void RenderString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return;

            if (_window.Zoom >= 1)
            {
                _window.Font.RenderString(s);
            }
            else
            {
                GL.PushAttrib(AttribMask.EnableBit);
                GL.Disable(EnableCap.Texture2D);
                var size = _window.Font.MeasureString(s);
                var halfHeight = _window.Font.Common.LineHeight / 2;
                GL.Color4(_placeholderTextColor);
                Fx.D2.DrawSolidRoundRectangle(0, 0, size.Width, _window.Font.Common.LineHeight, halfHeight, halfHeight,
                    halfHeight,
                    halfHeight);
                GL.PopAttrib();
            }
        }

        private bool ScreenContains(Node node)
        {
            var nodeRect = node.GetBounds();
            var nodeRectOthers = node.Outputs.Select(connection => connection?.ConnectedNode?.ParentNode?.GetBounds())
                .Where(rectangle => rectangle != null);

            var screenTopLeft = _window.ScreenToCanvasSpace(Vector2.Zero);
            var screenBotRight = _window.ScreenToCanvasSpace(new Vector2(_window.Width, _window.Height));
            var screen = new Rectangle(screenTopLeft.X, screenTopLeft.Y,
                screenBotRight.X - screenTopLeft.X, screenBotRight.Y - screenTopLeft.Y);

            return nodeRect.Intersects(screen) || nodeRectOthers.Any(node1 => node1.Intersects(screen));
        }
    }
}