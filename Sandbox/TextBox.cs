using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using PFX.Util;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    public class TextBox
    {
        private readonly MainWindow _window;
        private string _text = string.Empty;

        public Rectangle BoundingBox { get; }
        public int CursorPos { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }

        public EventHandler<EventArgs> Commit;

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
        
        public void RenderBackground()
        {
            GL.PushMatrix();
            GL.PushAttrib(AttribMask.EnableBit);
            GL.Disable(EnableCap.Texture2D);

            GL.Color3(ForegroundColor);
            Fx.D2.DrawSolidRectangle(BoundingBox.X - 1, BoundingBox.Y - 1, BoundingBox.Width + 2, BoundingBox.Height + 2);
            GL.Translate(0, 0, 0.01);
            GL.Color3(BackgroundColor);
            Fx.D2.DrawSolidRectangle(BoundingBox.X, BoundingBox.Y, BoundingBox.Width, BoundingBox.Height);

            if (DateTime.Now.Millisecond <= 500 || 1 == 1)
            {
                var shiftLeft = GetTextShift();

                GL.Translate(BoundingBox.X + 4 - shiftLeft,
                    BoundingBox.Y + (int) ((BoundingBox.Height - _window.Font.Common.LineHeight) / 2f), 0);
                var size = _window.Font.MeasureString(Text.Substring(0, CursorPos));

                GL.Translate(0, 0, 0.01);

                GL.Color3(ForegroundColor);
                Fx.D2.DrawSolidRectangle((int) size.Width + 1, 0, 2, _window.Font.Common.LineHeight);
            }

            GL.PopAttrib();
            GL.PopMatrix();
        }

        public void RenderForeground()
        {
            GL.PushMatrix();
            GL.PushAttrib(AttribMask.EnableBit);
            GL.Enable(EnableCap.Texture2D);
            var shiftLeft = GetTextShift();

            GL.Enable(EnableCap.ScissorTest);
            Fx.Util.Scissor(_window, (int) (BoundingBox.X + 2), (int) (BoundingBox.Y + 2), (int) (BoundingBox.Width - 4), (int) (BoundingBox.Height - 4));
            GL.Translate(BoundingBox.X + 4 - shiftLeft,
                BoundingBox.Y + (int)((BoundingBox.Height - _window.Font.Common.LineHeight) / 2f), 0.01);
            DrawForegroundText(Text);
            GL.PopAttrib();
            GL.PopMatrix();
        }

        private void DrawForegroundText(string s)
        {
            GL.Color3(ForegroundColor);
            _window.Font.RenderString(s);
        }

        private int GetTextShift()
        {
            var textSize = _window.Font.MeasureString(Text.Substring(0, CursorPos));
            var shiftLeft = 0;
            
            var width = BoundingBox.Width - 10;

            if (textSize.Width >= width)
                shiftLeft = (int)(textSize.Width - width);
            return shiftLeft;
        }

        public void OnCharacter(char c)
        {
            Insert(c.ToString());
        }

        private void Insert(string c)
        {
            c = c.Replace("\r", "").Replace("\n", "");
            Text = Text.Substring(0, CursorPos) + c + Text.Substring(CursorPos, Text.Length - CursorPos);
            CursorPos += c.Length;
        }

        private void DeleteCharacter(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    if (CursorPos != Text.Length)
                        Text = Text.Substring(0, CursorPos) + Text.Substring(CursorPos + 1, Text.Length - CursorPos - 1);
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
                            delegate ()
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

        public void InvokeCommit()
        {
            Commit?.Invoke(this, EventArgs.Empty);
        }

        public enum Direction
        {
            Forward,
            Backward
        }
    }
}
