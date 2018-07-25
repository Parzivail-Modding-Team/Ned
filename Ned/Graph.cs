using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        public Connection GetConnection(Guid id)
        {
            foreach (var node in this)
            {
                if (node.Input != null && node.Input.Id == id)
                    return node.Input;

                foreach (var connection in node.Outputs)
                    if (connection.Id == id)
                        return connection;
            }

            return null;
        }

        public Node GetNode(Guid id)
        {
            foreach (var node in this)
                if (node.Id == id)
                    return node;

            return null;
        }

        public void SaveAs(string fileName)
        {
            var savedGraph = this.Select(node => node.Save()).ToList();
            File.WriteAllText(fileName, JsonConvert.SerializeObject(savedGraph));
        }

        public static Graph Load(string fileName)
        {
            var graph = new Graph();

            var json = JsonConvert.DeserializeObject<List<SavedNode>>(File.ReadAllText(fileName));

            graph.AddRange(json.Select(node => new Node(node)));

            foreach (var node in graph)
                node.FinishLoading(graph);

            foreach (var node in graph)
                node.MakeConnections(graph);

            return graph;
        }
    }
}
