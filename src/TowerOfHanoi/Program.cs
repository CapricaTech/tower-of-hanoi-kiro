using System;
using TowerOfHanoi.Application;
using TowerOfHanoi.Presentation;

namespace TowerOfHanoi
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Resolve color support: environment detection, overridable by a
            // --no-color flag for convenience.
            bool forceNoColor = HasFlag(args, "--no-color");
            ColorSupport colorSupport = forceNoColor
                ? new ColorSupport(false)
                : new ColorSupport();

            var color = new AnsiColor(colorSupport);
            var options = new GameOptions { ColorEnabled = colorSupport.IsColorEnabled };

            IRenderer renderer = new ConsoleRenderer(color);
            IInputReader input = new ConsoleInputReader();

            // Restore the terminal (reset colors, show cursor) on Ctrl+C so we do
            // not leave color attributes leaking into the shell.
            Console.CancelKeyPress += (sender, e) =>
            {
                if (colorSupport.IsColorEnabled)
                {
                    Console.Write(AnsiColor.Reset + "\u001b[?25h");
                }
                Console.WriteLine();
                Console.WriteLine("Saindo...");
            };

            try
            {
                var controller = new GameController(renderer, input, options);
                controller.Run();
            }
            finally
            {
                if (colorSupport.IsColorEnabled)
                {
                    // Ensure the terminal is left in a clean state on normal exit.
                    Console.Write(AnsiColor.Reset);
                }
            }
        }

        private static bool HasFlag(string[] args, string flag)
        {
            if (args == null)
            {
                return false;
            }
            foreach (string a in args)
            {
                if (string.Equals(a, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
