using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NanoVGDotNet;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using MouseEventArgs = OpenTK.Input.MouseEventArgs;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    public class TextBox
    {
        public enum Direction
        {
            Forward,
            Backward
        }

        private readonly MainWindow _window;
        private bool _dragging;
        private bool _selecting;
        private int _selectionEnd = -1;
        private int _selectionStart = -1;
        private string _text = string.Empty;

        public EventHandler<EventArgs> Commit;

        public Rectangle BoundingBox { get; }
        public int CursorPos { get; set; }

        public int SelectionStart
        {
            get => Math.Min(_selectionStart, _selectionEnd);
            set => _selectionStart = value;
        }

        public int SelectionEnd
        {
            get => Math.Max(_selectionStart, _selectionEnd);
            set => _selectionEnd = value;
        }

        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }

        public string Text
        {
            get => _text;
            set => _text = value ?? string.Empty;
        }

        public TextBox(MainWindow window, Rectangle bounds)
        {
            _window = window;
            BoundingBox = bounds;

            BackgroundColor = Color.Black;
            ForegroundColor = Color.White;
        }

        private void DeleteCharacter(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    if (CursorPos != Text.Length)
                        Text = Text.Substring(0, CursorPos) +
                               Text.Substring(CursorPos + 1, Text.Length - CursorPos - 1);
                    break;
                case Direction.Backward:
                    if (CursorPos != 0)
                    {
                        Text = Text.Substring(0, CursorPos - 1) + Text.Substring(CursorPos, Text.Length - CursorPos);
                        CursorPos--;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private void DrawForegroundText(string s)
        {
            if (SelectionStart == -1 || SelectionEnd == -1)
            {
                NanoVG.nvgFillColor(MainWindow.Nvg, ForegroundColor.ToNvgColor());
                NvgHelper.RenderString(s);
            }
            else
            {
//                var selWidth = NvgHelper.MeasureString(Text.Substring(SelectionStart, SelectionEnd - SelectionStart)).Width;
//
//                GL.Color3(Color.DodgerBlue);
//                GL.Disable(EnableCap.Texture2D);
//                Fx.D2.DrawSolidRectangle(NvgHelper.MeasureString(Text.Substring(0, SelectionStart)).Width, 0,
//                    selWidth, _window.FontLineHeight);
//
//                GL.Translate(0, 0, 0.01);
//
//                GL.Color3(ForegroundColor);
//                GL.Enable(EnableCap.Texture2D);
//                NvgHelper.RenderString(s);
            }
        }

        private int GetTextIndex(float mouseLateral)
        {
            for (var i = 1; i < Text.Length; i++)
            {
                var substr = Text.Substring(0, i);
                var textLateral = NvgHelper.MeasureString(substr).Width - GetTextShift();
                if (!(textLateral > mouseLateral)) continue;
                return i;
            }

            return Text.Length;
        }

        private int GetTextShift()
        {
            var textSize = NvgHelper.MeasureString(Text.Substring(0, CursorPos));
            var shiftLeft = 0;

            var width = BoundingBox.Width - 10;

            if (textSize.Width >= width)
                shiftLeft = (int) (textSize.Width - width);
            return shiftLeft + 1;
        }

        private void Insert(string c)
        {
            c = c.Replace("\r", "").Replace("\n", "");
            Text = Text.Substring(0, CursorPos) + c + Text.Substring(CursorPos, Text.Length - CursorPos);
            CursorPos += c.Length;
        }

        public void InvokeCommit()
        {
            Commit?.Invoke(this, EventArgs.Empty);
        }

        public void OnCharacter(char c)
        {
            Insert(c.ToString());
        }

        public void OnKey(KeyboardKeyEventArgs k)
        {
            switch (k.Key)
            {
                case Key.Enter:
                case Key.KeypadEnter:
                    InvokeCommit();
                    break;
                case Key.Delete:
                    DeleteCharacter(Direction.Forward);
                    break;
                case Key.BackSpace:
                    DeleteCharacter(Direction.Backward);
                    break;
                case Key.Left:
                    CursorPos = Math.Max(0, CursorPos - 1);
                    break;
                case Key.Right:
                    CursorPos = Math.Min(Text.Length, CursorPos + 1);
                    break;
                case Key.Home:
                    CursorPos = 0;
                    break;
                case Key.End:
                    CursorPos = Text.Length;
                    break;
                case Key.V:
                    if (k.Control)
                    {
                        var text = "";
                        var staThread = new Thread(
                            delegate()
                            {
                                try
                                {
                                    text = Clipboard.GetText();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            });
                        staThread.SetApartmentState(ApartmentState.STA);
                        staThread.Start();
                        staThread.Join();
                        Insert(text);
                    }

                    break;
                default:
                    break;
            }
        }

        public void OnMouseDown(MouseEventArgs m)
        {
            SelectionStart = SelectionEnd = -1;

            var mouseLateral = _window.ScreenToCanvasSpace(new Vector2(m.X, m.Y)).X - BoundingBox.X;
            CursorPos = GetTextIndex(mouseLateral) - 1;

            _dragging = true;
        }

        public void OnMouseMove(MouseMoveEventArgs m)
        {
            //            if (_dragging && !_selecting)
            //            {
            //                _selecting = true;
            //                SelectionStart = CursorPos;
            //            }

            if (_selecting)
            {
                var mouseLateral = _window.ScreenToCanvasSpace(new Vector2(m.X, m.Y)).X - BoundingBox.X;
                var selectionEndIdx = GetTextIndex(mouseLateral) - 1;

                SelectionEnd = selectionEndIdx;

                CursorPos = selectionEndIdx;
            }
        }

        public void OnMouseUp(MouseEventArgs m)
        {
            _dragging = false;
            _selecting = false;
        }

        public void RenderBackground()
        {
            NanoVG.nvgSave(MainWindow.Nvg);
            
            NanoVG.nvgStrokeWidth(MainWindow.Nvg, 1);
            NanoVG.nvgFillColor(MainWindow.Nvg, BackgroundColor.ToNvgColor());
            NanoVG.nvgStrokeColor(MainWindow.Nvg, ForegroundColor.ToNvgColor());
            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgRect(MainWindow.Nvg, BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height);
            NanoVG.nvgFill(MainWindow.Nvg);
            NanoVG.nvgStroke(MainWindow.Nvg);

            if (DateTime.Now.Millisecond <= 500)
            {
                var shiftLeft = GetTextShift();

                NanoVG.nvgTranslate(MainWindow.Nvg, BoundingBox.X + 4 - shiftLeft,
                    BoundingBox.Y + (int) ((BoundingBox.Height - _window.FontLineHeight) / 2f));
                var size = NvgHelper.MeasureString(Text.Substring(0, CursorPos));

                NanoVG.nvgFillColor(MainWindow.Nvg, ForegroundColor.ToNvgColor());
                NanoVG.nvgBeginPath(MainWindow.Nvg);
                NanoVG.nvgRect(MainWindow.Nvg, (int) size.Width, 0, 2, _window.FontLineHeight);
                NanoVG.nvgFill(MainWindow.Nvg);
            }

            NanoVG.nvgRestore(MainWindow.Nvg);
        }

        public void RenderForeground()
        {
            NanoVG.nvgSave(MainWindow.Nvg);
            var shiftLeft = GetTextShift();

            NanoVG.nvgScissor(MainWindow.Nvg, BoundingBox.X + 2, BoundingBox.Y + 2, BoundingBox.Width - 4, BoundingBox.Height - 4);
            NanoVG.nvgTranslate(MainWindow.Nvg, BoundingBox.X + 4 - shiftLeft,
                BoundingBox.Y + (int) ((BoundingBox.Height - _window.FontLineHeight) / 2f));
            DrawForegroundText(Text);
            NanoVG.nvgResetScissor(MainWindow.Nvg);
            
            NanoVG.nvgRestore(MainWindow.Nvg);
        }
    }
}