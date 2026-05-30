using System;
using OpenTK.Graphics.OpenGL;
using Asteroids.Core;

namespace Asteroids
{
    /// <summary>
    /// Holds the game world and runs one frame of update+draw. This is the state
    /// half of the original <c>Application.renderCanvas</c> (the windowing half
    /// becomes <see cref="AsteroidsWindow"/>).
    /// </summary>
    public class Game
    {
        public int[] texture = new int[7];
        public float wLeft = -100f, wRight = 100f, wBottom = -100f, wTop = 100f, wNear = 1f, wFar = -1f;

        public Ship goodie;
        public Alien baddie;
        public Hud hud;
        public Level level;
        public Control controler;
        public Asteroids rocks;
        public bool debugGoodie;

        public readonly Random Rng = new Random();

        public Game()
        {
            level = new Level(this);
            controler = new Control(this);
            goodie = new Ship(this);
            setup();
        }

        public void LoadTextures()
        {
            texture[0] = TextureLoader.Load("backdrop-Sun.png");
            texture[1] = TextureLoader.Load("backdrop-Galaxy.png");
            texture[2] = TextureLoader.Load("backdrop-Moon.png");
            texture[3] = TextureLoader.Load("meteor.png");
            texture[4] = TextureLoader.Load("ship.png");
            texture[5] = TextureLoader.Load("alien.png");
            texture[6] = TextureLoader.Load("flame.png");
        }

        public void setup()
        {
            goodie.reset();
            hud = new Hud(this);
            rocks = new Asteroids(this);
            baddie = new Alien(this);
            rocks.createAsteroids();
        }

        public void clearup()
        {
            controler = null;
            goodie = null;
            rocks = null;
            baddie = null;
        }

        public void RenderBackgroundTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, texture[level.backgroundNumber]);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(wLeft, wBottom, -1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(wRight, wBottom, -1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(wRight, wTop, -1f);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(wLeft, wTop, -1f);
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        /// <summary>Port of the original <c>display()</c>: update + draw one frame.</summary>
        public void Display()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Color3(1.0f, 1.0f, 1.0f);

            RenderBackgroundTexture();

            if (level.currentLevel >= 0)
            {
                if (debugGoodie) hud.debugDetails(goodie);
                else hud.playDetails();

                rocks.draw();

                if (baddie != null)
                {
                    baddie.collisionDetection(goodie);
                    if (baddie.spent) baddie = null;
                }
                if (baddie != null)
                {
                    baddie.draw();
                    baddie.shots.draw();
                    baddie.shots.collisionDetection();
                }

                goodie.draw();
                goodie.shots.draw();

                rocks.collisionDetection();
                goodie.shots.collisionDetection();

                level.levelChange();
                level.gameOver();
            }
            else
            {
                hud.gameOver();
            }
        }
    }
}
