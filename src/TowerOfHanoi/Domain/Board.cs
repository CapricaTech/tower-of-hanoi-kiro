using System;

namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// The core game board and rules. Holds three pegs and the move count.
    /// All move validation lives here and is free of any console/IO concerns.
    /// </summary>
    public sealed class Board
    {
        public const int MinDiscs = 4;
        public const int MaxDiscs = 8;

        private readonly Peg[] _pegs;

        public int DiscCount { get; }
        public int MoveCount { get; private set; }

        /// <summary>Minimum number of moves to solve the puzzle: 2^n - 1.</summary>
        public int MinimumMoves
        {
            get { return (1 << DiscCount) - 1; }
        }

        /// <summary>
        /// Creates a board with all discs stacked on peg A, largest at the bottom.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="discCount"/> is outside the 4..8 range. This is a
        /// programming safeguard; user input is validated before reaching here.
        /// </exception>
        public Board(int discCount)
        {
            if (discCount < MinDiscs || discCount > MaxDiscs)
            {
                throw new ArgumentOutOfRangeException(
                    "discCount",
                    discCount,
                    "Disc count must be between " + MinDiscs + " and " + MaxDiscs + ".");
            }

            DiscCount = discCount;
            MoveCount = 0;

            _pegs = new[]
            {
                new Peg(PegId.A),
                new Peg(PegId.B),
                new Peg(PegId.C)
            };

            // Push largest (discCount) first so the smallest (1) ends on top.
            for (int size = discCount; size >= 1; size--)
            {
                _pegs[(int)PegId.A].Push(size);
            }
        }

        private Peg PegOf(PegId id)
        {
            return _pegs[(int)id];
        }

        /// <summary>
        /// Attempts a move. Returns a result describing success or the reason for
        /// rejection. On failure the board state is left unchanged and the move
        /// count is not incremented.
        /// </summary>
        public MoveResult TryMove(Move move)
        {
            if (!IsValidPeg(move.From) || !IsValidPeg(move.To))
            {
                return MoveResult.Fail(MoveError.InvalidPeg);
            }

            if (move.From == move.To)
            {
                return MoveResult.Fail(MoveError.SameSourceAndTarget);
            }

            Peg source = PegOf(move.From);
            Peg target = PegOf(move.To);

            if (source.IsEmpty)
            {
                return MoveResult.Fail(MoveError.SourceEmpty);
            }

            int disc = source.Peek().Value;
            if (!target.CanAccept(disc))
            {
                return MoveResult.Fail(MoveError.LargerOnSmaller);
            }

            target.Push(source.Pop());
            MoveCount++;
            return MoveResult.Ok();
        }

        private static bool IsValidPeg(PegId id)
        {
            return id == PegId.A || id == PegId.B || id == PegId.C;
        }

        /// <summary>
        /// True when every disc is stacked on peg C (the destination) in order.
        /// </summary>
        public bool IsSolved()
        {
            return PegOf(PegId.C).Count == DiscCount;
        }

        /// <summary>Produces an immutable snapshot for rendering.</summary>
        public GameState Snapshot()
        {
            return new GameState(
                PegOf(PegId.A).DiscsBottomToTop,
                PegOf(PegId.B).DiscsBottomToTop,
                PegOf(PegId.C).DiscsBottomToTop,
                DiscCount,
                MoveCount,
                MinimumMoves,
                IsSolved());
        }
    }
}
