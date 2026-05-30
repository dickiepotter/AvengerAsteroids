using OpenTK.Graphics.OpenGL;
using Asteroids.Core;

namespace Asteroids
{
    /// <summary>
    /// The heads-up display (score, level, shields, end-game text). Port of the
    /// GL4Java <c>Hud</c>; its <c>glutBitmapString</c> text is drawn with the
    /// <see cref="Text"/> bitmap-font renderer. "big"/"small" select two sizes,
    /// approximating the original Helvetica/Times font choices.
    /// </summary>
    public class Hud
    {
        private readonly Game caller;

        public Hud(Game caller) => this.caller = caller;

        public void playDetails()
        {
            renderString("Level " + caller.level.currentLevel, true, caller.wLeft, caller.wTop - 10);
            renderString("Score: " + caller.level.totalScore, true, caller.wLeft, caller.wTop - 5);
            renderString("Shields   |", true, caller.wRight - 122, caller.wTop - 5);
            drawShield();
            renderString("Emergency brakes " + caller.goodie.emergencyBrakes, true, caller.wLeft, caller.wBottom);
            renderString("RP ", false, caller.wRight - 5, caller.wBottom);
        }

        public void gameOver()
        {
            renderString("You scored    " + caller.level.totalScore, false, caller.wLeft + 15, caller.wTop - 30, 1f, 1f, 1f);
            renderString("GAME OVER", true, caller.wLeft + 15, caller.wTop - 60, 0.8f, 0.7f, 0.1f);
            renderString("Created by Richard Potter", true, caller.wLeft + 15, caller.wBottom + 60, 1f, 0.5f, 0.5f);
            renderString("With thanks to NASA for many of the pictures", false, caller.wLeft + 15, caller.wBottom, 0.5f, 0.5f, 0.5f);
        }

        protected void drawShield()
        {
            float[] color = caller.goodie.shieldColor();
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(color[0], color[1], color[2]); GL.Vertex2(caller.wRight - caller.goodie.shields, caller.wTop);
            GL.Color3(1f, 1f, 1f); GL.Vertex2(caller.wRight, caller.wTop);
            GL.Color3(1f, 1f, 1f); GL.Vertex2(caller.wRight, caller.wTop - 6);
            GL.Color3(color[0], color[1], color[2]); GL.Vertex2(caller.wRight - caller.goodie.shields, caller.wTop - 6);
            GL.End();
        }

        // Two-size white text (originally Helvetica 18 / Times 10).
        public void renderString(string s, bool big, float x, float y)
            => Text.Draw(s, x, y, big ? 0.55 : 0.32, 1f, 1f, 1f);

        // Two-size coloured text (originally Times 24 / Helvetica 18).
        public void renderString(string s, bool big, float x, float y, float r, float g, float b)
            => Text.Draw(s, x, y, big ? 0.7 : 0.5, r, g, b);

        public void debugDetails(Sprite toDebug)
        {
            renderString("Position (" + (int)toDebug.positionX + " , " + (int)toDebug.positionY + ")", false, caller.wLeft, caller.wBottom);
            renderString("Velocity (" + (int)toDebug.velocityX + " , " + (int)toDebug.velocityY + ")", false, caller.wLeft, caller.wBottom + 4);
            renderString("Heading " + toDebug.heading + " deg", false, caller.wLeft, caller.wBottom + 8);
            renderString("manoeuvrability " + toDebug.manoeuvrability, false, caller.wLeft, caller.wBottom + 12);

            if (toDebug is Ship shipToDebug)
            {
                renderString("Acceleration " + shipToDebug.acceleration, false, caller.wLeft, caller.wBottom + 16);
                renderString("Maximum speed " + Ship.MAX_SPEED, false, caller.wLeft, caller.wBottom + 20);
            }
        }
    }
}
