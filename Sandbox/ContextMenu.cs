using System;
using System.Collections.Generic;
using System.Drawing;
using NanoVGDotNet;
using Ned;
using OpenTK.Graphics.OpenGL;
using Rectangle = Ned.Rectangle;

namespace Sandbox
{
    internal class ContextMenu : List<ContextMenuItem>
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public bool Visible { get; set; }

        public ContextMenu(float x, float y, int width)
        {
            X = x;
            Y = y;
            Width = width;
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
                Width = (int) Math.Max(Width, item.Width + 10);
        }

        public void Render()
        {
            if (!Visible)
                return;

            NanoVG.nvgSave(MainWindow.Nvg);
            NanoVG.nvgTranslate(MainWindow.Nvg, X, Y);
            foreach (var menuItem in this)
                menuItem.RenderBackground();
            foreach (var menuItem in this)
                menuItem.Render();
            NanoVG.nvgRestore(MainWindow.Nvg);
        }
    }

    internal class ContextMenuItem : IShape
    {
        private readonly MainWindow _window;

        public string Shortcut { get; }
        public string Text { get; }
        public Action<ContextMenuItem> Action { get; }
        public int Index { get; set; }
        public ContextMenu Parent { get; set; }
        public float Width => NvgHelper.MeasureString($"{Shortcut} {Text}").Width;

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

        public bool Pick(float x, float y)
        {
            var lineHeight = (int) (_window.FontLineHeight * 1.5f);
            return new Rectangle(Parent.X, Parent.Y + lineHeight * Index, Parent.Width, lineHeight).Pick(x, y);
        }

        public void Render()
        {
            var picked = Pick(_window.MouseScreenSpace.X, _window.MouseScreenSpace.Y);
            
            var lineHeight = (int) (_window.FontLineHeight * 1.5f);
            NanoVG.nvgFillColor(MainWindow.Nvg, picked ? Color.LightGray.ToNvgColor() : Color.White.ToNvgColor());
            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgRect(MainWindow.Nvg, 0, lineHeight * Index, Parent.Width, lineHeight);
            NanoVG.nvgFill(MainWindow.Nvg);

            NanoVG.nvgSave(MainWindow.Nvg);
            NanoVG.nvgFillColor(MainWindow.Nvg, Color.Black.ToNvgColor());
            NanoVG.nvgTranslate(MainWindow.Nvg, 3, lineHeight * Index + 3);
            if (Shortcut == null)
                NvgHelper.RenderString(Text);
            else
            {
                NvgHelper.RenderString($"{Shortcut} {Text}");
                NanoVG.nvgFillColor(MainWindow.Nvg, Color.DarkGray.ToNvgColor());
                NvgHelper.RenderString(Shortcut);
            }

            NanoVG.nvgRestore(MainWindow.Nvg);
        }

        public void RenderBackground()
        {
            var lineHeight = (int) (_window.FontLineHeight * 1.5f);
            NanoVG.nvgFillColor(MainWindow.Nvg, Color.Black.ToNvgColor());
            NanoVG.nvgBeginPath(MainWindow.Nvg);
            NanoVG.nvgRect(MainWindow.Nvg, -1, lineHeight * Index - 1, Parent.Width + 2, lineHeight + 2);
            NanoVG.nvgRect(MainWindow.Nvg, 1, lineHeight * Index + 1, Parent.Width + 1, lineHeight + 1);
            NanoVG.nvgFill(MainWindow.Nvg);
        }
    }
}