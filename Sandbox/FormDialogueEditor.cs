using System;
using System.Windows.Forms;
using Ned;
using PFX;

namespace Sandbox
{
    public partial class FormDialogueEditor : Form
    {
        private readonly MainWindow _nodeEditor;
        private string _fileName;

        private Graph _graph;
        private Node _selectedNode;

        public FormDialogueEditor(MainWindow nodeEditor)
        {
            _nodeEditor = nodeEditor;
            InitializeComponent();
        }

        public string FileName
        {
            get => _fileName;

            private set
            {
                _fileName = value;
                Text = string.Format(Resources.AppTitleWorking, value);
            }
        }

        private void FormDialogEditor_Load(object sender, EventArgs e)
        {
            Text = Resources.AppTitleStatic;

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

        public void ChangeSelectionTo(Node node)
        {
            _selectedNode = node;

            if (node == null || node.Type != NodeType.Flow)
            {
                lDialogOptions.SetObjects(null);
                lDialogOptions.Enabled = false;
                bAddDialogOption.Enabled = false;
                bRemoveDialogOption.Enabled = false;
            }
            else
            {
                lDialogOptions.SetObjects(node.Outputs);
                lDialogOptions.Enabled = true;
                bAddDialogOption.Enabled = true;
                bRemoveDialogOption.Enabled = true;
            }
        }

        private void bAddDialogOption_Click(object sender, EventArgs e)
        {
            if (_selectedNode == null || _selectedNode.Actor != Actor.Player) return;

            _selectedNode.Outputs.Add(new Connection(_selectedNode, NodeSide.Output, 0, "Dialog Option"));
            _selectedNode.BuildConnections();
            lDialogOptions.SetObjects(_selectedNode.Outputs);
        }

        private void bRemoveDialogOption_Click(object sender, EventArgs e)
        {
            if (_selectedNode == null || _selectedNode.Actor != Actor.Player) return;

            _selectedNode.RemoveOutput((Connection) lDialogOptions.SelectedObject);
            _selectedNode.BuildConnections();
            lDialogOptions.SetObjects(_selectedNode.Outputs);
        }

        private void bOpen_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            FileName = ofd.FileName;

            Lumberjack.Info($"Opening {FileName}...");
            _graph = Graph.Load(ofd.FileName);
            Lumberjack.Info($"Opened {FileName}.");
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            if (FileName == null)
            {
                bSaveAs_Click(sender, e);
                return;
            }

            Lumberjack.Info($"Saving {FileName}...");
            _graph.SaveAs(FileName);
            Lumberjack.Info($"Saved {FileName}.");
        }

        private void bSaveAs_Click(object sender, EventArgs e)
        {
            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            FileName = sfd.FileName;

            Lumberjack.Info($"Saving {FileName}...");
            _graph.SaveAs(sfd.FileName);
            Lumberjack.Info($"Saved {FileName}.");
        }
    }
}