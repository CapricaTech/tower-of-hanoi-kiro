using System;

namespace TowerOfHanoi.Presentation
{
    /// <summary>
    /// Decides whether ANSI color output should be enabled, based on the
    /// environment. The decision is computed once at construction.
    /// </summary>
    public sealed class ColorSupport
    {
        public bool IsColorEnabled { get; }

        public ColorSupport()
        {
            IsColorEnabled = DetectColorSupport();
        }

        /// <summary>Allows forcing a value (e.g. for a --no-color flag or tests).</summary>
        public ColorSupport(bool forceEnabled)
        {
            IsColorEnabled = forceEnabled;
        }

        private static bool DetectColorSupport()
        {
            // If output is redirected (pipe/file), do not emit escape sequences.
            try
            {
                if (Console.IsOutputRedirected)
                {
                    return false;
                }
            }
            catch
            {
                // If we cannot determine, err on the side of no color.
                return false;
            }

            // Respect the NO_COLOR convention (https://no-color.org).
            string noColor = Environment.GetEnvironmentVariable("NO_COLOR");
            if (!string.IsNullOrEmpty(noColor))
            {
                return false;
            }

            // A "dumb" or missing TERM indicates no color capability.
            string term = Environment.GetEnvironmentVariable("TERM");
            if (string.IsNullOrEmpty(term) || term == "dumb")
            {
                return false;
            }

            return true;
        }
    }
}
