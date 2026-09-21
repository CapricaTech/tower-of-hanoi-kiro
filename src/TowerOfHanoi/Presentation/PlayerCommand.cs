using TowerOfHanoi.Domain;

namespace TowerOfHanoi.Presentation
{
    public enum CommandType
    {
        Move,
        Quit,
        Restart,
        Help,
        Invalid
    }

    /// <summary>
    /// The parsed result of a player's turn input: either a move or a control
    /// command, or an indication that the input was invalid.
    /// </summary>
    public struct PlayerCommand
    {
        public CommandType Type { get; }
        public Move Move { get; }

        private PlayerCommand(CommandType type, Move move)
        {
            Type = type;
            Move = move;
        }

        public static PlayerCommand ForMove(Move move)
        {
            return new PlayerCommand(CommandType.Move, move);
        }

        public static PlayerCommand Simple(CommandType type)
        {
            return new PlayerCommand(type, default(Move));
        }
    }
}
