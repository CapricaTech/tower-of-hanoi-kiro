using System;
using TowerOfHanoi.Application;
using TowerOfHanoi.Domain;

namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Reads and parses keyboard input from the console. Prompts for the disc
    /// count and game mode loop until a valid value is entered. Turn commands are
    /// parsed into a <see cref="PlayerCommand"/> for the controller to act on.
    /// </summary>
    public sealed class ConsoleInputReader : IInputReader
    {
        public int ReadDiscCount()
        {
            while (true)
            {
                Console.Write("Quantos discos? (" + Board.MinDiscs + "-" + Board.MaxDiscs + "): ");
                string line = Console.ReadLine();

                if (line == null)
                {
                    // End of input stream; default to the minimum to stay playable.
                    return Board.MinDiscs;
                }

                int value;
                if (int.TryParse(line.Trim(), out value) &&
                    value >= Board.MinDiscs && value <= Board.MaxDiscs)
                {
                    return value;
                }

                Console.WriteLine("Valor invalido. Informe um numero entre " +
                                  Board.MinDiscs + " e " + Board.MaxDiscs + ".");
            }
        }

        public GameMode ReadGameMode()
        {
            while (true)
            {
                Console.Write("Modo de jogo - [m]anual ou [a]uto execucao? (m/a): ");
                string line = Console.ReadLine();

                if (line == null)
                {
                    return GameMode.Manual;
                }

                string choice = line.Trim().ToLowerInvariant();
                switch (choice)
                {
                    case "m":
                    case "manual":
                        return GameMode.Manual;
                    case "a":
                    case "auto":
                    case "autoexecucao":
                        return GameMode.Auto;
                    default:
                        Console.WriteLine("Escolha invalida. Digite 'm' para manual ou 'a' para auto.");
                        break;
                }
            }
        }

        public PlayerCommand ReadCommand()
        {
            Console.Write("Movimento (ex.: 'A C') ou comando [h/r/q]: ");
            string line = Console.ReadLine();

            if (line == null)
            {
                return PlayerCommand.Simple(CommandType.Quit);
            }

            string input = line.Trim().ToLowerInvariant();
            if (input.Length == 0)
            {
                return PlayerCommand.Simple(CommandType.Invalid);
            }

            switch (input)
            {
                case "q":
                case "sair":
                    return PlayerCommand.Simple(CommandType.Quit);
                case "r":
                case "reiniciar":
                    return PlayerCommand.Simple(CommandType.Restart);
                case "h":
                case "ajuda":
                    return PlayerCommand.Simple(CommandType.Help);
            }

            return ParseMove(input);
        }

        /// <summary>
        /// Parses a move from pin labels. Accepts "a c", "ac", "a-c", etc.
        /// Extracts the first two peg letters (a/b/c) found in the input.
        /// </summary>
        private static PlayerCommand ParseMove(string input)
        {
            PegId? first = null;
            PegId? second = null;

            foreach (char c in input)
            {
                PegId peg;
                if (!TryParsePeg(c, out peg))
                {
                    continue;
                }

                if (first == null)
                {
                    first = peg;
                }
                else if (second == null)
                {
                    second = peg;
                    break;
                }
            }

            if (first == null || second == null)
            {
                return PlayerCommand.Simple(CommandType.Invalid);
            }

            return PlayerCommand.ForMove(new Move(first.Value, second.Value));
        }

        private static bool TryParsePeg(char c, out PegId peg)
        {
            switch (c)
            {
                case 'a': peg = PegId.A; return true;
                case 'b': peg = PegId.B; return true;
                case 'c': peg = PegId.C; return true;
                default: peg = PegId.A; return false;
            }
        }
    }
}
