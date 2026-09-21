namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Visual style used to draw the board. Purely a presentation concern; it
    /// does not affect the game rules or state.
    /// </summary>
    public enum RenderStyle
    {
        /// <summary>Discs drawn with '=' (the classic ASCII look).</summary>
        Ascii = 0,

        /// <summary>Discs drawn as solid Unicode block characters.</summary>
        Blocks = 1
    }
}
