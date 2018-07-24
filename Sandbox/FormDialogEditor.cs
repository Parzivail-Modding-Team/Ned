using Ned;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox
{
    public partial class FormDialogEditor : Form
    {
        private MainWindow _nodeEditor;

        private Graph _graph;
        private Node _selectedNode;

        public FormDialogEditor(MainWindow nodeEditor)
        {
            _nodeEditor = nodeEditor;
            InitializeComponent();
        }

        private void FormDialogEditor_Load(object sender, EventArgs e)
        {
            cbActor.DataSource = new List<Actor>
            {
                Actor.NPC,
                Actor.Player
            };

            ChangeSelectionTo(null);

            _graph = new Graph
            {
                new Node(NodeType.Start, "Start", 50, 50),
                new Node(NodeType.End, "End", 300, 100)
            };
        }

        private void FormDialogEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            _nodeEditor.Kill();
            e.Cancel = true;
        }

        public Graph GetGraph()
        {
            return _graph;
        }

        private void bAddNode_Click(object sender, EventArgs e)
        {
            _graph.Add(new Node(NodeType.Option, (Actor)cbActor.SelectedValue, 10, 10));
        }

        public void ChangeSelectionTo(Node node)
        {
            _selectedNode = node;

            if (node == null || node.Type != NodeType.Option)
            {
                lDialogOptions.SetObjects(null);
                lDialogOptions.Enabled = false;
            }
            else
            {
                lDialogOptions.SetObjects(node.Outputs);
                lDialogOptions.Enabled = true;
            }
        }

        private void lDialogOptions_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && _selectedNode != null)
            {
                _selectedNode.RemoveOutput((Connection)lDialogOptions.SelectedObject);
                _selectedNode.BuildConnections();
                lDialogOptions.BuildList();
            }
        }

        private void bAddDialogOption_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null && _selectedNode.Actor == Actor.Player)
            {
                _selectedNode.Outputs.Add(new Connection(_selectedNode, NodeSide.Output, 0, "Dialog Option"));
                _selectedNode.BuildConnections();
                lDialogOptions.BuildList();
            }
        }
    }
}
