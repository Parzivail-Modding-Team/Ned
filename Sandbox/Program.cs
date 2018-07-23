using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ned;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            var g = new Graph();
            g.Nodes.Add(new Node(50, 50, "Node 1"));
            g.Nodes.Add(new Node(350, 80, "Node 2"));
            g.Nodes.Add(new Node(200, 310, "Node 3"));

            new MainWindow(g).Run(20, 60);
        }
    }
}
