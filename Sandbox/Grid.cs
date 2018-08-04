using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    internal class Grid
    {
        private readonly MainWindow _window;

        public Vector2 Offset { get; set; }
        public int Pitch { get; set; } = 10;

        public Grid(MainWindow window)
        {
            _window = window;
        }

        public void Draw()
        {
            if (_window.Zoom < 1)
                return;

            var sZ = Pitch * _window.Zoom;

            GL.PushMatrix();
            GL.Translate(Offset.X % Pitch, Offset.Y % Pitch, 0);

            GL.Begin(PrimitiveType.Points);
            for (var x = -Pitch; x < _window.Width / _window.Zoom + sZ; x += Pitch)
            for (var y = -Pitch; y < _window.Height / _window.Zoom + sZ; y += Pitch)
                GL.Vertex2(x, y);

            GL.End();

            GL.PopMatrix();
        }
    }
}