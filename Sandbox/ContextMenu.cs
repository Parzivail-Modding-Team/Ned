using System;
using System.Collections.Generic;
using System.Drawing;
using Ned;
using OpenTK.Graphics.OpenGL;
using PFX.Util;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    internal class ContextMenu : List<ContextMenuItem>
    {
        public ContextMenu(float x, float y, int width)
        {
            X = x;
            Y = y;
            Width = width;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public bool Visible { get; set; }

        public void Render()
        {
            if (!Visible)
                return;

            GL.PushMatrix();
            GL.Translate(X, Y, 0);
            foreach (var menuItem in this)
                menuItem.RenderBackground();
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

        public void RecalculateWidth()
        {
            Width = 50;

            foreach (var item in this)
                Width = (int)Math.Max(Width, item.Width + 10);
        }
    }

    internal class ContextMenuItem : IShape
    {
        private readonly MainWindow _window;

        public ContextMenuItem(MainWindow window, string text, Action<ContextMenuItem> action)
        {
            _window = window;
            Text = text;
            Action = action;
        }

        public ContextMenuItem(MainWindow window, string shortcut, string text, Action<ContextMenuItem> action)
        {
            _window = window;
            Shortcut = shortcut;
            Text = text;
            Action = action;
        }

        public string Shortcut { get; }
        public string Text { get; }
        public Action<ContextMenuItem> Action { get; }
        public int Index { get; set; }
        public ContextMenu Parent { get; set; }
        public float Width => _window.Font.MeasureString($"{Shortcut} {Text}").Width;

        public bool Pick(float x, float y)
        {
            var lineHeight = (int) (_window.Font.Common.LineHeight * 1.5f);
            return new Rectangle(Parent.X, Parent.Y + lineHeight * Index, Parent.Width, lineHeight).Pick(x, y);
        }

        public void Render()
        {
            var picked = Pick(_window.MouseScreenSpace.X, _window.MouseScreenSpace.Y);

            GL.PushAttrib(AttribMask.EnableBit);
            var lineHeight = (int) (_window.Font.Common.LineHeight * 1.5f);
            GL.Disable(EnableCap.Texture2D);
            GL.Color3(picked ? Color.LightGray : Color.White);
            Fx.D2.DrawSolidRectangle(0, lineHeight * Index, Parent.Width, lineHeight);
            GL.PushMatrix();
            GL.Color3(Color.Black);
            GL.Translate(3, lineHeight * Index + 3, 0);
            GL.Enable(EnableCap.Texture2D);
            if (Shortcut == null)
            {
                _window.Font.RenderString(Text);
            }
            else
            {
                _window.Font.RenderString($"{Shortcut} {Text}");
                GL.Color3(Color.DarkGray);
                _window.Font.RenderString(Shortcut);
            }

            GL.PopMatrix();
            GL.PopAttrib();
        }

        public void RenderBackground()
        {
            GL.PushAttrib(AttribMask.EnableBit);
            var lineHeight = (int) (_window.Font.Common.LineHeight * 1.5f);
            GL.Disable(EnableCap.Texture2D);
            GL.Color3(Color.Black);
            Fx.D2.DrawSolidRectangle(-1, lineHeight * Index - 1, Parent.Width + 2, lineHeight + 2);
            Fx.D2.DrawSolidRectangle(1, lineHeight * Index + 1, Parent.Width + 1, lineHeight + 1);
            GL.PopAttrib();
        }
    }
}