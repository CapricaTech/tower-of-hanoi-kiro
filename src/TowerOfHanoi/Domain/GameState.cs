using System.Collections.Generic;

namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// An immutable snapshot of the board for the presentation layer.
    /// Disc lists are ordered bottom-to-top.
    /// </summary>
    public sealed class GameState
    {
        public IReadOnlyList<int> PegA { get; }
        public IReadOnlyList<int> PegB { get; }
        public IReadOnlyList<int> PegC { get; }
        public int DiscCount { get; }
        public int MoveCount { get; }
        public int MinimumMoves { get; }
        public bool IsSolved { get; }

        public GameState(
            IReadOnlyList<int> pegA,
            IReadOnlyList<int> pegB,
            IReadOnlyList<int> pegC,
            int discCount,
            int moveCount,
            int minimumMoves,
            bool isSolved)
        {
            PegA = pegA;
            PegB = pegB;
            PegC = pegC;
            DiscCount = discCount;
            MoveCount = moveCount;
            MinimumMoves = minimumMoves;
            IsSolved = isSolved;
        }

        /// <summary>Returns the disc list for the given peg (bottom-to-top).</summary>
        public IReadOnlyList<int> DiscsFor(PegId peg)
        {
            switch (peg)
            {
                case PegId.A: return PegA;
                case PegId.B: return PegB;
                default: return PegC;
            }
        }
    }
}
