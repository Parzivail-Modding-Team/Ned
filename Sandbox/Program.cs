using System;
using System.Windows.Forms;

namespace Sandbox
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new MainWindow().Run(20, 60);
        }
    }
}