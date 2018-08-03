using System;

namespace Ned
{
    internal class SavedConnection
    {
        public Guid Id;
        public NodeSide Side;
        public int ConnectionIndex;
        public string Text;
        public Guid ParentNode;
        public Guid? ConnectedNode;
        public bool CanEditName;
    }
}