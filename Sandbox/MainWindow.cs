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

namespace Sandbox
{
    public class MainWindow : GameWindow
    {
        private static FormDialogEditor _dialogEditor;
        private static Graph Graph => _dialogEditor.GetGraph();

        private static Node _selectedNode;
        private static Node _draggingNode;
        private static Vector2 _draggingNodeOffset;

        private static Connection _draggingConnection;
        private static Func<Connection, bool> _draggingConnectionPredicate = (connection) =>
        {
            return _draggingConnection == null || (_draggingConnection.ParentNode != connection.ParentNode && _draggingConnection.Side != connection.Side);
        };

        /*
         * Window-related
         */
        private bool _shouldDie;
        private Sparkline _fpsSparkline;
        private Sparkline _renderTimeSparkline;
        private readonly Profiler _profiler = new Profiler();
        private static KeyboardState _keyboard;
        private static Vector2 _mouse = Vector2.Zero;
        private static BitmapFont _font;
        private static readonly byte[] StippleDiagonalLines = { 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88, 0x22, 0x22, 0x22, 0x22, 0x88, 0x88, 0x88, 0x88 };
        private Dictionary<string, TimeSpan> _profile = new Dictionary<string, TimeSpan>();

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
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);

            // Set up blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            GL.Enable(EnableCap.PolygonSmooth);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

            // Set background color
            GL.ClearColor(Color.White);

            // Load fonts
            _font = BitmapFont.LoadBinaryFont("dina", FontBank.FontDina, FontBank.BmDina);

            // Load sparklines
            _fpsSparkline = new Sparkline(_font, $"0-{(int)TargetRenderFrequency}fps", 50,
                (float)TargetRenderFrequency, Sparkline.SparklineStyle.Area);
            _renderTimeSparkline = new Sparkline(_font, "0-50ms", 50, 50, Sparkline.SparklineStyle.Area);

            // Init keyboard to ensure first frame won't NPE
            _keyboard = Keyboard.GetState();

            _dialogEditor = new FormDialogEditor(this);
            _dialogEditor.Show();

