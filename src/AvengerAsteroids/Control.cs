using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Asteroids
{
    /// <summary>
    /// Maps keyboard input to ship actions. Port of the GL4Java <c>Control</c>,
    /// adapted from <c>KeyListener</c> to per-frame polling (held keys for
    /// thrust/turn) plus single presses (fire / teleport / brake / debug).
    ///   Up        thrust          Left / Right  turn
    ///   Space     fire            C             teleport
    ///   B         emergency stop  F1            toggle debug info
    /// </summary>
    public class Control
    {
        private readonly Game caller;

        public Control(Game caller) => this.caller = caller;

        public void Update(KeyboardState keyboard)
        {
            if (caller.goodie == null) return;
            if (keyboard.IsKeyDown(Keys.Up)) caller.goodie.thrust();
            if (keyboard.IsKeyDown(Keys.Left)) caller.goodie.rotate(false);
            if (keyboard.IsKeyDown(Keys.Right)) caller.goodie.rotate(true);
        }

        public void KeyPressed(Keys key)
        {
            if (caller.goodie == null) return;
            switch (key)
            {
                case Keys.C: caller.goodie.randomPosition(); break; // teleport
                case Keys.Space: caller.goodie.fire(); break;
                case Keys.B: caller.goodie.emergencyStop(); break;
                case Keys.F1: caller.debugGoodie = !caller.debugGoodie; break;
            }
        }
    }
}
