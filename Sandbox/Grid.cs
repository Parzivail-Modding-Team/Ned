using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PFX.Util;

namespace Sandbox
{
    class Grid
    {
        public MainWindow Window { get; }
        public Vector2 Offset { get; set; }
        public int Pitch { get; set; } = 10;

        public Grid(MainWindow window)
        {
            Window = window;
        }

        public void Draw()
        {
            if (Window.Zoom < 0.5)
                return;

            var sZ = Pitch * Window.Zoom;

            GL.PushMatrix();
            GL.Translate(Offset.X % Pitch, Offset.Y % Pitch, 0);

            GL.Begin(PrimitiveType.Points);
            for (var x = -Pitch; x < Window.Width / Window.Zoom + sZ; x += Pitch)
            for (var y = -Pitch; y < Window.Height / Window.Zoom + sZ; y += Pitch)
                GL.Vertex2(x, y);
            GL.End();

            GL.PopMatrix();
        }
    }
}
