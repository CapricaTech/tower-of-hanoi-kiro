using TowerOfHanoi.Application;

namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Abstraction over game input so the application layer never reads the
    /// console directly.
    /// </summary>
    public interface IInputReader
    {
        RenderStyle ReadRenderStyle();
        int ReadDiscCount();
        GameMode ReadGameMode();
        PlayerCommand ReadCommand();
    }
}
