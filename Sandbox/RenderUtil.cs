using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PFX.BmFont;
using PFX.Util;

namespace Sandbox
{
    internal static class RenderUtil
    {
        public static string MakeStringFit(BitmapFont font, float maxWidth, string text)
        {
            if (font.MeasureString(text).Width < maxWidth)
                return text;

            while (text.Length > 0 && font.MeasureString(text + "...").Width > maxWidth)
                text = text.Substring(0, text.Length - 1);
            return text + "...";
        }

        public static void CentripetalCatmullRomTo(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            var numSegments = (int)((c - b).Length / 10);

            if (numSegments > 30)
                numSegments = 30;

            var segments = new Vector2[numSegments + 1];

            for (var i = 0; i <= numSegments; i++)
                segments[i] = Fx.D2.EvalCentripetalCatmullRom(a, b, c, d, (float)i / numSegments);

            DrawSmoothLine(3, segments, PrimitiveType.TriangleStrip);
        }

        public static void DrawSmoothLine(float thickness, Vector2[] segments, PrimitiveType mode)
        {
            for (var i = 0; i < segments.Length - 1; i++) DrawSmoothLineSegment(thickness, i == 0 ? Vector2.Zero : segments[i - 1], segments[i], segments[i + 1], i == segments.Length - 2 ? Vector2.Zero : segments[i + 2], mode);
        }

        private static void DrawSmoothLineSegment(float thickness, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, PrimitiveType mode)
        {
            if (p1 == p2)
                return;

            var line = (p2 - p1).Normalized();
            var normal = new Vector2(-line.Y, line.X).Normalized();

            var tangent1 = p0 == Vector2.Zero ? line : ((p1 - p0).Normalized() + line).Normalized();
            var tangent2 = p3 == Vector2.Zero ? line : ((p3 - p2).Normalized() + line).Normalized();

            var miter1 = new Vector2(-tangent1.Y, tangent1.X);
            var miter2 = new Vector2(-tangent2.Y, tangent2.X);

            var length1 = thickness / Vector2.Dot(normal, miter1);
            var length2 = thickness / Vector2.Dot(normal, miter2);

            GL.Begin(mode);

            if (p0 == Vector2.Zero)
            {
                GL.Vertex2(p1.X - length1 * miter1.X, p1.Y - length1 * miter1.Y);
                GL.Vertex2(p1.X + length1 * miter1.X, p1.Y + length1 * miter1.Y);
            }

            if (p3 == Vector2.Zero)
            {
                GL.Vertex2(p2.X + length2 * miter2.X, p2.Y + length2 * miter2.Y);
                GL.Vertex2(p2.X - length2 * miter2.X, p2.Y - length2 * miter2.Y);
            }

            GL.Vertex2(p2.X - length2 * miter2.X, p2.Y - length2 * miter2.Y);
            GL.Vertex2(p1.X - length1 * miter1.X, p1.Y - length1 * miter1.Y);

            GL.Vertex2(p2.X + length2 * miter2.X, p2.Y + length2 * miter2.Y);
            GL.Vertex2(p1.X + length1 * miter1.X, p1.Y + length1 * miter1.Y);

            GL.End();
        }

        public static void RoundRectangle(float x, float y, float w, float h, float radiusTopLeft, float radiusTopRight, float radiusBottomLeft, float radiusBottomRight, PrimitiveType mode)
        {
            var step = 45 / Math.Max(radiusTopLeft, Math.Max(radiusTopRight, Math.Max(radiusBottomLeft, radiusBottomRight)));
            if (step > 45)
                step = 45;
            GL.Begin(mode);
            BufferRoundRectangle(x, y, w, h, radiusTopLeft, radiusTopRight, radiusBottomLeft, radiusBottomRight, step);
            GL.End();
        }

        public static void BufferRoundRectangle(float x, float y, float w, float h, float radiusTopLeft, float radiusTopRight, float radiusBottomLeft, float radiusBottomRight, float step)
        {
            for (float i = -90; i <= 0; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusBottomLeft + radiusBottomLeft;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusBottomLeft + h - radiusBottomLeft;
                GL.Vertex2(nx + x, ny + y);
            }
            for (float i = 0; i <= 90; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusBottomRight + w - radiusBottomRight;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusBottomRight + h - radiusBottomRight;
                GL.Vertex2(nx + x, ny + y);
            }
            for (float i = 90; i <= 180; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusTopRight + w - radiusTopRight;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusTopRight + radiusTopRight;
                GL.Vertex2(nx + x, ny + y);
            }
            for (float i = 180; i <= 270; i += step)
            {
                var nx = Math.Sin(i * 3.141526f / 180) * radiusTopLeft + radiusTopLeft;
                var ny = Math.Cos(i * 3.141526f / 180) * radiusTopLeft + radiusTopLeft;
                GL.Vertex2(nx + x, ny + y);
            }
        }
    }
}