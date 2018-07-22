using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ned
{
    public class Graph
    {
        public List<Node> Nodes { get; }

        public Graph()
        {
            Nodes = new List<Node>();
        }

        public void ClearConnectionsFrom(Connection connection)
        {
            if (connection.Side == NodeSide.Output)
                connection.ConnectedNode = null;
            else // Loop through the rest of the nodes since we only store a one-way connection to rmeove cyclic dependencies
                foreach (var node in Nodes)
                foreach (var output in node.Outputs)
                {
                    if (output.ConnectedNode == connection)
                        output.ConnectedNode = null;
                }
        }
    }
}
