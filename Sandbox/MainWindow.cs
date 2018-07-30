using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Ned;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PFX;
using PFX.BmFont;
using PFX.Util;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    public class MainWindow : GameWindow
    {
        private readonly Dictionary<Node, Vector2> _draggingNodeOffset = new Dictionary<Node, Vector2>();
        private readonly Profiler _profiler = new Profiler();
        private ContextMenu _contextMenu;
        private bool _draggingBackground;
        private Connection _draggingConnection;
        private Func<Connection, bool> _draggingConnectionPredicate;
        private bool _draggingNode;
        private Sparkline _fpsSparkline;
        private Grid _grid;
        private KeyboardState _keyboard;
        private Vector2 _mouseCanvasSpace = Vector2.Zero;
        private Connection _pickedConnection;
        private Dictionary<string, TimeSpan> _profile = new Dictionary<string, TimeSpan>();
        private Sparkline _renderTimeSparkline;
        private SelectionHandler _selectionHandler;
        private bool _shouldDie;

        public FormDialogueEditor DialogEditor;
        public BitmapFont Font;
        public Vector2 MouseScreenSpace = Vector2.Zero;
        public float Zoom = 1;

        public MainWindow() : base(800, 600)
        {
            // Wire up window
            Load += HandleLoad;
            Closing += HandleClose;
            Resize += HandleResize;
            UpdateFrame += Update;
            RenderFrame += Render;
            MouseWheel += OnMouseWheel;

            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;

            KeyDown += OnKeyDown;

            //Title = $"{EmbeddedFiles.AppName} | {EmbeddedFiles.Title_Unsaved}";
            //Icon = EmbeddedFiles.logo;
        }

        public Graph Graph => DialogEditor.GetGraph();

        private void CreateContextMenu(Node context)
        {
            _contextMenu = new ContextMenu(MouseScreenSpace.X + 1, MouseScreenSpace.Y + 1, 100);
            if (context == null)
            {
                _contextMenu.Add(new ContextMenuItem(this, "Add NPC", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeType.Flow, Actor.NPC, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Add Player", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeType.Flow, Actor.Player, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                if (!_selectionHandler.IsClipboardEmpty)
                    _contextMenu.Add(new ContextMenuItem(this, "CTRL+V", "Paste",
                        item => _selectionHandler.Paste(_contextMenu.X, _contextMenu.Y, !_keyboard[Key.ShiftLeft],
                            _grid.Pitch)));
            }
            else
            {
                if (context.Type != NodeType.Flow) return;

                if (_selectionHandler.OneOrNoneSelected)
                    _selectionHandler.Select(context);

                _contextMenu.Add(new ContextMenuItem(this, "DEL", "Delete", item => _selectionHandler.Delete()));
                _contextMenu.Add(new ContextMenuItem(this, "CTRL+X", "Cut", item => _selectionHandler.Cut()));
                _contextMenu.Add(new ContextMenuItem(this, "CTRL+C", "Copy", item => _selectionHandler.Copy()));
            }
        }

        private void HandleClose(object sender, CancelEventArgs e)
        {
            DialogEditor.Close();
        }

        private void HandleLoad(object sender, EventArgs e)
        {
            // Set up caps
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);

            // Set up blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            // Set background color
            GL.ClearColor(Color.White);

            // Load fonts
            Font = BitmapFont.LoadBinaryFont("dina", FontBank.FontDina, FontBank.BmDina);

            // Load sparklines
            _fpsSparkline = new Sparkline(Font, $"0-{(int) TargetRenderFrequency}fps", 50,
                (float) TargetRenderFrequency, Sparkline.SparklineStyle.Area);
            _renderTimeSparkline = new Sparkline(Font, "0-50ms", 50, 50, Sparkline.SparklineStyle.Area);

            // Init keyboard to ensure first frame won't NPE
            _keyboard = Keyboard.GetState();

            Node.WidthCalculator = GetNodeWidth;

            _grid = new Grid(this);

            _selectionHandler = new SelectionHandler(this);

            DialogEditor = new FormDialogueEditor(this);
            DialogEditor.Show();

            CreateContextMenu(null);

            _draggingConnectionPredicate = connection =>
                _draggingConnection == null || _draggingConnection.ParentNode != connection.ParentNode &&
                _draggingConnection.Side != connection.Side;

            Lumberjack.Info("Window Loaded.");
        }

        private int GetNodeWidth(Node node)
        {
            var width = 90;

            width = (int) Math.Max(Font.MeasureString(node.Name).Width + 40, width);

            if (node.Input != null)
                width = (int) Math.Max(Font.MeasureString(node.Input.Text).Width + 40, width);

            foreach (var connection in node.Outputs)
                width = (int) Math.Max(Font.MeasureString(connection.Text).Width + 40, width);

            return width;
        }

        private void HandleResize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            // Set up 2D mode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -1000, 100);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private bool IsControlPressed()
        {
            return _keyboard[Key.ControlLeft] || _keyboard[Key.ControlRight];
        }

        public void Kill()
        {
            _shouldDie = true;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!Focused)
                return;

            if (e.Key == Key.Delete) _selectionHandler.Delete();

            if (e.Control && e.Key == Key.R)
            {
                SetZoom(1);
                if (e.Shift)
                    _grid.Offset = Vector2.Zero;
            }

            if (e.Control && e.Key == Key.C) _selectionHandler.Copy();

            if (e.Control && e.Key == Key.X) _selectionHandler.Cut();

            if (e.Control && e.Key == Key.V)
                _selectionHandler.Paste(_mouseCanvasSpace.X, _mouseCanvasSpace.Y, !_keyboard[Key.ShiftLeft],
                    _grid.Pitch);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            switch (mouseButtonEventArgs.Button)
            {
                case MouseButton.Right:
                    CreateContextMenu(Graph.PickNode(_mouseCanvasSpace.X, _mouseCanvasSpace.Y));
                    _contextMenu.Visible = true;
                    break;
                case MouseButton.Middle:
                    _draggingBackground = true;
                    break;
                case MouseButton.Left:
                    var x = _mouseCanvasSpace.X;
                    var y = _mouseCanvasSpace.Y;

                    if (_contextMenu.Visible)
                    {
                        foreach (var menuItem in _contextMenu)
                        {
                            if (!menuItem.Pick(MouseScreenSpace.X, MouseScreenSpace.Y)) continue;

                            menuItem.Action.Invoke(menuItem);
                            _contextMenu.Visible = false;
                            return;
                        }

                        _contextMenu.Visible = false;
                    }

                    var clickedConnection = Graph.PickConnection(x, y, _draggingConnectionPredicate);

                    if (clickedConnection != null)
                    {
                        _selectionHandler.Select(null);

                        if (_keyboard[Key.ShiftLeft])
                            Graph.ClearConnectionsFrom(clickedConnection);
                        else
                            _draggingConnection = clickedConnection;
                        return;
                    }

                    var clickedNode = Graph.PickNode(x, y);

                    if (clickedNode != null)
                    {
                        if (!_selectionHandler.SelectedNodes.Contains(clickedNode))
                            _selectionHandler.Select(clickedNode);

                        _draggingNode = true;

                        _draggingNodeOffset.Clear();
                        foreach (var selectedNode in _selectionHandler.SelectedNodes)
                            _draggingNodeOffset.Add(selectedNode, new Vector2(x - selectedNode.X, y - selectedNode.Y));

                        return;
                    }

                    if (!IsControlPressed())
                        _selectionHandler.Select(null);

                    _selectionHandler.SelectionRectangle =
                        new Rectangle(_mouseCanvasSpace.X, _mouseCanvasSpace.Y, 1, 1);
                    break;
            }
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
        {
            MouseScreenSpace = new Vector2(mouseMoveEventArgs.X, mouseMoveEventArgs.Y);
            _mouseCanvasSpace = ScreenToCanvasSpace(MouseScreenSpace);

            if (_draggingBackground)
            {
                _grid.Offset += new Vector2(mouseMoveEventArgs.XDelta, mouseMoveEventArgs.YDelta) / Zoom;
                return;
            }

            if (_selectionHandler.SelectionRectangle != null)
            {
                _selectionHandler.SelectionRectangle.Width =
                    _mouseCanvasSpace.X - _selectionHandler.SelectionRectangle.X;
                _selectionHandler.SelectionRectangle.Height =
                    _mouseCanvasSpace.Y - _selectionHandler.SelectionRectangle.Y;
                return;
            }

            if (_draggingNode && _selectionHandler.SelectedNodes.Count > 0)
                foreach (var node in _selectionHandler.SelectedNodes)
                {
                    node.X = _mouseCanvasSpace.X - _draggingNodeOffset[node].X;
                    node.Y = _mouseCanvasSpace.Y - _draggingNodeOffset[node].Y;

                    if (!_keyboard[Key.ShiftLeft])
                    {
                        node.X = (int) (Math.Floor(node.X / _grid.Pitch) * _grid.Pitch);
                        node.Y = (int) (Math.Floor(node.Y / _grid.Pitch) * _grid.Pitch);
                    }
                }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            switch (mouseButtonEventArgs.Button)
            {
                case MouseButton.Middle:
                    _draggingBackground = false;
                    break;
                case MouseButton.Left:
                    if (_draggingConnection != null)
                    {
                        var picked = Graph.PickConnection(_mouseCanvasSpace.X, _mouseCanvasSpace.Y,
                            _draggingConnectionPredicate);
                        if (picked != null)
                            _draggingConnection.ConnectTo(picked);
                    }

                    _selectionHandler.SelectionRectangle = null;
                    _draggingNode = false;
                    _draggingConnection = null;
                    break;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var z = Zoom;

            var v = _keyboard[Key.ShiftLeft] ? 1.5f : 2f;
            if (Math.Sign(e.DeltaPrecise) > 0)
                z *= v;
            else
                z /= v;

            SetZoom(z);
        }

        private void Render(object sender, FrameEventArgs e)
        {
            // Start profiling
            _profiler.Start("render");

            // Update sparklines
            if (_profile.ContainsKey("render"))
                _renderTimeSparkline.Enqueue((float) _profile["render"].TotalMilliseconds);

            _fpsSparkline.Enqueue((float) RenderFrequency);

            // Reset the view
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            GL.PushMatrix();
            GL.Scale(Zoom, Zoom, 0.01f);

            GL.Disable(EnableCap.Texture2D);

            GL.PushMatrix();
            GL.Translate(0, 0, -50);

            GL.Color3(Color.Gray);
            GL.PointSize(1);

            _grid.Draw();

            GL.PopMatrix();

            GL.Translate(_grid.Offset.X, _grid.Offset.Y, 0);
            GL.LineWidth(2);

            GL.Color3(Color.Black);
            Fx.D2.DrawLine(-10, 0, 10, 0);
            Fx.D2.DrawLine(0, -10, 0, 10);

            GL.Color3(Color.DarkGray);
            if (_selectionHandler.SelectionRectangle != null)
                Fx.D2.DrawWireRectangle(_selectionHandler.SelectionRectangle.X, _selectionHandler.SelectionRectangle.Y,
                    _selectionHandler.SelectionRectangle.Width, _selectionHandler.SelectionRectangle.Height);

            GL.LineWidth(3);
            if (_draggingConnection != null)
            {
                var end = _mouseCanvasSpace;

                var picked = _pickedConnection;
                if (picked != null && picked.Side != _draggingConnection.Side)
                {
                    var b = picked.GetBounds();
                    end = new Vector2(b.X, b.Y);
                }

                GL.Color3(Color.Gray);
                RenderConnection(_draggingConnection, end);
            }

            foreach (var node in Graph)
            {
                if (!ScreenContains(node))
                    continue;
                RenderNode(node);
                RenderConnections(node);
            }

            GL.PopMatrix();

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);

            _contextMenu.Render();

            GL.Color4(0, 0, 0, 1f);
            if (_keyboard[Key.D] && Focused)
            {
                // Static diagnostic header
                GL.PushMatrix();
                Font.RenderString($"FPS: {(int) Math.Round(RenderFrequency)}\n" +
                                  $"Render Time: {(int) _profile["render"].TotalMilliseconds}ms\n" +
                                  $"Zoom: {Zoom}\n" +
                                  $"Nodes: {Graph.Count}", false);

                // Sparklines
                GL.Translate(5, (int) (Height - Font.Common.LineHeight * 1.4f * 2), 0);
                _fpsSparkline.Render(Color.Blue, Color.LimeGreen);
                GL.Translate(0, (int) (Font.Common.LineHeight * 1.4f), 0);
                _renderTimeSparkline.Render(Color.Blue, Color.LimeGreen);
                GL.PopMatrix();
            }
            else
            {
                // Info footer
                GL.PushMatrix();
                Font.RenderString($"Ned - Development Build");
                GL.Translate(0, Height - Font.Common.LineHeight, 0);
                Font.RenderString("PRESS 'D' FOR DIAGNOSTICS");
                GL.PopMatrix();
            }

            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.PopMatrix();

            // Swap the graphics buffer
            SwapBuffers();

            // Stop profiling and get the results
            _profiler.End();
            _profile = _profiler.Reset();
        }

        private bool ScreenContains(Node node)
        {
            var nodeRect = node.GetBounds();
            var nodeRectOthers = node.Outputs.Select(connection => connection?.ConnectedNode?.ParentNode?.GetBounds())
                .Where(rectangle => rectangle != null);

            var screenTopLeft = ScreenToCanvasSpace(Vector2.Zero);
            var screenBotRight = ScreenToCanvasSpace(new Vector2(Width, Height));
            var screen = new Rectangle(screenTopLeft.X, screenTopLeft.Y,
                screenBotRight.X - screenTopLeft.X, screenBotRight.Y - screenTopLeft.Y);

            return nodeRect.Intersects(screen) || nodeRectOthers.Any(node1 => node1.Intersects(screen));
        }

        private void RenderConnection(Connection connection, Connection end)
        {
            var b = end.GetBounds();
            RenderConnection(connection, new Vector2(b.X, b.Y));
        }

        private void RenderConnection(Connection connection, Vector2 end)
        {
            var v = new Vector2(200, 0);
            var bound = connection.GetBounds();
            var pos = new Vector2(bound.X, bound.Y);
            Fx.D2.CentripetalCatmullRomTo(connection.Side == NodeSide.Input ? pos + v : pos - v, pos, end,
                connection.Side == NodeSide.Input ? end - v : end + v);
        }

        private void RenderConnections(Node node)
        {
            GL.PushMatrix();
            GL.Translate(0, 0, -10);
            GL.Color3(Color.Gray);

            foreach (var connection in node.Outputs)
                if (connection.ConnectedNode != null)
                    RenderConnection(connection, connection.ConnectedNode);

            GL.PopMatrix();
        }

        private void RenderConnector(Connection connection)
        {
            GL.PushMatrix();

            var pickedForDeletion = _pickedConnection == connection && _keyboard[Key.ShiftLeft];
            var bound = connection.GetBounds();
            var r = bound.Radius;
            var twor = 2 * r;
            var halfr = r / 2;

            GL.Color3(Color.White);
            GL.Enable(EnableCap.Texture2D);

            GL.PushMatrix();
            switch (connection.Side)
            {
                case NodeSide.Input:
                    GL.Translate(bound.X + twor, bound.Y - r, 0.01);
                    RenderString(connection.Text);
                    break;
                case NodeSide.Output:
                    var s = connection.Text;
                    GL.Translate(bound.X - twor - Font.MeasureString(s).Width, bound.Y - r, 0.01);
                    RenderString(s);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);

            GL.Color3(Color.DarkSlateGray);
            //Fx.D2.DrawSolidCircle(bound.X, bound.Y, r + 2);
            if (connection.Side == NodeSide.Input)
                Fx.D2.DrawSolidRoundRectangle(bound.X - r - 2, bound.Y - r - 2, twor + 4, twor + 4, r + 2, 0, r + 2, 0);
            else
                Fx.D2.DrawSolidRoundRectangle(bound.X - r - 2, bound.Y - r - 2, twor + 4, twor + 4, 0, r + 2, 0, r + 2);

            GL.Translate(0, 0, 0.01);

            GL.Color3(connection.Side == NodeSide.Input ? Color.DeepSkyBlue : Color.LimeGreen);
            Fx.D2.DrawSolidCircle(bound.X, bound.Y, r);

            if (_pickedConnection != null && _draggingConnection == connection &&
                _pickedConnection.Side != _draggingConnection.Side)
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

        private void RenderNode(Node node)
        {
            const int borderRadius = 6;

            GL.PushMatrix();
            GL.Translate(0, 0, node.Layer);
            GL.Disable(EnableCap.Texture2D);

            if (_selectionHandler.SelectedNodes.Contains(node))
            {
                GL.Color3(Color.White);
                Fx.D2.DrawSolidRoundRectangle(node.X - 1 / Zoom, node.Y - 1 / Zoom, node.Width + 2 / Zoom,
                    node.Height + 2 / Zoom, borderRadius, borderRadius, borderRadius, borderRadius);
                GL.Translate(0, 0, 0.01);
                GL.Color3(Color.Black);
                MarchingAnts.Use();
                Fx.D2.DrawSolidRoundRectangle(node.X - 1 / Zoom, node.Y - 1 / Zoom, node.Width + 2 / Zoom,
                    node.Height + 2 / Zoom, borderRadius, borderRadius, borderRadius, borderRadius);
                MarchingAnts.Release();
            }

            GL.Translate(0, 0, 0.01);

            switch (node.Type)
            {
                case NodeType.End:
                    GL.Color3(Color.IndianRed);
                    break;
                case NodeType.Start:
                    GL.Color3(Color.LimeGreen);
                    break;
                case NodeType.Flow:
                    switch (node.Actor)
                    {
                        case Actor.None:
                            GL.Color3(Color.Black);
                            break;
                        case Actor.NPC:
                            GL.Color3(Color.MediumPurple);
                            break;
                        case Actor.Player:
                            GL.Color3(Color.LightSkyBlue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Fx.D2.DrawSolidRoundRectangle(node.X, node.Y, node.Width, node.Height, borderRadius, borderRadius,
                borderRadius, borderRadius);

            GL.Translate(0, 0, 0.01);

            GL.Color3(Color.DarkSlateGray);
            Fx.D2.DrawSolidRoundRectangle(node.X + 2, node.Y + 20, node.Width - 4, node.Height - 20 - 2,
                borderRadius - 1, borderRadius - 1, borderRadius - 1, borderRadius - 1);

            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);

            GL.PushMatrix();
            GL.Translate(node.X + 4, node.Y + 4, 0.01);
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
            if (Zoom >= 0.5)
            {
                Font.RenderString(s);
            }
            else
            {
                GL.PushAttrib(AttribMask.EnableBit);
                GL.Disable(EnableCap.Texture2D);
                var size = Font.MeasureString(s);
                var halfHeight = size.Height / 2;
                Fx.D2.DrawSolidRoundRectangle(0, 0, size.Width, size.Height, halfHeight, halfHeight, halfHeight,
                    halfHeight);
                GL.PopAttrib();
            }
        }

        public Vector2 ScreenToCanvasSpace(Vector2 input)
        {
            var v = input - _grid.Offset * Zoom;
            var temp = Vector3.TransformVector(new Vector3(v), Matrix4.CreateScale(1f / Zoom));
            return temp.Xy;
        }

        private void SetZoom(float zoom)
        {
            var zoomBefore = Zoom;
            Zoom = zoom;

            if (Zoom > 1)
                Zoom = (float) Math.Round(Zoom);

            if (Zoom > 5)
                Zoom = 5;
            else if (Zoom < 0.1)
                Zoom = 0.1f;

            var size = new Vector2(Width, Height);
            _grid.Offset -= (size / zoomBefore - size / Zoom) / 2;
            _grid.Offset = new Vector2((int) _grid.Offset.X, (int) _grid.Offset.Y);
        }

        private void Update(object sender, FrameEventArgs e)
        {
            // Grab the new keyboard state
            _keyboard = Keyboard.GetState();

            if (_shouldDie)
                Exit();

            MarchingAnts.Update();

            if (_selectionHandler.SelectionRectangle != null)
            {
                _selectionHandler.Select(Graph, IsControlPressed() ? SelectionMode.Additive : SelectionMode.Normal);
                return;
            }

            _pickedConnection = Graph.PickConnection(_mouseCanvasSpace.X, _mouseCanvasSpace.Y);
        }
    }
}