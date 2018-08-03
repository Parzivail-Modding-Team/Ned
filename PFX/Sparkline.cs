using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using PFX.BmFont;
using PFX.Util;

namespace PFX
{
    public class Sparkline : ConcurrentQueue<float>
    {
        public enum SparklineStyle
        {
            Area,
            Line
        }

        private readonly BitmapFont _font;
        private readonly string _label;
        private readonly float _maxValue;
        private readonly SparklineStyle _style;

        private readonly object _syncObject = new object();

        public int MaxEntries { get; }

        public Sparkline(BitmapFont font, string label, int maxEntries, float maxValue, SparklineStyle style)
        {
            _font = font;
            _label = label;
            MaxEntries = maxEntries;
            _maxValue = maxValue;
            _style = style;
        }

        private double Clamp(float x)
        {
            if (x > _maxValue)
                return _maxValue;
            return x < 0 ? 0 : x;
        }

        public new void Enqueue(float obj)
        {
            base.Enqueue(obj);
            lock (_syncObject)
            {
                while (Count > MaxEntries)
                    TryDequeue(out var outObj);
            }
        }

        public void Render(Color foreColor, Color backColor, params object[] formatArgs)
        {
            GL.PushMatrix();
            var label = string.Format(_label, formatArgs);
            var scalar = _font.Common.LineHeight / _maxValue;

            GL.PushAttrib(AttribMask.EnableBit);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.LineSmooth);
            GL.Disable(EnableCap.PolygonSmooth);
            GL.Disable(EnableCap.PointSmooth);
            GL.Disable(EnableCap.DepthTest);

            GL.Color3(backColor);
            Fx.D2.DrawSolidRectangle(0, 0, MaxEntries, _font.Common.LineHeight);

            GL.Color3(foreColor);
            GL.LineWidth(1);
            switch (_style)
            {
                case SparklineStyle.Area:
                    GL.Begin(PrimitiveType.Lines);
                    for (var i = 0; i < Count; i++)
                    {
                        GL.Vertex2(i + 1, _font.Common.LineHeight);
                        GL.Vertex2(i + 1, _font.Common.LineHeight - scalar * Clamp(this.ElementAt(i)) - 1);
                    }

                    GL.End();
                    break;
                case SparklineStyle.Line:
                    GL.Begin(PrimitiveType.LineStrip);
                    for (var i = 0; i < Count; i++)
                        GL.Vertex2(i + 1, _font.Common.LineHeight - scalar * Clamp(this.ElementAt(i)));
                    GL.End();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GL.Translate(MaxEntries + 2, 0, 0);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.Texture2D);
            _font.RenderString(label);

            GL.PopAttrib();
            GL.PopMatrix();
        }
    }
}