using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NanoVGDotNet;
using OpenTK.Graphics;

namespace Sandbox
{
    static class NvgHelper
    {
        public static NVGcolor ToNvgColor(this Color color)
        {
            return NanoVG.nvgRGBA(color.R, color.G, color.B, color.A);
        }

        public static NVGcolor ToNvgColor(this Color4 color)
        {
            return NanoVG.nvgRGBA((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255), (byte)(color.A * 255));
        }

        public static SizeF MeasureString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return SizeF.Empty;

            if (text.EndsWith(" "))
                text = $"{text.Substring(0, text.Length - 1)}(";
            NanoVG.nvgFontFace(MainWindow.Nvg, "sans");
            var sb = new float[4];

            try
            {
                NanoVG.nvgTextBounds(MainWindow.Nvg, 0, 0, text, sb);
            }
            catch (Exception e)
            {
                Lumberjack.Error($"NvgHelper::MeasureString: {e.Message}");
            }

            var sfw = sb[2] - sb[0];
            var sfh = sb[3] - sb[1];
            return new SizeF(sfw, sfh);
        }

        public static void RenderString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return;

            NanoVG.nvgTextAlign(MainWindow.Nvg, (int)NvgAlign.Top | (int)NvgAlign.Left);
            NanoVG.nvgFontSize(MainWindow.Nvg, 16);
            NanoVG.nvgFontFace(MainWindow.Nvg, "sans");
            try
            {
                NanoVG.nvgText(MainWindow.Nvg, 0, 0, s);
            }
            catch (Exception e)
            {
                Lumberjack.Error($"NvgHelper::RenderString: {e.Message}");
            }
        }
    }
}
