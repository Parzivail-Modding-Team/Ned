using Ned;

namespace Sandbox
{
    internal class TextBoxHandler
    {
        public static TextBox TextBox;
        public static Connection EditingConnection;

        public static void Destroy(bool invokeCommit = true)
        {
            if (invokeCommit)
                TextBox?.InvokeCommit();

            EditingConnection = null;
            TextBox = null;
        }

        public static TextBox PickAndCreate(MainWindow window, Graph graph, float x, float y)
        {
            var pickedNode = graph.PickNode(x, y);

            if (TextBox != null && TextBox.BoundingBox.Pick(x, y))
                return TextBox;

            if (pickedNode == null) return null;

            foreach (var output in pickedNode.Outputs)
            {
                var s = window.Font.MeasureString(output.Text);
                var bound = output.GetBounds();
                var r = bound.Radius;
                var twor = 2 * r;

                var outputRect = new Rectangle(bound.X - twor - s.Width, bound.Y - r, s.Width, s.Height);
                if (!outputRect.Pick(x, y) || !output.CanEditName) continue;

                StartEditing(window, output);
                return TextBox;
            }

            return null;
        }

        public static void StartEditing(MainWindow window, Connection output)
        {
            var s = window.Font.MeasureString(output.Text);
            var bound = output.GetBounds();
            var r = bound.Radius;
            var twor = 2 * r;
            var outputRect = new Rectangle(bound.X - twor - s.Width - 3, bound.Y - r - 3, s.Width + 6, s.Height + 6);

            if (TextBox != null)
                Destroy();

            EditingConnection = output;
            TextBox = new TextBox(window, outputRect)
            {
                Text = output.Text,
                CursorPos = output.Text.Length
            };
            TextBox.Commit += (sender, args) =>
            {
                var finalText = ((TextBox) sender).Text;
                if (string.IsNullOrWhiteSpace(finalText))
                    finalText = "Empty Dialog Option";

                EditingConnection.Text = finalText;
                Destroy(false);
            };
        }
    }
}