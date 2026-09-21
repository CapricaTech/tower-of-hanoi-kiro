namespace TowerOfHanoi.Application
{
    /// <summary>Runtime options resolved at startup.</summary>
    public sealed class GameOptions
    {
        /// <summary>Whether ANSI color output is enabled.</summary>
        public bool ColorEnabled { get; set; }

        /// <summary>Delay in milliseconds between moves during auto execution.</summary>
        public int AutoStepDelayMs { get; set; }

        public GameOptions()
        {
            ColorEnabled = true;
            AutoStepDelayMs = 500;
        }
    }
}
