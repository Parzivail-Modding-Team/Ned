﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using PFX.Util;

namespace Sandbox
{
    class MarchingAnts
    {
        private static readonly byte[][] Pattern = { new byte[]{ 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195 }, new byte[] { 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240 }, new byte[] { 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60 }, new byte[] { 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15 }, new byte[] { 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195 }, new byte[] { 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240 }, new byte[] { 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60 }, new byte[] { 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15, 30, 30, 30, 30, 60, 60, 60, 60, 120, 120, 120, 120, 240, 240, 240, 240, 225, 225, 225, 225, 195, 195, 195, 195, 135, 135, 135, 135, 15, 15, 15, 15 } };
        private static int _index;
        private static int _indexTimer;

        public static void Update()
        {
            if (_indexTimer++ < 1)
                return;

            _indexTimer = 0;
            _index = (_index + 1) % Pattern.Length;
        }

        public static void Use()
        {
            GL.Enable(EnableCap.PolygonStipple);
            GL.PolygonStipple(Pattern[_index]);
        }

        public static void Release()
        {
            GL.Disable(EnableCap.PolygonStipple);
        }
    }
}
