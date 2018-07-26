using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ned;
using OpenTK.Graphics.OpenGL;
using PFX.Util;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    class ContextMenu : List<ContextMenuItem>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; }
        public bool Visible { get; set; }

        public ContextMenu(float x, float y, int width)
        {
            X = x;
            Y = y;
            Width = width;
        }

        public void Render()
        {
            if (!Visible)
                return;

            GL.PushMatrix();
            GL.Translate(X, Y, 0);
            foreach (var menuItem in this)
                menuItem.Render();
            GL.PopMatrix();
        }

        public new void Add(ContextMenuItem item)
        {
            item.Parent = this;
            item.Index = Count;
            base.Add(item);
        }
    }

    internal class ContextMenuItem : IShape
    {
        private readonly MainWindow _window;

        public string Text { get; }
        public Action<ContextMenuItem> Action { get; }
        public int Index { get; set; }
        public ContextMenu Parent { get; set; }

        public ContextMenuItem(MainWindow window, string text, Action<ContextMenuItem> action)
        {
            _window = window;
            Text = text;
            Action = action;
        }

        public void Render()
        {
            GL.PushAttrib(AttribMask.EnableBit);
            var lineHeight = (int) (_window.Font.Common.LineHeight * 1.5f);
            GL.Disable(EnableCap.Texture2D);
            GL.Color3(Color.White);
            Fx.D2.DrawSolidRectangle(-1, lineHeight * Index - 1, Parent.Width + 2, lineHeight + 2);
            GL.Color3(Pick(_window.MouseScreenSpace.X, _window.MouseScreenSpace.Y) ? Color.DodgerBlue : Color.Black);
            Fx.D2.DrawSolidRectangle(0, lineHeight * Index, Parent.Width, lineHeight);
            GL.PushMatrix();
            GL.Color3(Color.White);
            GL.Translate(3, lineHeight * Index + 3, 0);
            GL.Enable(EnableCap.Texture2D);
            _window.Font.RenderString(Text);
            GL.PopMatrix();
            GL.PopAttrib();
        }

        public bool Pick(float x, float y)
        {
            var lineHeight = (int)(_window.Font.Common.LineHeight * 1.5f);
            return new Rectangle(Parent.X, Parent.Y + lineHeight * Index, Parent.Width, lineHeight).Pick(x, y);
        }
    }
}
