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
            var n1 = new Node(50, 50, "Node 1");
            var n2 = new Node(350, 80, "Node 2");

            n1.Outputs[0].ConnectTo(n1);

            var g = new Graph();
            g.Nodes.Add(n1);
            g.Nodes.Add(n2);

            new MainWindow(g).Run(20, 60);
        }
    }
}
