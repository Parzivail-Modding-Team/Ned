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

namespace Sandbox
{
    public class MainWindow : GameWindow
    {
        private readonly Graph _graph;

        private Node _draggingNode;
        private Vector2 _draggingNodeOffset;

        /*
         * Window-related
         */
        private bool _shouldDie;
        private Sparkline _fpsSparkline;
        private Sparkline _renderTimeSparkline;
        private readonly Profiler _profiler = new Profiler();
        private static KeyboardState _keyboard;
        private static BitmapFont _font;
        private Dictionary<string, TimeSpan> _profile = new Dictionary<string, TimeSpan>();

        public MainWindow(Graph graph) : base(800, 600)
        {
            _graph = graph;
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

            Lumberjack.Info("Window Loaded.");
        }

        private void WindowVisualize_MouseWheel(object sender, MouseWheelEventArgs e)
        {
        }

        private void CloseHandler(object sender, CancelEventArgs e)
        {
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var clickedNode = PickNode(mouseButtonEventArgs.X, mouseButtonEventArgs.Y);

            if (clickedNode == null)
                return;

            _draggingNode = clickedNode;
            _draggingNodeOffset = new Vector2(mouseButtonEventArgs.X - clickedNode.X, mouseButtonEventArgs.Y - clickedNode.Y);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _draggingNode = null;
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
        {
            if (_draggingNode != null)
            {
                _draggingNode.X = mouseMoveEventArgs.X - _draggingNodeOffset.X;
                _draggingNode.Y = mouseMoveEventArgs.Y - _draggingNodeOffset.Y;
            }
        }

        private Node PickNode(int x, int y)
        {
            return _graph.Nodes.FirstOrDefault(node => new Rectangle((int)node.X, (int)node.Y, (int)node.Width, (int)node.Height).Contains(x, y));
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
            GL.Ortho(0, Width, Height, 0, 100, -1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private void UpdateHandler(object sender, FrameEventArgs e)
        {
            // Grab the new keyboard state
            _keyboard = Keyboard.GetState();

            if (_shouldDie)
                Exit();
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

            foreach (var graphNode in _graph.Nodes)
            {
                DrawNode(graphNode);
            }

            GL.Color4(0, 0, 0, 0.2f);
            // Render diagnostic data
            GL.Enable(EnableCap.Texture2D);

            GL.Color4(0, 0, 0, 1f);
            if (_keyboard[Key.D])
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

        private static void DrawNode(Node graphNode)
        {
            GL.PushMatrix();
            GL.Color3(Color.DarkSlateGray);
            GL.Translate(graphNode.X, graphNode.Y, 0);
            GL.Disable(EnableCap.Texture2D);
            Fx.D2.DrawSolidRectangle(0, 18, graphNode.Width, graphNode.Height - 18);

            GL.Color3(Color.Orange);
            Fx.D2.DrawSolidRectangle(0, 0, graphNode.Width, 18);

            GL.Enable(EnableCap.Texture2D);
            GL.Color3(Color.White);

            GL.PushMatrix();
            GL.Translate(4, 4, 0);
            _font.RenderString(graphNode.Name);
            GL.PopMatrix();

            DrawConnection(graphNode.Input);
            foreach (var nodeOutput in graphNode.Outputs)
                DrawConnection(nodeOutput);
            GL.PopMatrix();
        }

        private static void DrawConnection(Connection connection)
        {
            GL.PushMatrix();
            GL.Disable(EnableCap.Texture2D);

            switch (connection.Side) {
                case NodeSide.Input:
                    GL.Color3(Color.DarkSlateGray);
                    Fx.D2.DrawSolidCircle(-6, (connection.ConnectionIndex + 1) * 24 + (int)(12 * 1.5f), 8);
                    GL.Color3(Color.DeepSkyBlue);
                    Fx.D2.DrawSolidCircle(-6, (connection.ConnectionIndex + 1) * 24 + (int)(12 * 1.5f), 6);
                    break;
                case NodeSide.Output:
                    GL.Color3(Color.DarkSlateGray);
                    Fx.D2.DrawSolidCircle(connection.ParentNode.Width + 6, (connection.ConnectionIndex + 1) * 24 + 18, 8);
                    GL.Color3(Color.LimeGreen);
                    Fx.D2.DrawSolidCircle(connection.ParentNode.Width + 6, (connection.ConnectionIndex + 1) * 24 + 18, 6);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GL.Color3(Color.White);
            GL.Enable(EnableCap.Texture2D);

            switch (connection.Side)
            {
                case NodeSide.Input:
                    GL.Translate(6, (connection.ConnectionIndex + 1) * 24 + 12, 0);
                    _font.RenderString(connection.Text);
                    break;
                case NodeSide.Output:
                    GL.Translate(connection.ParentNode.Width - 6 - _font.MeasureString(connection.Text).Width, (connection.ConnectionIndex + 1) * 24 + 12, 0);
                    _font.RenderString(connection.Text);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GL.PopMatrix();
        }
    }
}