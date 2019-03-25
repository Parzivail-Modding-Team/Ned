using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NanoVGDotNet;
using Ned;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    public class MainWindow : GameWindow
    {
        public static NVGcontext Nvg = new NVGcontext();

        private readonly Dictionary<Node, Vector2> _draggingNodeOffset = new Dictionary<Node, Vector2>();
        private readonly Color4 _selectionRectangleColor = new Color4(0, 0, 1, 0.1f);
        private readonly float[] _zoomLevels = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 1, 2, 3, 4 };
        private ContextMenu _contextMenu;
        private bool _draggingBackground;
        private Func<Connection, bool> _draggingConnectionPredicate;
        private GridRenderer _grid;
        private Vector2 _mouseCanvasSpace = Vector2.Zero;
        private NodeRenderer _nodeRenderer;
        private bool _shouldDie;
        private int _zoomIdx = 5;

        public FormDialogueEditor DialogEditor;
        public KeyboardState Keyboard;
        public Vector2 MouseScreenSpace = Vector2.Zero;
        public SelectionHandler Selection;
        public float Zoom => _zoomLevels[_zoomIdx];

        public float FontLineHeight = 16;

        public Graph Graph => DialogEditor.GetGraph();

        public MainWindow() : base(800, 600, new GraphicsMode(new ColorFormat(32), 24, 8, 0))
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
            TextBoxHandler.StartEditing(this, c);
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
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Player Dialogue", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.PlayerDialogue, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Has Flag", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.WaitForFlag, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Set Flag", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.SetFlag, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Clear Flag", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.ClearFlag, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Is Quest Active", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.HasQuest, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Start Quest", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.StartQuest, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Complete Quest", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.CompleteQuest, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                _contextMenu.Add(new ContextMenuItem(this, "Trigger Event", item =>
                {
                    var v = ScreenToCanvasSpace(new Vector2(_contextMenu.X, _contextMenu.Y));
                    var n = new Node(NodeInfo.TriggerEvent, v.X, v.Y);
                    Graph.Add(n);
                    Selection.Select(n);
                }));

                if (!Selection.IsClipboardEmpty)
                    _contextMenu.Add(new ContextMenuItem(this, "CTRL+V", "Paste",
                        item => Selection.Paste(_contextMenu.X, _contextMenu.Y, !Keyboard[Key.ShiftLeft],
                            _grid.Pitch)));
            }
            else
            {
                if (!context.NodeInfo.CanEditNode) return;

                if (Selection.OneOrNoneSelected)
                    Selection.Select(context);

                _contextMenu.Add(new ContextMenuItem(this, "DEL", "Delete Node", item => Selection.Delete()));
                _contextMenu.Add(new ContextMenuItem(this, "CTRL+X", "Cut", item => Selection.Cut()));
                _contextMenu.Add(new ContextMenuItem(this, "CTRL+C", "Copy", item => Selection.Copy()));

                if (context.NodeInfo.CanEditConnectors)
                {
                    if (context.NodeInfo.Type == NodeInfo.PlayerDialogue.Type)
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

        private void HandleClose(object sender, CancelEventArgs e)
        {
            if (Graph.Count != 2)
                switch (MessageBox.Show("Save changes before closing?", "Save changes?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                    case DialogResult.Yes:
                        DialogEditor.AskSaveFile();
                        break;
                    case DialogResult.No:
                        break;
                }
            DialogEditor.Close();
        }

        private void HandleLoad(object sender, EventArgs e)
        {
            SetupOpenGl();

            // Init keyboard to ensure first frame won't NPE
            Keyboard = OpenTK.Input.Keyboard.GetState();

            _grid = new GridRenderer(this);
            _nodeRenderer = new NodeRenderer(this, _grid);

            Node.WidthCalculator = _nodeRenderer.GetNodeWidth;

            Selection = new SelectionHandler(this);

            DialogEditor = new FormDialogueEditor(this);
            DialogEditor.Show();
            DialogEditor.Hide();

            CreateContextMenu(null, 0, 0);

            _draggingConnectionPredicate = connection =>
                Selection.DraggingConnection == null ||
                Selection.DraggingConnection.ParentNode != connection.ParentNode &&
                Selection.DraggingConnection.Side != connection.Side;

            RegisterKeybinds();

            var keybinds = KeybindHandler.GetKeybinds();
            Lumberjack.Info("Keybinds:");
            foreach (var keyCombo in keybinds)
                Lumberjack.Info($"- {keyCombo}");

            Lumberjack.Info("Window Loaded.");
        }

        private void HandleResize(object sender, EventArgs e)
        {
            GL.Viewport(ClientRectangle);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
        }

        public void Kill()
        {
            _shouldDie = true;
        }

        private void OnKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (!Focused)
                return;

            if (TextBoxHandler.TextBox != null)
            {
                if (e.Control && e.Key == Key.Minus)
                {
                    if (TextBoxHandler.TextBox == null) return;

                    var connection = TextBoxHandler.EditingConnection;
                    connection.ParentNode.RemoveOutput(connection);
                    TextBoxHandler.Destroy(false);
                    connection.ParentNode.BuildConnections();
                    return;
                }

                TextBoxHandler.TextBox.OnKey(e);
                return;
            }

            KeybindHandler.Consume(new KeyCombo(e));
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (TextBoxHandler.TextBox == null) return;
            TextBoxHandler.TextBox.OnCharacter(e.KeyChar);
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

                    var clickedTextBox = TextBoxHandler.PickAndCreate(this, Graph, x, y);
                    if (clickedTextBox != null)
                    {
                        clickedTextBox.OnMouseDown(mouseButtonEventArgs);
                        return;
                    }
                    else
                    {
                        TextBoxHandler.Destroy();
                    }

                    var clickedConnection = Graph.PickConnection(x, y, _draggingConnectionPredicate);
                    if (clickedConnection != null)
                    {
                        Selection.Clear();

                        if (Keyboard[Key.ShiftLeft])
                            Graph.ClearConnectionsFrom(clickedConnection);
                        else
                            Selection.DraggingConnection = clickedConnection;
                        return;
                    }

                    var clickedNode = Graph.PickNode(x, y);
                    if (clickedNode != null)
                    {
                        if (!Selection.SelectedNodes.Contains(clickedNode))
                            Selection.Select(clickedNode);

                        Selection.IsDraggingNode = true;

                        _draggingNodeOffset.Clear();
                        foreach (var selectedNode in Selection.SelectedNodes)
                            _draggingNodeOffset.Add(selectedNode, new Vector2(x - selectedNode.X, y - selectedNode.Y));

                        return;
                    }

                    Selection.SelectionRectangle =
                        new Rectangle(_mouseCanvasSpace.X, _mouseCanvasSpace.Y, 0, 0);
                    break;
            }
        }

        private void OnMouseMove(object sender, MouseMoveEventArgs mouseMoveEventArgs)
        {
            MouseScreenSpace = new Vector2(mouseMoveEventArgs.X, mouseMoveEventArgs.Y);
            _mouseCanvasSpace = ScreenToCanvasSpace(MouseScreenSpace);

            if (TextBoxHandler.TextBox != null)
            {
                TextBoxHandler.TextBox.OnMouseMove(mouseMoveEventArgs);
                return;
            }

            if (_draggingBackground)
            {
                _grid.Offset += new Vector2(mouseMoveEventArgs.XDelta, mouseMoveEventArgs.YDelta) / Zoom;
                return;
            }

            if (Selection.SelectionRectangle != null)
            {
                Selection.SelectionRectangle = new Rectangle(Selection.SelectionRectangle.X,
                    Selection.SelectionRectangle.Y, _mouseCanvasSpace.X - Selection.SelectionRectangle.X,
                    _mouseCanvasSpace.Y - Selection.SelectionRectangle.Y);
                return;
            }

            if (Selection.IsDraggingNode && Selection.SelectedNodes.Count > 0)
                foreach (var node in Selection.SelectedNodes)
                {
                    node.X = _mouseCanvasSpace.X - _draggingNodeOffset[node].X;
                    node.Y = _mouseCanvasSpace.Y - _draggingNodeOffset[node].Y;

                    if (!Keyboard[Key.ShiftLeft])
                    {
                        node.X = (int)(Math.Floor(node.X / _grid.Pitch) * _grid.Pitch);
                        node.Y = (int)(Math.Floor(node.Y / _grid.Pitch) * _grid.Pitch);
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
                    if (TextBoxHandler.TextBox != null)
                    {
                        TextBoxHandler.TextBox.OnMouseUp(mouseButtonEventArgs);
                        return;
                    }

                    if (Selection.DraggingConnection != null)
                    {
                        var picked = Graph.PickConnection(_mouseCanvasSpace.X, _mouseCanvasSpace.Y,
                            _draggingConnectionPredicate);
                        if (picked != null)
                            Selection.DraggingConnection.ConnectTo(picked);
                    }

                    Selection.SelectionRectangle = null;
                    Selection.IsDraggingNode = false;
                    Selection.DraggingConnection = null;
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
                var s = NvgHelper.MeasureString(output.Text);
                var bound = output.GetBounds();
                var r = bound.Radius;
                var twor = 2 * r;

                var outputRect = new Rectangle(bound.X - twor - s.Width, bound.Y - r, s.Width, s.Height);
                if (!outputRect.Pick(x, y)) continue;

                return output;
            }

            return null;
        }

        private void RegisterKeybinds()
        {
            KeybindHandler.Register(new KeyCombo("Delete Selection", Key.Delete), () => Selection.Delete());
            KeybindHandler.Register(new KeyCombo("Reset Zoom", Key.R, KeyModifiers.Control), () => SetZoom(5));
            KeybindHandler.Register(
                new KeyCombo("Reset Zoom and Pan", Key.R, KeyModifiers.Control | KeyModifiers.Shift), () =>
                {
                    SetZoom(5);
                    _grid.Offset = Vector2.Zero;
                });
            KeybindHandler.Register(new KeyCombo("Add Output", Key.Plus, KeyModifiers.Control), () =>
            {
                if (Selection.SingleSelectedNode == null ||
                    !Selection.SingleSelectedNode.NodeInfo.CanEditConnectors) return;

                AddOutput(Selection.SingleSelectedNode);
            });
            KeybindHandler.Register(new KeyCombo("Copy", Key.C, KeyModifiers.Control), () => Selection.Copy());
            KeybindHandler.Register(new KeyCombo("Cut", Key.X, KeyModifiers.Control), () => Selection.Cut());
            KeybindHandler.Register(new KeyCombo("Paste", Key.V, KeyModifiers.Control),
                () => Selection.Paste(_mouseCanvasSpace.X, _mouseCanvasSpace.Y, !Keyboard[Key.ShiftLeft],
                    _grid.Pitch));
            KeybindHandler.Register(new KeyCombo("Open Project", Key.O, KeyModifiers.Control),
                () => DialogEditor.AskOpenFile());
            KeybindHandler.Register(new KeyCombo("Save Project", Key.S, KeyModifiers.Control),
                () => DialogEditor.AskSaveFile());
            KeybindHandler.Register(new KeyCombo("Save Project As", Key.S, KeyModifiers.Control | KeyModifiers.Shift),
                () => DialogEditor.AskSaveFileAs());
            KeybindHandler.Register(new KeyCombo("Export Graph", Key.E, KeyModifiers.Control),
                () => DialogEditor.AskExportFile());
            KeybindHandler.Register(new KeyCombo("Export Graph as JSON", Key.E, KeyModifiers.Control | KeyModifiers.Shift),
                () => DialogEditor.AskExportJsonFile());
        }

        private void Render(object sender, FrameEventArgs e)
        {
            // Reset the view
            GL.Clear(ClearBufferMask.ColorBufferBit |
                     ClearBufferMask.DepthBufferBit |
                     ClearBufferMask.StencilBufferBit);

            //GL.PushMatrix();
            NanoVG.nvgBeginFrame(Nvg, Width, Height, 1);
            NanoVG.nvgSave(Nvg);
            NanoVG.nvgScale(Nvg, Zoom, Zoom);

            //_grid.Draw();

            NanoVG.nvgTranslate(Nvg, _grid.Offset.X, _grid.Offset.Y);
            NanoVG.nvgStrokeWidth(Nvg, 2);

            NanoVG.nvgStrokeColor(Nvg, NanoVG.nvgRGBA(0, 0, 0, 255));

            NanoVG.nvgBeginPath(Nvg);
            NanoVG.nvgMoveTo(Nvg, -_grid.Pitch, 0);
            NanoVG.nvgLineTo(Nvg, _grid.Pitch, 0);
            NanoVG.nvgStroke(Nvg);

            NanoVG.nvgBeginPath(Nvg);
            NanoVG.nvgMoveTo(Nvg, 0, -_grid.Pitch);
            NanoVG.nvgLineTo(Nvg, 0, _grid.Pitch);
            NanoVG.nvgStroke(Nvg);

            const int cxnLineWidth = 3;
            NanoVG.nvgStrokeWidth(Nvg, cxnLineWidth);
            if (Selection.DraggingConnection != null)
            {
                var end = _mouseCanvasSpace;

                var picked = Selection.HoveringConnection;
                if (picked != null && picked.Side != Selection.DraggingConnection.Side)
                {
                    var b = picked.GetBounds();
                    end = new Vector2(b.X, b.Y);
                }

                _nodeRenderer.RenderConnection(Selection.DraggingConnection, end);
            }

            foreach (var node in Graph)
                _nodeRenderer.RenderConnections(node);

            foreach (var node in Graph)
                _nodeRenderer.RenderNode(node);

            if (Selection.SelectionRectangle != null && Selection.SelectionRectangle.Width != 0 && Selection.SelectionRectangle.Height != 0)
            {
                NanoVG.nvgFillColor(Nvg, _selectionRectangleColor.ToNvgColor());
                NanoVG.nvgBeginPath(Nvg);
                var r = Selection.SelectionRectangle;
                var x = Math.Min(r.X, r.X + r.Width);
                var y = Math.Min(r.Y, r.Y + r.Height);
                var w = Math.Abs(r.Width);
                var h = Math.Abs(r.Height);
                NanoVG.nvgRect(Nvg, x, y, w, h);
                NanoVG.nvgFill(Nvg);
            }
            NanoVG.nvgRestore(Nvg);

            _contextMenu.Render();

            GL.Color4(0, 0, 0, 1f);
            if (Keyboard[Key.D] && Focused && TextBoxHandler.TextBox == null)
            {
                // Static diagnostic header
                NanoVG.nvgSave(Nvg);
                NanoVG.nvgFillColor(Nvg, Color.Black.ToNvgColor());
                NvgHelper.RenderString($"{Zoom}x Zoom");
                NanoVG.nvgTranslate(Nvg, 0, FontLineHeight);
                NvgHelper.RenderString($"{Graph.Count} Nodes");

                // Sparklines
                //                GL.Translate(5, (int)(Height - Font.Common.LineHeight * 1.4f * 2), 0);
                //                _fpsSparkline.Render(Color.Blue, Color.LimeGreen);
                //                GL.Translate(0, (int)(Font.Common.LineHeight * 1.4f), 0);
                //                _renderTimeSparkline.Render(Color.Blue, Color.LimeGreen);
                NanoVG.nvgRestore(Nvg);
            }
            
            NanoVG.nvgEndFrame(Nvg);
            // Swap the graphics buffer
            SwapBuffers();
        }

        public Vector2 ScreenToCanvasSpace(Vector2 input)
        {
            return input / Zoom - _grid.Offset;
        }

        private static void SetupOpenGl()
        {
            // Set up caps
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.RescaleNormal);

            // Set up blending
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Set background color
            GL.ClearColor(Color.White);

            GlNanoVG.nvgCreateGL(ref Nvg, (int)NvgCreateFlags.AntiAlias | (int)NvgCreateFlags.StencilStrokes);
            var rSans = NanoVG.nvgCreateFont(Nvg, "sans",
                $"Resources{Path.DirectorySeparatorChar}Fonts{Path.DirectorySeparatorChar}latosemi.ttf");
            if (rSans == -1)
                Lumberjack.Error("Unable to load sans");
        }

        private void SetZoom(int zoomIdx)
        {
            var zoomBefore = Zoom;
            _zoomIdx = zoomIdx;

            var size = new Vector2(Width, Height);
            _grid.Offset -= (size / zoomBefore - size / Zoom) / 2;
            _grid.Offset = new Vector2((int)_grid.Offset.X, (int)_grid.Offset.Y);
        }

        private void Update(object sender, FrameEventArgs e)
        {
            Title = $"FPS: {Math.Round(RenderFrequency)} | RenderTime: {Math.Round(RenderTime * 1000)}ms";

            // Grab the new keyboard state
            Keyboard = OpenTK.Input.Keyboard.GetState();

            if (_shouldDie)
                Exit();

            MarchingAnts.March();

            Selection.HoveringConnection = Graph.PickConnection(_mouseCanvasSpace.X, _mouseCanvasSpace.Y);

            var err = GL.GetError();
            if (err != ErrorCode.NoError)
                Lumberjack.Error(err.ToString());
        }
    }
}