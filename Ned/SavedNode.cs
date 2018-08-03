using System;
using System.Collections.Generic;

namespace Ned
{
    internal class SavedNode
    {
        public Guid Id;
        public SavedConnection Input;
        public float Layer;
        public string Name;
        public List<SavedConnection> Outputs;
        public int Type;
        public float X;
        public float Y;
    }
}