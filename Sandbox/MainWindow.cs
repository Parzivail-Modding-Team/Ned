using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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
        private static FormDialogueEditor _dialogEditor;
        private static Graph Graph => _dialogEditor.GetGraph();

        public static Node SelectedNode
        {
            get => SelectedNodes.Count == 1 ? SelectedNodes[0] : null;
            private set
            {
                SelectedNodes.Clear();
                if (value != null)
                    SelectedNodes.Add(value);
            }
        }

        public static List<Node> SelectedNodes = new List<Node>();

        private static Node _copiedNode;
        private static bool _draggingNode;
        private static Dictionary<Node, Vector2> _draggingNodeOffset = new Dictionary<Node, Vector2>();
        private static Rectangle _selectionRectangle;

        private static Connection _draggingConnection;
        private static readonly Func<Connection, bool> DraggingConnectionPredicate = connection => _draggingConnection == null || _draggingConnection.ParentNode != connection.ParentNode && _draggingConnection.Side != connection.Side;

        /*
         * Window-related
         */
        private bool _shouldDie;
        private Sparkline _fpsSparkline;
        private Sparkline _renderTimeSparkline;
        private Grid _grid;
        private readonly Profiler _profiler = new Profiler();
        private static KeyboardState _keyboard;
        private static Vector2 _mouse = Vector2.Zero;
        private static BitmapFont _font;
        private static readonly byte[] StippleDiagonalLines = { 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88 };
        private Dictionary<string, TimeSpan> _profile = new Dictionary<string, TimeSpan>();

        private static bool _draggingBackground;

        public float Zoom { get; private set; } = 1;

        public MainWindow() : base(800, 600)
        {
            // Wire up window
            Load += LoadHandler;
            Closing += CloseHandler;
            Resize += ResizeHandler;
            UpdateFrame += UpdateHandler;
            RenderFrame += RenderHandler;
            MouseWheel += WindowVisualize_MouseWheel;

            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;

            KeyDown += OnKeyDown;

            //Title = $"{EmbeddedFiles.AppName} | {EmbeddedFiles.Title_Unsaved}";
            //Icon = EmbeddedFiles.logo;
        }

        private void LoadHandler(object sender, EventArgs e)
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
            _font = BitmapFont.LoadBinaryFont("dina", FontBank.FontDina, FontBank.BmDina);

            // Load sparklines
            _fpsSparkline = new Sparkline(_font, $"0-{(int)TargetRenderFrequency}fps", 50,
                (float)TargetRenderFrequency, Sparkline.SparklineStyle.Area);
            _renderTimeSparkline = new Sparkline(_font, "0-50ms", 50, 50, Sparkline.SparklineStyle.Area);

            _grid = new Grid(this);

            // Init keyboard to ensure first frame won't NPE
            _keyboard = Keyboard.GetState();

            _dialogEditor = new FormDialogueEditor(this);
            _dialogEditor.Show();

            Lumberjack.Info("Window Loaded.");
        }

        private void WindowVisualize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var z = Zoom;

            var v = _keyboard[Key.ShiftLeft] ? 1.5f : 2f;
            if (Math.Sign(e.DeltaPrecise) > 0)
                z *= v;
            else
                z /= v;

            SetZoom(z);
        }

        private void SetZoom(float zoom)
        {
            var zoomBefore = Zoom;
            Zoom = zoom;

            if (Zoom > 1)
                Zoom = (float)Math.Round(Zoom);

            if (Zoom > 5)
                Zoom = 5;
            else if (Zoom < 0.1)
                Zoom = 0.1f;

            var size = new Vector2(Width, Height);
            _grid.Offset -= (size / zoomBefore - size / Zoom) / 2;
            _grid.Offset = new Vector2((int)_grid.Offset.X, (int)_grid.Offset.Y);
        }

        private void CloseHandler(object sender, CancelEventArgs e)
        {
            _dialogEditor.Close();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            switch (mouseButtonEventArgs.Button)
            {
                case MouseButton.Right:
                    _draggingBackground = true;
                    break;
                case MouseButton.Left:
                    var x = _mouse.X;
                    var y = _mouse.Y;

                    var clickedConnection = Graph.PickConnection(x, y, DraggingConnectionPredicate);

                    if (clickedConnection != null)
                    {
                        SelectedNode = null;

                        if (_keyboard[Key.ShiftLeft])
                            Graph.ClearConnectionsFrom(clickedConnection);
                        else
                            _draggingConnection = clickedConnection;
                        return;
                    }

                    var clickedNode = Graph.PickNode(x, y);

                    if (clickedNode != null)
                    {
                        if (SelectedNodes.Count <= 1)
                        {
                            SelectedNode = clickedNode;
                            _dialogEditor.ChangeSelectionTo(SelectedNode);
                        }

                        _draggingNode = true;

                        _draggingNodeOffset.Clear();
                        foreach (var selectedNode in SelectedNodes)
                            _draggingNodeOffset.Add(selectedNode, new Vector2(x - selectedNode.X, y - selectedNode.Y));

                        return;
                    }

                    SelectedNode = null;

                    _selectionRectangle = new Rectangle(_mouse.X, _mouse.Y, 1, 1);
                    break;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            switch (mouseButtonEventArgs.Button)
            {
                case MouseButton.Right:
                    _draggingBackground = false;
                    break;
                case MouseButton.Left:
                    if (_draggingConnection != null)
                    {
                        var picked = Graph.PickConnection(_mouse.X, _mouse.Y, DraggingConnectionPredicate);
                        if (picked != null)
                            _draggingConnection.ConnectTo(picked);
                    }

                    _selectionRectangle = null;
                    _draggingNode = false;
                    _draggingConnection = null;
                    break;
            }
        }

        public Vector2 ScreenToCanvasSpace(Vector2 input)
        {
            var v = input - _grid.Offset * Zoom;
            var temp = Vector3.TransformVector(new Vector3(v), Matrix4.CreateScale(1f / Zoom));
            return temp.Xy;
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
        {
            _mouse = ScreenToCanvasSpace(new Vector2(mouseMoveEventArgs.X, mouseMoveEventArgs.Y));

            if (_draggingBackground)
            {
                _grid.Offset += new Vector2(mouseMoveEventArgs.XDelta, mouseMoveEventArgs.YDelta) / Zoom;
                return;
            }

            if (_selectionRectangle != null)
            {
                _selectionRectangle.Width = _mouse.X - _selectionRectangle.X;
                _selectionRectangle.Height = _mouse.Y - _selectionRectangle.Y;

                SelectAllInSelectionRectangle();
                return;
            }

            if (_draggingNode && SelectedNodes.Count > 0)
            {
                foreach (var node in SelectedNodes)
                {
                    node.X = _mouse.X - _draggingNodeOffset[node].X;
                    node.Y = _mouse.Y - _draggingNodeOffset[node].Y;

                    if (!_keyboard[Key.ShiftLeft])
                    {
                        node.X = (int)((float)Math.Round(node.X / _grid.Pitch) * _grid.Pitch);
                        node.Y = (int)((float)Math.Round(node.Y / _grid.Pitch) * _grid.Pitch);
                    }
                }
            }
        }

        private static void SelectAllInSelectionRectangle()
        {
            SelectedNodes.Clear();
            SelectedNodes.AddRange(Graph.Where(node => _selectionRectangle.Pick(node.X, node.Y)));
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!Focused)
                return;

            if (e.Key == Key.Delete && SelectedNode != null && SelectedNode.Type == NodeType.Option)
            {
                _dialogEditor.ChangeSelectionTo(null);
                Graph.RemoveAll(node => SelectedNodes.Contains(node));
                SelectedNode = null;
            }

            if (e.Control && e.Key == Key.R)
            {
                SetZoom(1);
                if (e.Shift)
                    _grid.Offset = Vector2.Zero;
            }

            if (e.Control && e.Key == Key.C && SelectedNode != null && SelectedNode.Type == NodeType.Option)
            {
                _copiedNode = new Node(SelectedNode)
                {
                    X = 0,
                    Y = 0
                };
            }

            if (e.Control && e.Key == Key.X && SelectedNode != null && SelectedNode.Type == NodeType.Option)
            {
                _copiedNode = new Node(SelectedNode)
                {
                    X = 0,
                    Y = 0
                };

                _dialogEditor.ChangeSelectionTo(null);
                Graph.Remove(SelectedNode);
            }

            if (e.Control && e.Key == Key.V && _copiedNode != null)
            {
                _copiedNode.X = _mouse.X;
                _copiedNode.Y = _mouse.Y;

                Graph.Add(new Node(_copiedNode));
            }
        }

        public void Kill()
        {
            _shouldDie = true;
        }

        private void ResizeHandler(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            // Set up 2D mode
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, -100, 100);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            // Grab the new keyboard state
            _keyboard = Keyboard.GetState();

            if (_shouldDie)
                Exit();

            MarchingAnts.Update();
        }

        private void RenderHandler(object sender, FrameEventArgs e)
        {
            // Start profiling
            _profiler.Start("render");

            // Update sparklines
            if (_profile.ContainsKey("render"))
                _renderTimeSparkline.Enqueue((float)_profile["render"].TotalMilliseconds);

            _fpsSparkline.Enqueue((float)RenderFrequency);

            // Reset the view
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            GL.PushMatrix();
            GL.Scale(Zoom, Zoom, 1);

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

            if (_selectionRectangle != null)
                Fx.D2.DrawWireRectangle(_selectionRectangle.X, _selectionRectangle.Y, _selectionRectangle.Width, _selectionRectangle.Height);

            GL.LineWidth(3);
            if (_draggingConnection != null)
            {
                var end = _mouse;

                var picked = Graph.PickConnection(_mouse.X, _mouse.Y, DraggingConnectionPredicate);
                if (picked != null)
                {
                    var b = picked.GetBounds();
                    end = new Vector2(b.X, b.Y);
                }

                GL.Color3(Color.Gray);
                DrawConnection(_draggingConnection, end);
            }

            for (var i = 0; i < Graph.Count; i++)
                DrawNode(Graph[i], i);

            foreach (var graphNode in Graph)
                DrawConnections(graphNode);

            GL.PopMatrix();

            // Render diagnostic data
            GL.Enable(EnableCap.Texture2D);

            GL.Color4(0, 0, 0, 1f);
            if (_keyboard[Key.D] && Focused)
            {
                // Static diagnostic header

                GL.PushMatrix();
                _font.RenderString($"FPS: {(int)Math.Ceiling(RenderFrequency)}\n" +
                                   $"Zoom: {Zoom}");

                // Sparklines
                GL.Translate(5, (int)(Height - _font.Common.LineHeight * 1.4f * 2), 0);
                _fpsSparkline.Render(Color.Blue, Color.LimeGreen);
                GL.Translate(0, (int)(_font.Common.LineHeight * 1.4f), 0);
                _renderTimeSparkline.Render(Color.Blue, Color.LimeGreen);
                GL.PopMatrix();
            }
            else
            {
                // Info footer
                GL.PushMatrix();
                _font.RenderString($"Ned - Development Build");
                GL.Translate(0, Height - _font.Common.LineHeight, 0);
                _font.RenderString("PRESS 'D' FOR DIAGNOSTICS");
                GL.PopMatrix();
            }
            GL.Disable(EnableCap.Texture2D);

            GL.PopMatrix();

            // Swap the graphics buffer
            SwapBuffers();

            // Stop profiling and get the results
            _profiler.End();
            _profile = _profiler.Reset();
        }

        private void DrawConnections(Node node)
        {
            GL.PushMatrix();
            GL.Translate(0, 0, -10);
            GL.Color3(Color.Gray);

            foreach (var connection in node.Outputs)
            {
                if (connection.ConnectedNode != null)
                    DrawConnection(connection, connection.ConnectedNode);
            }
            GL.PopMatrix();
        }

        private void DrawNode(Node node, int zIndex)
        {
            const int borderRadius = 6;

            GL.PushMatrix();
            GL.Translate(0, 0, zIndex);
            GL.Disable(EnableCap.Texture2D);

            if (SelectedNodes.Contains(node))
            {
                GL.Color3(Color.White);
                RoundRectangle(node.X - 1 / Zoom, node.Y - 1 / Zoom, node.Width + 2 / Zoom, node.Height + 2 / Zoom, borderRadius, borderRadius, borderRadius, borderRadius, PrimitiveType.TriangleFan);
                GL.Translate(0, 0, 0.01);
                GL.Color3(Color.Black);
                MarchingAnts.Use();
                RoundRectangle(node.X - 1 / Zoom, node.Y - 1 / Zoom, node.Width + 2 / Zoom, node.Height + 2 / Zoom, borderRadius, borderRadius, borderRadius, borderRadius, PrimitiveType.TriangleFan);
                MarchingAnts.Release();
            }

            GL.Translate(0, 0, 0.01);

            GL.Color3(Color.DarkSlateGray);
            RoundRectangle(node.X, node.Y + 20, node.Width, node.Height - 20, 0, 0, borderRadius, borderRadius, PrimitiveType.TriangleFan);

            switch (node.Type)
            {
                case NodeType.End:
                case NodeType.Start:
                    GL.Color3(Color.LimeGreen);
                    break;
                case NodeType.Option:
                    switch (node.Actor)
                    {
                        case Actor.None:
                            GL.Color3(Color.Red);
                            break;
                        case Actor.NPC:
                            GL.Color3(Color.Orange);
                            break;
                        case Actor.Player:
                            GL.Color3(Color.DodgerBlue);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            RoundRectangle(node.X, node.Y, node.Width, 20, borderRadius, borderRadius, 0, 0, PrimitiveType.TriangleFan);

            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);

            GL.PushMatrix();
            GL.Translate(node.X + 4, node.Y + 4, 0.01);
            RenderString(node.Name);
            GL.PopMatrix();

            if (node.Input != null)
                DrawConnector(node.Input);

            foreach (var nodeOutput in node.Outputs)
                DrawConnector(nodeOutput);
            GL.PopMatrix();
        }

        private void RenderString(string s)
        {
            if (Zoom >= 0.5)
                _font.RenderString(s);
            else
            {
                GL.PushAttrib(AttribMask.EnableBit);
                GL.Disable(EnableCap.Texture2D);
                var size = _font.MeasureString(s);
                Fx.D2.DrawSolidRectangle(0, 0, size.Width, size.Height);
                GL.PopAttrib();
            }
        }

        private void DrawConnector(Connection connection)
        {
            GL.PushMatrix();

            var pickedForDeletion = Graph.PickConnection(_mouse.X, _mouse.Y, DraggingConnectionPredicate) == connection && _keyboard[Key.ShiftLeft];
            var bound = connection.GetBounds();

            GL.Color3(Color.White);
            GL.Enable(EnableCap.Texture2D);

            GL.PushMatrix();
            switch (connection.Side)
            {
                case NodeSide.Input:
                    GL.Translate(bound.X + 12, bound.Y - 6, 0.01);
                    RenderString(connection.Text);
                    break;
                case NodeSide.Output:
                    var s = MakeStringFit(connection.ParentNode.Width - 20, connection.Text);
                    GL.Translate(bound.X - 12 - _font.MeasureString(s).Width, bound.Y - 6, 0.01);
                    RenderString(s);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);

            GL.Color3(Color.DarkSlateGray);
            Fx.D2.DrawSolidCircle(bound.X, bound.Y, 8);

            GL.Translate(0, 0, 0.01);

            GL.Color3(connection.Side == NodeSide.Input ? Color.DeepSkyBlue : Color.LimeGreen);
            Fx.D2.DrawSolidCircle(bound.X, bound.Y, 6);

            var picked = Graph.PickConnection(_mouse.X, _mouse.Y, DraggingConnectionPredicate);
            if (picked == connection && _draggingConnection != null || _draggingConnection == connection)
            {
                GL.PushMatrix();
                GL.Color3(Color.SlateGray);
                GL.Translate(0, 0, 0.01);
                Fx.D2.DrawSolidCircle(bound.X, bound.Y, 3);
                GL.PopMatrix();
            }
            else if (connection.ConnectedNode != null)
            {
                GL.PushMatrix();
                GL.Color3(Color.DarkSlateGray);
                GL.Translate(0, 0, 0.01);
                Fx.D2.DrawSolidCircle(bound.X, bound.Y, 3);
                GL.PopMatrix();
            }

            if (pickedForDeletion)
            {
                GL.Translate(0, 0, 0.01);

                GL.Color3(Color.Red);
                GL.Enable(EnableCap.PolygonStipple);
                GL.PolygonStipple(StippleDiagonalLines);
                Fx.D2.DrawSolidCircle(bound.X, bound.Y, 6);
                GL.Disable(EnableCap.PolygonStipple);
            }

            GL.PopMatrix();
        }

        private static string MakeStringFit(float maxWidth, string text)
        {
            if (_font.MeasureString(text).Width < maxWidth)
                return text;

            while (text.Length > 0 && _font.MeasureString(text + "...").Width > maxWidth)
                text = text.Substring(0, text.Length - 1);
            return text + "...";
        }

        private void DrawConnection(Connection connection, Connection end)
        {
            var b = end.GetBounds();
            DrawConnection(connection, new Vector2(b.X, b.Y));
        }

        private void DrawConnection(Connection connection, Vector2 end)
        {
            var v = new Vector2(200, 0);
            var bound = connection.GetBounds();
            var pos = new Vector2(bound.X, bound.Y);
            CentripetalCatmullRomTo(connection.Side == NodeSide.Input ? pos + v : pos - v, pos, end,
                connection.Side == NodeSide.Input ? end - v : end + v);
        }

        private void CentripetalCatmullRomTo(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var numSegments = (int)((c - b).Length / 10);

            if (numSegments > 30)
                numSegments = 30;

            var segments = new Vector2[numSegments + 1];

            for (var i = 0; i <= numSegments; i++)
                segments[i] = Fx.D2.EvalCentripetalCatmullRom(a, b, c, d, (float)i / numSegments);

            DrawSmoothLine(4, segments, PrimitiveType.TriangleStrip);
        }

        public static void DrawSmoothLine(float thickness, Vector2[] segments, PrimitiveType mode)
        {
            for (var i = 0; i < segments.Length - 1; i++)
                DrawSmoothLineSegment(thickness, i == 0 ? Vector2.Zero : segments[i - 1], segments[i], segments[i + 1], i == segments.Length - 2 ? Vector2.Zero : segments[i + 2], mode);
        }

        private static void DrawSmoothLineSegment(float thickness, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, PrimitiveType mode)
        {
            if (p1 == p2)
                return;

            var line = (p2 - p1).Normalized();
            var normal = new Vector2(-line.Y, line.X).Normalized();

            var tangent1 = p0 == Vector2.Zero ? line : ((p1 - p0).Normalized() + line).Normalized();
            var tangent2 = p3 == Vector2.Zero ? line : ((p3 - p2).Normalized() + line).Normalized();

            var miter1 = new Vector2(-tangent1.Y, tangent1.X);
            var miter2 = new Vector2(-tangent2.Y, tangent2.X);

            var length1 = thickness / Vector2.Dot(normal, miter1);
            var length2 = thickness / Vector2.Dot(normal, miter2);

            GL.Begin(mode);

            if (p0 == Vector2.Zero)
            {
                GL.Vertex2(p1.X - length1 * miter1.X, p1.Y - length1 * miter1.Y);
                GL.Vertex2(p1.X + length1 * miter1.X, p1.Y + length1 * miter1.Y);
            }

            if (p3 == Vector2.Zero)
            {
                GL.Vertex2(p2.X + length2 * miter2.X, p2.Y + length2 * miter2.Y);
                GL.Vertex2(p2.X - length2 * miter2.X, p2.Y - length2 * miter2.Y);
            }

            GL.Vertex2(p2.X - length2 * miter2.X, p2.Y - length2 * miter2.Y);
            GL.Vertex2(p1.X - length1 * miter1.X, p1.Y - length1 * miter1.Y);

            GL.Vertex2(p2.X + length2 * miter2.X, p2.Y + length2 * miter2.Y);
            GL.Vertex2(p1.X + length1 * miter1.X, p1.Y + length1 * miter1.Y);

            GL.End();
        }

        public static void RoundRectangle(float x, float y, float w, float h, float radiusTopLeft, float radiusTopRight, float radiusBottomLeft, float radiusBottomRight, PrimitiveType mode)
        {
            float step = 45 / Math.Max(radiusTopLeft, Math.Max(radiusTopRight, Math.Max(radiusBottomLeft, radiusBottomRight)));
            if (step > 45)
                step = 45;
            GL.Begin(mode);
            BufferRoundRectangle(x, y, w, h, radiusTopLeft, radiusTopRight, radiusBottomLeft, radiusBottomRight, step);
            GL.End();
        }

        public static void BufferRoundRectangle(float x, float y, float w, float h, float radiusTopLeft, float radiusTopRight, float radiusBottomLeft, float radiusBottomRight, float step)
        {
            for (float i = -90; i <= 0; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusBottomLeft + radiusBottomLeft;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusBottomLeft + h - radiusBottomLeft;
                GL.Vertex2(nx + x, ny + y);
            }
            for (float i = 0; i <= 90; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusBottomRight + w - radiusBottomRight;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusBottomRight + h - radiusBottomRight;
                GL.Vertex2(nx + x, ny + y);
            }
            for (float i = 90; i <= 180; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusTopRight + w - radiusTopRight;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusTopRight + radiusTopRight;
                GL.Vertex2(nx + x, ny + y);
            }
            for (float i = 180; i <= 270; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusTopLeft + radiusTopLeft;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusTopLeft + radiusTopLeft;
                GL.Vertex2(nx + x, ny + y);
            }
        }
    }
}