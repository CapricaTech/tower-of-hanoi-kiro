using System.Collections.Generic;
using System.Linq;

namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// A single peg holding a stack of discs. Discs are represented by their size
    /// (1 = smallest). The invariant is that the top disc is always the smallest
    /// on the peg, i.e. sizes strictly decrease from bottom to top.
    /// </summary>
    public sealed class Peg
    {
        private readonly Stack<int> _discs = new Stack<int>();

        public PegId Id { get; }

        public Peg(PegId id)
        {
            Id = id;
        }

        public bool IsEmpty
        {
            get { return _discs.Count == 0; }
        }

        public int Count
        {
            get { return _discs.Count; }
        }

        /// <summary>Size of the top disc, or null if the peg is empty.</summary>
        public int? Peek()
        {
            return IsEmpty ? (int?)null : _discs.Peek();
        }

        /// <summary>
        /// True if a disc of the given size may be placed on this peg
        /// (peg is empty or the incoming disc is smaller than the current top).
        /// </summary>
        public bool CanAccept(int discSize)
        {
            return IsEmpty || discSize < _discs.Peek();
        }

        /// <summary>Removes and returns the top disc. Caller must ensure not empty.</summary>
        public int Pop()
        {
            return _discs.Pop();
        }

        /// <summary>Pushes a disc onto the peg. Caller must have validated the move.</summary>
        public void Push(int discSize)
        {
            _discs.Push(discSize);
        }

        /// <summary>
        /// Discs from the bottom of the peg to the top, for rendering.
        /// </summary>
        public IReadOnlyList<int> DiscsBottomToTop
        {
            // Stack enumerates top-to-bottom, so reverse for bottom-to-top.
            get { return _discs.Reverse().ToList(); }
        }
    }
}
