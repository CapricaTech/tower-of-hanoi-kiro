namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// The reason a move was rejected. <see cref="None"/> means the move succeeded.
    /// User-facing messages are mapped in the presentation layer, not here.
    /// </summary>
    public enum MoveError
    {
        None = 0,
        SourceEmpty,
        LargerOnSmaller,
        SameSourceAndTarget,
        InvalidPeg
    }
}
