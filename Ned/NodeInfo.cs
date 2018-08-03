using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace Ned
{
    public class NodeInfo
    {
        private static readonly Dictionary<string, NodeInfo> _nodeTypes = new Dictionary<string, NodeInfo>();
        public string Name { get; }
        public int Type { get; }
        public Action<Node> AddConnections { get; }
        public bool CanEditConnectors { get; }
        public bool CanEditNode { get; }

        public static readonly NodeInfo None = new NodeInfo(-1, "None", false, false, node => { });
        public static readonly NodeInfo Start = new NodeInfo(0, "Interact", false, false, node =>
        {
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "", false));
        });
        public static readonly NodeInfo End = new NodeInfo(1, "Exit", false, false, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
        });
        public static readonly NodeInfo NpcDialogue = new NodeInfo(2, "NPC", true, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.AddOutput("NPC Dialogue");

        });
        public static readonly NodeInfo PlayerDialogue = new NodeInfo(3, "Player", true, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.AddOutput("Dialogue Option 1");
            node.AddOutput("Dialogue Option 2");
            node.AddOutput("Dialogue Option 3");

        });
        public static readonly NodeInfo WaitForFlag = new NodeInfo(4, "Has Flag", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "flagname"));
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "[Else]", false));
        });
        public static readonly NodeInfo SetFlag = new NodeInfo(5, "Set Flag", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "flagname"));
        });
        public static readonly NodeInfo ClearFlag = new NodeInfo(6, "Clear Flag", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "flagname"));
        });
        public static readonly NodeInfo HasQuest = new NodeInfo(7, "Is Quest Active", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "quetsname"));
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "[Else]", false));
        });
        public static readonly NodeInfo StartQuest = new NodeInfo(8, "Start Quest", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "quetsname"));
        });
        public static readonly NodeInfo CompleteQuest = new NodeInfo(9, "Complete Quest", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "questname"));
        });
        public static readonly NodeInfo TriggerEvent = new NodeInfo(10, "Trigger Event", false, true, node =>
        {
            node.Input = new Connection(node, NodeSide.Input, 0, "");
            node.Outputs.Add(new Connection(node, NodeSide.Output, node.Outputs.Count, "eventname"));
        });

        private NodeInfo(int type, string name, bool canEditConnectors, bool canEditNode, Action<Node> addConnections)
        {
            Type = type;
            Name = name;
            AddConnections = addConnections;
            CanEditConnectors = canEditConnectors;
            CanEditNode = canEditNode;
            _nodeTypes.Add(name, this);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeInfo function && Name.Equals(function.Name);
        }

        public override int GetHashCode()
        {
            var hashCode = 1947881711;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(NodeInfo function1, NodeInfo function2)
        {
            return EqualityComparer<NodeInfo>.Default.Equals(function1, function2);
        }

        public static bool operator !=(NodeInfo function1, NodeInfo function2)
        {
            return !(function1 == function2);
        }

        public static NodeInfo GetByName(string name)
        {
            return _nodeTypes[name];
        }
    }
}
