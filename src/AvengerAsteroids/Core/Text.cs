using OpenTK.Graphics.OpenGL;

namespace Asteroids.Core
{
    /// <summary>
    /// Draws text from the <see cref="Font8x8"/> bitmap as small filled quads in
    /// world coordinates. Replaces GL4Java's <c>glutBitmapString</c> (the HUD's
    /// text), which OpenTK has no equivalent for.
    /// </summary>
    public static class Text
    {
        /// <summary>Draw <paramref name="s"/> with its bottom-left near (x, y),
        /// extending up and to the right. <paramref name="pixel"/> is the world size
        /// of one font pixel; a glyph is 8 pixels tall.</summary>
        public static void Draw(string s, double x, double y, double pixel, float r, float g, float b)
        {
            GL.Color3(r, g, b);
            GL.Begin(PrimitiveType.Quads);

            double penX = x;
            foreach (char ch in s)
            {
                int code = ch < 128 ? ch : '?';
                byte[] glyph = Font8x8.Glyphs[code];

                for (int row = 0; row < 8; row++)
                {
                    int bits = glyph[row];
                    if (bits == 0) continue;
                    // row 0 is the top of the glyph; world y grows upward.
                    double py = y + (7 - row) * pixel;
                    for (int col = 0; col < 8; col++)
                    {
                        if ((bits & (1 << col)) == 0) continue;
                        double px = penX + col * pixel;
                        GL.Vertex2(px, py);
                        GL.Vertex2(px + pixel, py);
                        GL.Vertex2(px + pixel, py + pixel);
                        GL.Vertex2(px, py + pixel);
                    }
                }

                penX += 8 * pixel; // monospace advance
            }

            GL.End();
        }
    }
}
