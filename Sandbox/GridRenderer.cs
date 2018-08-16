using NanoVGDotNet;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    internal class GridRenderer
    {
        private readonly MainWindow _window;

        public Vector2 Offset { get; set; }
        public int Pitch { get; set; } = 10;

        public GridRenderer(MainWindow window)
        {
            _window = window;
        }

        public void Draw()
        {
            if (_window.Zoom < 1)
                return;

            var sZ = Pitch * _window.Zoom;

            NanoVG.nvgSave(MainWindow.Nvg);
            NanoVG.nvgTranslate(MainWindow.Nvg, Offset.X % Pitch, Offset.Y % Pitch);
            
            NanoVG.nvgFillColor(MainWindow.Nvg, NanoVG.nvgRGBA(0, 0, 0, 64));

            for (var x = -Pitch; x < _window.Width / _window.Zoom + sZ; x += Pitch)
            for (var y = -Pitch; y < _window.Height / _window.Zoom + sZ; y += Pitch)
            {
                NanoVG.nvgBeginPath(MainWindow.Nvg);
                NanoVG.nvgCircle(MainWindow.Nvg, x, y, 2);
                NanoVG.nvgFill(MainWindow.Nvg);
            }


            NanoVG.nvgRestore(MainWindow.Nvg);
        }
    }
}