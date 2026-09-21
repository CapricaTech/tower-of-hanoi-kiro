namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Applies ANSI escape-code colors to text. When color is disabled it returns
    /// text unchanged, so callers never need to branch on color support. Uses only
    /// ANSI sequences (no System.Drawing / Windows APIs) for macOS + Linux/Mono.
    /// </summary>
    public sealed class AnsiColor
    {
        public const string Reset = "\u001b[0m";
        public const string Bold = "\u001b[1m";

        // A fixed palette of bright foreground colors, indexed for a consistent
        // per-disc-size color throughout a game. Index 0 is unused so disc size
        // maps directly (size 1 -> _palette[1]).
        private static readonly string[] Palette =
        {
            "\u001b[37m",   // 0 (unused) - white
            "\u001b[91m",   // 1 - bright red
            "\u001b[92m",   // 2 - bright green
            "\u001b[93m",   // 3 - bright yellow
            "\u001b[94m",   // 4 - bright blue
            "\u001b[95m",   // 5 - bright magenta
            "\u001b[96m",   // 6 - bright cyan
            "\u001b[33m",   // 7 - yellow
            "\u001b[35m"    // 8 - magenta
        };

        private readonly bool _enabled;

        public AnsiColor(ColorSupport support)
        {
            _enabled = support.IsColorEnabled;
        }

        public bool Enabled
        {
            get { return _enabled; }
        }

        /// <summary>
        /// Wraps text with a color code and a reset. Returns the text unchanged
        /// when color is disabled.
        /// </summary>
        public string Wrap(string text, string colorCode)
        {
            if (!_enabled || string.IsNullOrEmpty(colorCode))
            {
                return text;
            }
            return colorCode + text + Reset;
        }

        /// <summary>Returns a consistent color code for a given disc size.</summary>
        public string ForDisc(int discSize)
        {
            if (discSize <= 0 || discSize >= Palette.Length)
            {
                return Palette[0];
            }
            return Palette[discSize];
        }

        /// <summary>Colors text according to the disc size palette.</summary>
        public string ColorDisc(string text, int discSize)
        {
            return Wrap(text, ForDisc(discSize));
        }
    }
}
