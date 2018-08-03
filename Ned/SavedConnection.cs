using System;

namespace Ned
{
    internal class SavedConnection
    {
        public bool CanEditName;
        public Guid? ConnectedNode;
        public int ConnectionIndex;
        public Guid Id;
        public Guid ParentNode;
        public NodeSide Side;
        public string Text;
    }
}