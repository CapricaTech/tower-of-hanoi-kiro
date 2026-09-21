using System.Collections.Generic;

namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// Generates the optimal solution to the Tower of Hanoi puzzle using the
    /// classic recursive algorithm. Pure and deterministic, with no console IO.
    /// The sequence contains exactly 2^n - 1 moves.
    /// </summary>
    public static class HanoiSolver
    {
        /// <summary>
        /// Yields the sequence of moves that transfers <paramref name="discCount"/>
        /// discs from <paramref name="from"/> to <paramref name="to"/> using
        /// <paramref name="aux"/> as the auxiliary peg.
        /// </summary>
        public static IEnumerable<Move> Solve(int discCount, PegId from, PegId to, PegId aux)
        {
            if (discCount <= 0)
            {
                yield break;
            }

            // Move the top n-1 discs from source to auxiliary.
            foreach (Move m in Solve(discCount - 1, from, aux, to))
            {
                yield return m;
            }

            // Move the largest remaining disc from source to destination.
            yield return new Move(from, to);

            // Move the n-1 discs from auxiliary to destination.
            foreach (Move m in Solve(discCount - 1, aux, to, from))
            {
                yield return m;
            }
        }
    }
}
