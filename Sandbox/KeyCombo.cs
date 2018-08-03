using System.Collections.Generic;
using OpenTK.Input;

namespace Sandbox
{
    internal class KeyCombo
    {
        public string Name { get; }
        public Key Key { get; }
        public bool Control { get; }
        public bool Alt { get; }
        public bool Shift { get; }

        public KeyCombo(string name, Key key, KeyModifiers modifiers = 0)
        {
            Name = name;
            Key = key;
            Control = modifiers.HasFlag(KeyModifiers.Control);
            Alt = modifiers.HasFlag(KeyModifiers.Alt);
            Shift = modifiers.HasFlag(KeyModifiers.Shift);
        }

        public KeyCombo(KeyboardKeyEventArgs kkea)
        {
            Key = kkea.Key;
            Control = kkea.Control;
            Alt = kkea.Alt;
            Shift = kkea.Shift;
        }

        public override string ToString()
        {
            var terms = new List<string>();
            if (Control)
                terms.Add("CTRL");
            if (Alt)
                terms.Add("ALT");
            if (Shift)
                terms.Add("Shift");
            terms.Add(Key.ToString());

            return $"{Name} ({string.Join("+", terms)})";
        }

        public override bool Equals(object obj)
        {
            var combo = obj as KeyCombo;
            return combo != null &&
                   Key == combo.Key &&
                   Control == combo.Control &&
                   Alt == combo.Alt &&
                   Shift == combo.Shift;
        }

        public override int GetHashCode()
        {
            var hashCode = -1076853235;
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + Control.GetHashCode();
            hashCode = hashCode * -1521134295 + Alt.GetHashCode();
            hashCode = hashCode * -1521134295 + Shift.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(KeyCombo combo1, KeyCombo combo2)
        {
            return EqualityComparer<KeyCombo>.Default.Equals(combo1, combo2);
        }

        public static bool operator !=(KeyCombo combo1, KeyCombo combo2)
        {
            return !(combo1 == combo2);
        }
    }
}