using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace Asteroids.Core
{
    /// <summary>
    /// Loads PNGs into GL textures (RGBA). Replaces GL4Java's
    /// <c>PngTextureLoader</c>; the sprite textures carry an alpha channel for
    /// transparency, so everything is uploaded as RGBA.
    /// </summary>
    public static class TextureLoader
    {
        private static readonly string AssetDir = Path.Combine(AppContext.BaseDirectory, "Assets");

        public static int Load(string file)
        {
            // Flip so the image is the right way up for the bottom-left = (0,0)
            // texture coordinates used throughout (otherwise sprites/backdrops
            // render upside down).
            StbImage.stbi_set_flip_vertically_on_load(1);

            using var stream = File.OpenRead(Path.Combine(AssetDir, file));
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            int tex = GL.GenTexture();
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            return tex;
        }
    }
}
