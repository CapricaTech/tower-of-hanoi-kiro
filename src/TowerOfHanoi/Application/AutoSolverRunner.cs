using System;
using System.Threading;
using TowerOfHanoi.Domain;
using TowerOfHanoi.Presentation;

namespace TowerOfHanoi.Application
{
    /// <summary>
    /// Runs the auto-execution mode: consumes the optimal solution from the
    /// solver and applies one move at a time, redrawing the board between steps
    /// with a configurable pause. The player can interrupt by pressing a key.
    /// </summary>
    public sealed class AutoSolverRunner
    {
        private readonly IRenderer _renderer;
        private readonly int _stepDelayMs;

        public AutoSolverRunner(IRenderer renderer, int stepDelayMs)
        {
            _renderer = renderer;
            _stepDelayMs = stepDelayMs;
        }

        /// <summary>
        /// Applies the optimal solution to the board. Returns true if the puzzle
        /// was solved to completion, false if the player interrupted it.
        /// </summary>
        public bool Run(Board board)
        {
            _renderer.ClearScreen();
            _renderer.RenderBoard(board.Snapshot());
            _renderer.RenderMessage("Auto execucao iniciada. Pressione uma tecla para interromper.");
            Pause();

            foreach (Move move in HanoiSolver.Solve(board.DiscCount, PegId.A, PegId.C, PegId.B))
            {
                if (Interrupted())
                {
                    _renderer.RenderMessage("Auto execucao interrompida.");
                    return false;
                }

                MoveResult result = board.TryMove(move);
                // The solver only produces valid moves; a failure would indicate
                // a logic error, so surface it rather than silently continuing.
                if (!result.Success)
                {
                    _renderer.RenderError("Movimento invalido durante a auto execucao: " + move);
                    return false;
                }

                _renderer.ClearScreen();
                _renderer.RenderBoard(board.Snapshot());
                Pause();
            }

            return board.IsSolved();
        }

        private void Pause()
        {
            if (_stepDelayMs > 0)
            {
                Thread.Sleep(_stepDelayMs);
            }
        }

        /// <summary>
        /// True if the user pressed a key to interrupt. Guarded because
        /// Console.KeyAvailable throws when input is redirected (non-interactive).
        /// </summary>
        private static bool Interrupted()
        {
            try
            {
                if (Console.KeyAvailable)
                {
                    // Consume the key so it does not leak into later input.
                    Console.ReadKey(true);
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                // No interactive console (e.g. redirected input); cannot interrupt.
            }
            return false;
        }
    }
}
