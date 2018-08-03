using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Ned;
using OpenTK;
using OpenTK.Graphics;
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
        private readonly Dictionary<NodeInfo, Color> _colorMap = new Dictionary<NodeInfo, Color>();
        private readonly Dictionary<Node, Vector2> _draggingNodeOffset = new Dictionary<Node, Vector2>();
        private readonly Profiler _profiler = new Profiler();
        private readonly Color4 _selectionRectangleColor = new Color4(0f, 0f, 1f, 0.1f);
        private readonly float[] _zoomLevels = {0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 1, 2, 3, 4};
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
        private int _zoomIdx = 5;

        public FormDialogueEditor DialogEditor;
        public BitmapFont Font;
        public Vector2 MouseScreenSpace = Vector2.Zero;
        public float Zoom => _zoomLevels[_zoomIdx];

        public Graph Graph => DialogEditor.GetGraph();

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
            KeyPress += OnKeyPress;

            Title = $"{string.Format(Resources.AppTitleWorking, "Untitled")}  (beta-{Resources.Version})";
            //Icon = EmbeddedFiles.logo;
        }

        private void AddOutput(Node node)
        {
            var c = new Connection(node, NodeSide.Output, 0, "Dialog Option");
            node.Outputs.Add(c);
            node.BuildConnections();
            GuiHandler.StartEditing(this, c);
        }

        public Vector2 CanvasToScreenSpace(Vector2 input)
        {
            return (input + _grid.Offset) * Zoom;
        }

        private void CreateContextMenu(Node context, float x, float y)
        {
            const int ctxMenuOffset = 1;
            _contextMenu = new ContextMenu(MouseScreenSpace.X + ctxMenuOffset, MouseScreenSpace.Y + ctxMenuOffset, 250);

            if (context == null)
            {
                _contextMenu.Add(new ContextMenuItem(this, "NPC Dialogue", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.NpcDialogue, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Player Dialogue", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.PlayerDialogue, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Has Flag", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.WaitForFlag, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Set Flag", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.SetFlag, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Clear Flag", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.ClearFlag, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Is Quest Active", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.HasQuest, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Start Quest", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.StartQuest, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Complete Quest", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.CompleteQuest, v.X, v.Y);
                    Graph.Add(n);
                    _selectionHandler.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Trigger Event", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.TriggerEvent, v.X, v.Y);
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
                if (!context.NodeInfo.CanEditNode) return;

                if (_selectionHandler.OneOrNoneSelected)
                    _selectionHandler.Select(context);

                _contextMenu.Add(new ContextMenuItem(this, "DEL", "Delete Node", item => _selectionHandler.Delete()));
                _contextMenu.Add(new ContextMenuItem(this, "CTRL+X", "Cut", item => _selectionHandler.Cut()));
                _contextMenu.Add(new ContextMenuItem(this, "CTRL+C", "Copy", item => _selectionHandler.Copy()));

                if (context.NodeInfo.CanEditConnectors)
                {
                    _contextMenu.Add(new ContextMenuItem(this, "CTRL+Plus", "Add Connection",
                        item => AddOutput(context)));

                    var subContext = PickConnectionWithText(x, y);
                    if (subContext != null)
                        _contextMenu.Add(new ContextMenuItem(this, "CTRL+Minus", $"Delete \"{subContext.Text}\"",
                            item => context.RemoveOutput(subContext)));
                }
            }

            _contextMenu.RecalculateWidth();
        }

        private int GetNodeWidth(Node node)
        {
            var width = 120;
            const int textPadding = 40;

            width = (int) Math.Max(Font.MeasureString(node.Name).Width + textPadding, width);

            if (node.Input != null)
                width = (int) Math.Max(Font.MeasureString(node.Input.Text).Width + textPadding, width);

            foreach (var connection in node.Outputs)
                width = (int) Math.Max(Font.MeasureString(connection.Text).Width + textPadding, width);

            width = (int) (Math.Ceiling(width / (float) _grid.Pitch) * _grid.Pitch);

            return width;
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
            DialogEditor.Hide();

            CreateContextMenu(null, 0, 0);

            _draggingConnectionPredicate = connection =>
                _draggingConnection == null || _draggingConnection.ParentNode != connection.ParentNode &&
                _draggingConnection.Side != connection.Side;

            _colorMap.Add(NodeInfo.None, Color.Black);

            _colorMap.Add(NodeInfo.Start, Color.LimeGreen);
            _colorMap.Add(NodeInfo.End, Color.IndianRed);

            _colorMap.Add(NodeInfo.NpcDialogue, Color.MediumPurple);
            _colorMap.Add(NodeInfo.PlayerDialogue, Color.LightSkyBlue);

            _colorMap.Add(NodeInfo.WaitForFlag, Color.Orange);
            _colorMap.Add(NodeInfo.SetFlag, Color.MediumSeaGreen);
            _colorMap.Add(NodeInfo.ClearFlag, Color.MediumVioletRed);

            _colorMap.Add(NodeInfo.HasQuest, Color.DarkOrange);
            _colorMap.Add(NodeInfo.StartQuest, Color.CadetBlue);
            _colorMap.Add(NodeInfo.CompleteQuest, Color.DarkOrchid);

            _colorMap.Add(NodeInfo.TriggerEvent, Color.DarkKhaki);

            KeybindHandler.Register(new KeyCombo("Delete Selection", Key.Delete), () => _selectionHandler.Delete());
            KeybindHandler.Register(new KeyCombo("Reset Zoom", Key.R, KeyModifiers.Control), () => SetZoom(1));
            KeybindHandler.Register(
                new KeyCombo("Reset Zoom and Pan", Key.R, KeyModifiers.Control | KeyModifiers.Shift), () =>
                {
                    SetZoom(1);
                    _grid.Offset = Vector2.Zero;
                });
            KeybindHandler.Register(new KeyCombo("Add Output", Key.Plus, KeyModifiers.Control), () =>
            {
                if (_selectionHandler.SingleSelectedNode == null ||
                    _selectionHandler.SingleSelectedNode.NodeInfo.CanEditConnectors) return;

                AddOutput(_selectionHandler.SingleSelectedNode);
            });
            KeybindHandler.Register(new KeyCombo("Copy", Key.C, KeyModifiers.Control), () => _selectionHandler.Copy());
            KeybindHandler.Register(new KeyCombo("Cut", Key.X, KeyModifiers.Control), () => _selectionHandler.Cut());
            KeybindHandler.Register(new KeyCombo("Paste", Key.V, KeyModifiers.Control),
                () => _selectionHandler.Paste(_mouseCanvasSpace.X, _mouseCanvasSpace.Y, !_keyboard[Key.ShiftLeft],
                    _grid.Pitch));
            KeybindHandler.Register(new KeyCombo("Open Project", Key.O, KeyModifiers.Control),
                () => DialogEditor.AskOpenFile());
            KeybindHandler.Register(new KeyCombo("Save Project", Key.S, KeyModifiers.Control),
                () => DialogEditor.AskSaveFile());
            KeybindHandler.Register(new KeyCombo("Save Project As", Key.S, KeyModifiers.Control | KeyModifiers.Shift),
                () => DialogEditor.AskSaveFileAs());
            KeybindHandler.Register(new KeyCombo("Export Graph", Key.E, KeyModifiers.Control),
                () => DialogEditor.AskExportFile());

            var keybinds = KeybindHandler.GetKeybinds();

            Lumberjack.Info("Keybinds:");
            foreach (var keyCombo in keybinds)
                Lumberjack.WriteLine(keyCombo.ToString(), ConsoleColor.Blue, OutputLevel.Info, "KEYS");

            Lumberjack.Info("Window Loaded.");
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

            if (GuiHandler.TextBox != null)
            {
                if (e.Control && e.Key == Key.Minus)
                {
                    if (GuiHandler.TextBox == null) return;

                    var connection = GuiHandler.EditingConnection;
                    connection.ParentNode.RemoveOutput(connection);
                    GuiHandler.Destroy(false);
                    connection.ParentNode.BuildConnections();
                    return;
                }

                GuiHandler.TextBox.OnKey(e);
                return;
            }

            KeybindHandler.Consume(new KeyCombo(e));
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (GuiHandler.TextBox == null) return;
            GuiHandler.TextBox.OnCharacter(e.KeyChar);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            switch (mouseButtonEventArgs.Button)
            {
                case MouseButton.Right:
                    CreateContextMenu(Graph.PickNode(_mouseCanvasSpace.X, _mouseCanvasSpace.Y), _mouseCanvasSpace.X,
                        _mouseCanvasSpace.Y);
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

                    var clickedTextBox = GuiHandler.PickAndCreate(this, Graph, x, y);
                    if (clickedTextBox != null)
                    {
                        clickedTextBox.OnMouseDown(mouseButtonEventArgs);
                        return;
                    }
                    else
                    {
                        GuiHandler.Destroy();
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
                        new Rectangle(_mouseCanvasSpace.X, _mouseCanvasSpace.Y, 0, 0);
                    break;
            }
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
        {
            MouseScreenSpace = new Vector2(mouseMoveEventArgs.X, mouseMoveEventArgs.Y);
            _mouseCanvasSpace = ScreenToCanvasSpace(MouseScreenSpace);

            if (GuiHandler.TextBox != null)
            {
                GuiHandler.TextBox.OnMouseMove(mouseMoveEventArgs);
                return;
            }

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
                    if (GuiHandler.TextBox != null)
                    {
                        GuiHandler.TextBox.OnMouseUp(mouseButtonEventArgs);
                        return;
                    }

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
            var z = _zoomIdx;

            if (Math.Sign(e.DeltaPrecise) > 0)
                z++;
            else
                z--;

            if (z < 0)
                z = 0;
            else if (z >= _zoomLevels.Length)
                z = _zoomLevels.Length - 1;

            SetZoom(z);
        }

        public Connection PickConnectionWithText(float x, float y)
        {
            var pickedNode = Graph.PickNode(x, y);

            if (pickedNode == null) return null;

            foreach (var output in pickedNode.Outputs)
            {
                var s = Font.MeasureString(output.Text);
                var bound = output.GetBounds();
                var r = bound.Radius;
                var twor = 2 * r;

                var outputRect = new Rectangle(bound.X - twor - s.Width, bound.Y - r, s.Width, s.Height);
                if (!outputRect.Pick(x, y)) continue;

                return output;
            }

            return null;
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
            GL.Scale(Zoom, Zoom, 2);

            GL.Disable(EnableCap.Texture2D);

            GL.PushMatrix();
            GL.Translate(0, 0, -10);

            GL.Color3(Color.Gray);
            GL.PointSize(1);

            _grid.Draw();

            GL.PopMatrix();

            GL.Translate(_grid.Offset.X, _grid.Offset.Y, 0);
            GL.LineWidth(2);

            GL.Color3(Color.Black);
            Fx.D2.DrawLine(-_grid.Pitch, 0, _grid.Pitch, 0);
            Fx.D2.DrawLine(0, -_grid.Pitch, 0, _grid.Pitch);

            const int cxnLineWidth = 3;
            GL.LineWidth(cxnLineWidth);
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
                Fx.D2.DrawSolidCircle(end.X, end.Y, cxnLineWidth);
            }

            foreach (var node in Graph)
            {
                if (!ScreenContains(node))
                    continue;
                RenderNode(node);
                RenderConnections(node);
            }

            GL.Disable(EnableCap.DepthTest);
            GL.Color4(_selectionRectangleColor);
            if (_selectionHandler.SelectionRectangle != null)
                Fx.D2.DrawSolidRectangle(_selectionHandler.SelectionRectangle.X, _selectionHandler.SelectionRectangle.Y,
                    _selectionHandler.SelectionRectangle.Width, _selectionHandler.SelectionRectangle.Height);

            GL.PopMatrix();

            GL.Enable(EnableCap.Texture2D);

            _contextMenu.Render();

            GL.Color4(0, 0, 0, 1f);
            if (_keyboard[Key.D] && Focused && GuiHandler.TextBox == null)
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

            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.PopMatrix();

            // Swap the graphics buffer
            SwapBuffers();

            // Stop profiling and get the results
            _profiler.End();
            _profile = _profiler.Reset();
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

            var pickedForDeletion = _pickedConnection == connection && _keyboard[Key.ShiftLeft];
            var bound = connection.GetBounds();
            var r = bound.Radius;
            var twor = 2 * r;
            var halfr = r / 2;
            const int cxnBorderWidth = 2;

            GL.Color3(Color.White);
            GL.Enable(EnableCap.Texture2D);

            GL.PushMatrix();
            if (connection != GuiHandler.EditingConnection)
            {
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
            }
            else
            {
                GL.Translate(0, 0, 0.01);
                GuiHandler.TextBox.RenderBackground();
                GL.Translate(0, 0, 0.01);
                GuiHandler.TextBox.RenderForeground();
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
            const int headerHeight = 18;
            const int panelInset = 2;
            const float halfPanelInset = panelInset / 2f;

            var oneCanvasPixel = 1 / Zoom;

            GL.PushMatrix();
            GL.Translate(0, 0, node.Layer);
            GL.Disable(EnableCap.Texture2D);

            if (_selectionHandler.SelectedNodes.Contains(node))
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
            var headerOffset = (headerHeight + panelInset) / 2f - Font.MeasureString(node.Name).Height / 2;
            GL.Translate(node.X + 2 * panelInset, node.Y + headerOffset, 0.01);
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

            if (Zoom >= 0.5)
            {
                Font.RenderString(s);
            }
            else
            {
                GL.PushAttrib(AttribMask.EnableBit);
                GL.Disable(EnableCap.Texture2D);
                var size = Font.MeasureString(s);
                var halfHeight = Font.Common.LineHeight / 2;
                Fx.D2.DrawSolidRoundRectangle(0, 0, size.Width, Font.Common.LineHeight, halfHeight, halfHeight,
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

            var screenTopLeft = ScreenToCanvasSpace(Vector2.Zero);
            var screenBotRight = ScreenToCanvasSpace(new Vector2(Width, Height));
            var screen = new Rectangle(screenTopLeft.X, screenTopLeft.Y,
                screenBotRight.X - screenTopLeft.X, screenBotRight.Y - screenTopLeft.Y);

            return nodeRect.Intersects(screen) || nodeRectOthers.Any(node1 => node1.Intersects(screen));
        }

        public Vector2 ScreenToCanvasSpace(Vector2 input)
        {
            return input / Zoom - _grid.Offset;
        }

        private void SetZoom(int zoomIdx)
        {
            var zoomBefore = Zoom;
            _zoomIdx = zoomIdx;

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