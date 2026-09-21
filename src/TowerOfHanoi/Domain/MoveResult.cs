namespace TowerOfHanoi.Domain
{
    /// <summary>
    /// The outcome of a <see cref="Board.TryMove"/> attempt. Immutable.
    /// The domain never throws for an invalid player move; it returns a result.
    /// </summary>
    public struct MoveResult
    {
        public bool Success { get; }
        public MoveError Error { get; }

        private MoveResult(bool success, MoveError error)
        {
            Success = success;
            Error = error;
        }

        public static MoveResult Ok()
        {
            return new MoveResult(true, MoveError.None);
        }

        public static MoveResult Fail(MoveError error)
        {
            return new MoveResult(false, error);
        }
    }
}
