using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ned
{
    internal class SavedNode
    {
        public Guid Id;
        public SavedConnection Input;
        public List<SavedConnection> Outputs;
        public float X { get; set; }
        public float Y { get; set; }
        public string Name;
        public NodeType Type;
        public Actor Actor;
    }

    internal class SavedConnection
    {
        public Guid Id;
        public NodeSide Side;
        public int ConnectionIndex;
        public string Text;
        public Guid ParentNode;
        public Guid? ConnectedNode;
    }
}
