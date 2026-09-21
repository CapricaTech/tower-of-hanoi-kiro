using System;
using System.Collections.Generic;
using System.Text;
using TowerOfHanoi.Domain;

namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Renders the game to the console using ASCII art and optional ANSI color.
    /// The disc for size s is drawn as a block of width (2*s + 1), centered over
    /// its peg. Empty peg positions show only the vertical mast.
    /// </summary>
    public sealed class ConsoleRenderer : IRenderer
    {
        private const char DiscChar = '=';
        private const char MastChar = '|';
        private const int PegGap = 3; // spaces between pegs

        private readonly AnsiColor _color;

        public ConsoleRenderer(AnsiColor color)
        {
            _color = color;
        }

        public void RenderWelcome()
        {
            Console.WriteLine();
            Console.WriteLine(_color.Wrap("=== Torre de Hanoi ===", AnsiColor.Bold));
            Console.WriteLine("Bem-vindo! Mova todos os discos do pino A para o pino C.");
            Console.WriteLine();
        }

        public void RenderRules()
        {
            Console.WriteLine("Regras:");
            Console.WriteLine("  - Mova um disco por vez (sempre o do topo de um pino).");
            Console.WriteLine("  - Um disco maior nunca pode ficar sobre um disco menor.");
            Console.WriteLine("  - Objetivo: transferir toda a pilha para o pino C.");
            Console.WriteLine();
            Console.WriteLine("Comandos:");
            Console.WriteLine("  - Movimento: informe origem e destino, ex.: 'A C' ou 'ac'.");
            Console.WriteLine("  - 'h' ou 'ajuda'    : mostrar esta ajuda.");
            Console.WriteLine("  - 'r' ou 'reiniciar': recomecar a partida.");
            Console.WriteLine("  - 'q' ou 'sair'     : sair do jogo.");
            Console.WriteLine();
        }

        public void RenderBoard(GameState state)
        {
            int n = state.DiscCount;
            int colWidth = 2 * n + 1;      // width to hold the largest disc
            int height = n;                 // one line per possible disc level
            string gap = new string(' ', PegGap);

            var sb = new StringBuilder();
            sb.AppendLine();

            // Draw from the top level down to the bottom level.
            for (int level = height - 1; level >= 0; level--)
            {
                sb.Append(RenderLevel(state.PegA, level, colWidth));
                sb.Append(gap);
                sb.Append(RenderLevel(state.PegB, level, colWidth));
                sb.Append(gap);
                sb.Append(RenderLevel(state.PegC, level, colWidth));
                sb.AppendLine();
            }

            // Base line under all three pegs.
            string baseSegment = new string('-', colWidth);
            sb.Append(baseSegment).Append(gap).Append(baseSegment).Append(gap).Append(baseSegment);
            sb.AppendLine();

            // Peg labels centered under each column.
            sb.Append(Center("A", colWidth)).Append(gap)
              .Append(Center("B", colWidth)).Append(gap)
              .Append(Center("C", colWidth));
            sb.AppendLine();
            sb.AppendLine();

            // Status line.
            sb.Append("Movimentos: ")
              .Append(state.MoveCount)
              .Append("   Minimo: ")
              .Append(state.MinimumMoves);

            Console.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Renders one horizontal slice of a single peg at the given level.
        /// If a disc occupies that level, draws the colored block; otherwise the mast.
        /// </summary>
        private string RenderLevel(IReadOnlyList<int> discsBottomToTop, int level, int colWidth)
        {
            if (level < discsBottomToTop.Count)
            {
                int size = discsBottomToTop[level];
                int blockWidth = 2 * size + 1;
                int pad = (colWidth - blockWidth) / 2;
                string block = new string(DiscChar, blockWidth);
                string colored = _color.ColorDisc(block, size);
                return new string(' ', pad) + colored + new string(' ', colWidth - blockWidth - pad);
            }

            // Empty at this level: show the mast centered.
            int mastPad = colWidth / 2;
            return new string(' ', mastPad) + MastChar + new string(' ', colWidth - mastPad - 1);
        }

        private static string Center(string text, int width)
        {
            if (text.Length >= width)
            {
                return text;
            }
            int pad = (width - text.Length) / 2;
            return new string(' ', pad) + text + new string(' ', width - text.Length - pad);
        }

        public void RenderMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void RenderError(string message)
        {
            Console.WriteLine(_color.Wrap("Erro: " + message, "\u001b[91m"));
        }

        public void RenderVictory(GameState state, bool optimal)
        {
            Console.WriteLine();
            Console.WriteLine(_color.Wrap("*** Parabens! Voce resolveu a Torre de Hanoi! ***", AnsiColor.Bold));
            Console.WriteLine("Movimentos usados: " + state.MoveCount +
                              "   Minimo possivel: " + state.MinimumMoves);
            if (optimal)
            {
                Console.WriteLine(_color.Wrap("Solucao otima! Voce usou o numero minimo de movimentos.", "\u001b[92m"));
            }
            Console.WriteLine();
        }

        public void ClearScreen()
        {
            if (_color.Enabled)
            {
                // ANSI clear screen + cursor home. Only when we know we have a
                // capable terminal, to avoid emitting raw control chars.
                Console.Write("\u001b[2J\u001b[H");
            }
            else
            {
                Console.WriteLine();
            }
        }
    }
}
