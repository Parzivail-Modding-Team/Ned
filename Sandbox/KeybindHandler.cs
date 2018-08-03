using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Sandbox
{
    class KeybindHandler
    {
        private static readonly Dictionary<KeyCombo, Action> Keybinds = new Dictionary<KeyCombo, Action>();

        public static void Register(KeyCombo combo, Action action)
        {
            Keybinds.Add(combo, action);
        }

        public static void Consume(KeyCombo combo)
        {
            if (Keybinds.ContainsKey(combo))
                Keybinds[combo].Invoke();
        }

        public static List<KeyCombo> GetKeybinds()
        {
            return Keybinds.Keys.ToList();
        }
    }
}
