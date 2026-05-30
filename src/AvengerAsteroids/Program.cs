using System;

namespace Asteroids
{
    /// <summary>Entry point. Replaces the original applet bootstrap.</summary>
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            string capturePath = null;
            int frames = 2;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--capture": capturePath = args[++i]; break;
                    case "--frames": frames = int.Parse(args[++i]); break;
                }
            }

            using var window = new AsteroidsWindow(capturePath, frames);
            window.Run();
            return 0;
        }
    }
}
