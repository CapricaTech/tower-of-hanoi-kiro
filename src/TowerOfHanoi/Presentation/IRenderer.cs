using TowerOfHanoi.Domain;

namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Abstraction over game output so the application layer never touches the
    /// console directly. This keeps the domain/application logic portable.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>The visual style used to draw the board.</summary>
        RenderStyle Style { get; set; }

        void RenderWelcome();
        void RenderRules();
        void RenderBoard(GameState state);
        void RenderMessage(string message);
        void RenderError(string message);
        void RenderVictory(GameState state, bool optimal);
        void ClearScreen();
    }
}
