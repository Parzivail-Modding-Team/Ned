using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ned
{
    public class Graph : List<Node>
    {
        public Connection PickConnection(float x, float y, Func<Connection, bool> predicate = null)
        {
            foreach (var node in this)
            {
                if (node.Input != null && node.Input.GetBounds().Pick(x, y) && (predicate == null || predicate.Invoke(node.Input)))
                    return node.Input;

                foreach (var connection in node.Outputs)
                {
                    if (connection.GetBounds().Pick(x, y) && (predicate == null || predicate.Invoke(connection)))
                        return connection;
                }
            }

            return null;
        }

        public Node PickNode(float x, float y)
        {
            return this.FirstOrDefault(node => node.Pick(x, y));
        }

        public void ClearConnectionsFrom(Connection connection)
        {
            if (connection == null)
                return;

            if (connection.Side == NodeSide.Output)
                connection.ConnectedNode = null;
            else // Loop through the rest of the nodes since we only store a one-way connection to rmeove cyclic dependencies
                foreach (var node in this)
                foreach (var output in node.Outputs)
                {
                    if (output.ConnectedNode == connection)
                        output.ConnectedNode = null;
                }
        }
    }
}
