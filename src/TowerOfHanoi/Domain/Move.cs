namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// An immutable value describing an attempted move from one peg to another.
    /// </summary>
    public struct Move
    {
        public PegId From { get; }
        public PegId To { get; }

        public Move(PegId from, PegId to)
        {
            From = from;
            To = to;
        }

        public override string ToString()
        {
            return From + " -> " + To;
        }
    }
}
