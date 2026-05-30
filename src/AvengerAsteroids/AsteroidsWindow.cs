using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Asteroids.Core;

namespace Asteroids
{
    /// <summary>
    /// The window and render loop. Replaces the original <c>Application</c> applet
    /// + GL4Java <c>GLAnimCanvas</c>: OpenTK's <see cref="GameWindow"/> owns the
    /// context, loop and input, and drives <see cref="Game"/> once per frame.
    /// </summary>
    public class AsteroidsWindow : GameWindow
    {
        private Game game;

        private readonly string capturePath;
        private readonly int captureAfterFrames;
        private int frame;

        public AsteroidsWindow(string capturePath = null, int captureAfterFrames = 2)
            : base(GameWindowSettings.Default, new NativeWindowSettings
            {
                ClientSize = new Vector2i(700, 700),
                Title = "Avenger Asteroids  (Up: thrust, Left/Right: turn, Space: fire, C: teleport, B: brake, F1: debug)",
                Profile = ContextProfile.Compatability, // fixed-function GL needs compatibility
                APIVersion = new Version(3, 3),
            })
        {
            this.capturePath = capturePath;
            this.captureAfterFrames = captureAfterFrames;
            UpdateFrequency = capturePath != null ? 0 : 60;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            game = new Game();
            game.LoadTextures();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(game.wLeft, game.wRight, game.wBottom, game.wTop, game.wNear, game.wFar);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (capturePath == null)
                game.controler?.Update(KeyboardState);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.IsRepeat)
                game.controler?.KeyPressed(e.Key);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            game.Display();
            SwapBuffers();

            if (capturePath != null && ++frame >= captureAfterFrames)
            {
                Capture(capturePath);
                Close();
            }
        }

        private void Capture(string path)
        {
            int w = ClientSize.X, h = ClientSize.Y;
            var pixels = new byte[w * h * 4];
            GL.ReadBuffer(ReadBufferMode.Front);
            GL.ReadPixels(0, 0, w, h, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            var flipped = new byte[pixels.Length];
            int stride = w * 4;
            for (int y = 0; y < h; y++)
                Array.Copy(pixels, y * stride, flipped, (h - 1 - y) * stride, stride);
            for (int i = 3; i < flipped.Length; i += 4) flipped[i] = 255;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            Png.Save(path, w, h, flipped);
            Console.WriteLine($"Captured {w}x{h} -> {path}");
        }
    }
}
