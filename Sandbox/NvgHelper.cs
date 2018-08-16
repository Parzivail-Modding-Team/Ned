using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
