using TowerOfHanoi.Domain;
using TowerOfHanoi.Presentation;

namespace TowerOfHanoi.Application
{
    /// <summary>
    /// Drives the game life cycle: welcome, disc/mode selection, the manual or
    /// auto play loop, victory, and restart/quit. Talks only to the renderer and
    /// input reader abstractions, keeping it free of direct console access.
    /// </summary>
    public sealed class GameController
    {
        private readonly IRenderer _renderer;
        private readonly IInputReader _input;
        private readonly GameOptions _options;

        public GameController(IRenderer renderer, IInputReader input, GameOptions options)
        {
            _renderer = renderer;
            _input = input;
            _options = options;
        }

        public void Run()
        {
            _renderer.RenderWelcome();
            _renderer.RenderRules();

            bool keepPlaying = true;
            while (keepPlaying)
            {
                int discCount = _input.ReadDiscCount();
                GameMode mode = _input.ReadGameMode();
                var board = new Board(discCount);

                bool solved;
                RestartOutcome outcome;

                if (mode == GameMode.Auto)
                {
                    var runner = new AutoSolverRunner(_renderer, _options.AutoStepDelayMs);
                    solved = runner.Run(board);
                    outcome = RestartOutcome.Finished;
                }
                else
                {
                    outcome = RunManualLoop(board, out solved);
                }

                if (solved)
                {
                    _renderer.RenderBoard(board.Snapshot());
                    bool optimal = board.MoveCount == board.MinimumMoves;
                    _renderer.RenderVictory(board.Snapshot(), optimal);
                }

                if (outcome == RestartOutcome.Quit)
                {
                    keepPlaying = false;
                }
                // Restart and Finished both fall through to another round.
            }

            _renderer.RenderMessage("Ate a proxima!");
        }

        private enum RestartOutcome
        {
            Finished,
            Restart,
            Quit
        }

        /// <summary>
        /// Runs the interactive manual loop until the puzzle is solved or the
        /// player restarts/quits. Sets <paramref name="solved"/> accordingly.
        /// </summary>
        private RestartOutcome RunManualLoop(Board board, out bool solved)
        {
            solved = false;
            bool redrawBoard = true;

            while (true)
            {
                if (redrawBoard)
                {
                    _renderer.ClearScreen();
                    _renderer.RenderBoard(board.Snapshot());
                    redrawBoard = false;
                }

                if (board.IsSolved())
                {
                    solved = true;
                    return RestartOutcome.Finished;
                }

                PlayerCommand command = _input.ReadCommand();
                switch (command.Type)
                {
                    case CommandType.Quit:
                        return RestartOutcome.Quit;

                    case CommandType.Restart:
                        return RestartOutcome.Restart;

                    case CommandType.Help:
                        // Show rules without clearing; board is redrawn next turn.
                        _renderer.RenderRules();
                        redrawBoard = true;
                        break;

                    case CommandType.Move:
                        // Only redraw when the move actually changed the board.
                        redrawBoard = ApplyMove(board, command.Move);
                        break;

                    case CommandType.Invalid:
                    default:
                        // Keep the board and error visible; do not clear.
                        _renderer.RenderError("Entrada invalida. Use 'A C' para mover ou h/r/q.");
                        break;
                }
            }
        }

        /// <summary>
        /// Applies a move. Returns true if the board changed (so it should be
        /// redrawn); false if the move was rejected (error shown, board kept).
        /// </summary>
        private bool ApplyMove(Board board, Move move)
        {
            MoveResult result = board.TryMove(move);
            if (result.Success)
            {
                return true;
            }

            _renderer.RenderError(DescribeError(result.Error));
            return false;
        }

        private static string DescribeError(MoveError error)
        {
            switch (error)
            {
                case MoveError.SourceEmpty:
                    return "O pino de origem esta vazio.";
                case MoveError.LargerOnSmaller:
                    return "Nao e possivel colocar um disco maior sobre um menor.";
                case MoveError.SameSourceAndTarget:
                    return "Origem e destino sao iguais.";
                case MoveError.InvalidPeg:
                    return "Pino invalido.";
                default:
                    return "Movimento invalido.";
            }
        }
    }
}