            Lumberjack.Info("Window Loaded.");
        }

        private void WindowVisualize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        private void CloseHandler(object sender, CancelEventArgs e)
        {
            _dialogEditor.Close();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _selectedNode = null;
            _dialogEditor.ChangeSelectionTo(_selectedNode);

            var clickedConnection = Graph.PickConnection(mouseButtonEventArgs.X, mouseButtonEventArgs.Y, _draggingConnectionPredicate);

            if (clickedConnection != null)
            {
                if (_keyboard[Key.ShiftLeft])
                    Graph.ClearConnectionsFrom(clickedConnection);
                else
                    _draggingConnection = clickedConnection;
                return;
            }

            var clickedNode = Graph.PickNode(mouseButtonEventArgs.X, mouseButtonEventArgs.Y);

            if (clickedNode != null)
            {
                _selectedNode = clickedNode;
                _dialogEditor.ChangeSelectionTo(_selectedNode);

                _draggingNode = clickedNode;
                _draggingNodeOffset = new Vector2(mouseButtonEventArgs.X - clickedNode.X, mouseButtonEventArgs.Y - clickedNode.Y);
                return;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_draggingConnection != null)
            {
                var picked = Graph.PickConnection(_mouse.X, _mouse.Y, _draggingConnectionPredicate);
                if (picked != null)
                    _draggingConnection.ConnectTo(picked);
            }

            _draggingNode = null;
            _draggingConnection = null;
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
        {
            _mouse = new Vector2(mouseMoveEventArgs.X, mouseMoveEventArgs.Y);

            if (_draggingNode != null)
            {
                _draggingNode.X = mouseMoveEventArgs.X - _draggingNodeOffset.X;
                _draggingNode.Y = mouseMoveEventArgs.Y - _draggingNodeOffset.Y;
            }
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (Focused && e.Key == Key.Delete && _selectedNode != null)
            {
                Graph.ClearConnectionsFrom(_selectedNode.Input);
                foreach (var connection in _selectedNode.Outputs)
                    Graph.ClearConnectionsFrom(connection);

                _dialogEditor.ChangeSelectionTo(null);
                Graph.Remove(_selectedNode);
                _selectedNode = null;
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

            GL.Disable(EnableCap.Texture2D);
            GL.LineWidth(3);
            if (_draggingConnection != null)
            {
                var end = _mouse;

                var picked = Graph.PickConnection(_mouse.X, _mouse.Y, _draggingConnectionPredicate);
                if (picked != null)
                {
                    var bounds = picked.GetBounds();
                    end = new Vector2(bounds.X, bounds.Y);

                    GL.Color3(Color.DarkSlateGray);
                    GL.Enable(EnableCap.LineStipple);
                    GL.LineStipple(3, 0xAAAA);
                    Fx.D2.DrawWireCircle(end.X, end.Y, 12);
                    GL.Disable(EnableCap.LineStipple);
                }


                GL.Color3(Color.Gray);
                DrawConnection(_draggingConnection, end);
            }

            foreach (var graphNode in Graph)
                DrawNode(graphNode);

            // Render diagnostic data
            GL.Enable(EnableCap.Texture2D);

            GL.Color4(0, 0, 0, 1f);
            if (_keyboard[Key.D] && Focused)
            {
                // Static diagnostic header

                GL.PushMatrix();
                _font.RenderString($"FPS: {(int)Math.Ceiling(RenderFrequency)}");

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

        private static void DrawNode(Node node)
        {
            GL.PushMatrix();
            GL.Disable(EnableCap.Texture2D);

            if (_selectedNode == node)
            {
                GL.Color3(Color.White);
                Fx.D2.DrawSolidRectangle(node.X - 1, node.Y - 1, node.Width + 2, node.Height + 2);
                GL.Color3(Color.Black);
                MarchingAnts.Use();
                Fx.D2.DrawSolidRectangle(node.X - 1, node.Y - 1, node.Width + 2, node.Height + 2);
                MarchingAnts.Release();
            }

            GL.Color3(Color.DarkSlateGray);
            Fx.D2.DrawSolidRectangle(node.X, node.Y + 18, node.Width, node.Height - 18);

            GL.Color3(Color.Orange);
            Fx.D2.DrawSolidRectangle(node.X, node.Y, node.Width, 18);

            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);

            GL.PushMatrix();
            GL.Translate(node.X + 4, node.Y + 4, 0);
            _font.RenderString(node.Name);
            GL.PopMatrix();

            if (node.Input != null)
                DrawConnector(node.Input);

            foreach (var nodeOutput in node.Outputs)
                DrawConnector(nodeOutput);
            GL.PopMatrix();
        }

        private static void DrawConnector(Connection connection)
        {
            GL.PushMatrix();

            var pickedForDeletion = Graph.PickConnection(_mouse.X, _mouse.Y, _draggingConnectionPredicate) == connection && _keyboard[Key.ShiftLeft];
            var bound = connection.GetBounds();

            GL.Color3(Color.White);
            GL.Enable(EnableCap.Texture2D);

            GL.PushMatrix();
            switch (connection.Side)
            {
                case NodeSide.Input:
                    GL.Translate(bound.X + 12, bound.Y - 6, 0);
                    _font.RenderString(connection.Text);
                    break;
                case NodeSide.Output:
                    var s = MakeStringFit(connection.ParentNode.Width - 20, connection.Text);
                    GL.Translate(bound.X - 12 - _font.MeasureString(s).Width, bound.Y - 6, 0);
                    _font.RenderString(s);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            GL.PopMatrix();

            GL.Disable(EnableCap.Texture2D);
            GL.Color3(Color.Gray);

            if (connection.Side == NodeSide.Output && connection.ConnectedNode != null)
                DrawConnection(connection, connection.ConnectedNode);

            GL.Color3(Color.DarkSlateGray);
            Fx.D2.DrawSolidCircle(bound.X, bound.Y, 8);

            GL.Color3(connection.Side == NodeSide.Input ? Color.DeepSkyBlue : Color.LimeGreen);
            Fx.D2.DrawSolidCircle(bound.X, bound.Y, 6);

            if (pickedForDeletion)
            {
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

        private static void DrawConnection(Connection connection, Connection end)
        {
            var b = end.GetBounds();
            DrawConnection(connection, new Vector2(b.X, b.Y));
        }

        private static void DrawConnection(Connection connection, Vector2 end)
        {
            GL.LineWidth(4);
            var v = new Vector2(200, 0);
            var bound = connection.GetBounds();
            var pos = new Vector2(bound.X, bound.Y);
            Fx.D2.CentripetalCatmullRomTo(connection.Side == NodeSide.Input ? pos + v : pos - v, pos, end,
                connection.Side == NodeSide.Input ? end - v : end + v, 20);
        }
    }
}