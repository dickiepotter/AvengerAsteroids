using System;
using OpenTK.Graphics.OpenGL;

namespace Asteroids
{
    /// <summary>
    /// Base class for every moving object: a textured quad with position,
    /// velocity, heading and circle-radius collision. Port of the GL4Java
    /// <c>Sprite</c> (the GL-function handle is dropped — OpenTK's GL is static).
    /// </summary>
    public class Sprite
    {
        public int manoeuvrability = 10;
        public double acceleration = 0.2;
        protected int textureNumber = -1;
        public float scale = 1;

        // Corner positions {Left,Top}{Right,Top}{Right,Bottom}{Left,Bottom}.
        // Unlike the original (which shadowed this field per subclass, so the base
        // draw always used +/-8), each sprite's own size is used here.
        public float[][] dimensions =
        {
            new[] { -8f, 8f }, new[] { 8f, 8f }, new[] { 8f, -8f }, new[] { -8f, -8f },
        };

        public int heading;
        public double positionX, positionY;
        public double velocityX, velocityY;

        protected Game caller;

        public int maxBullets;
        public double collisionRadius = 8;

        public Sprite(Game caller)
        {
            this.caller = caller;
            maxBullets = 1;
            reset();
        }

        public void renderTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, caller.texture[textureNumber]);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Texture2D);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0f, 0f); GL.Vertex3(dimensions[3][0], dimensions[3][1], -1f);
            GL.TexCoord2(1f, 0f); GL.Vertex3(dimensions[2][0], dimensions[2][1], -1f);
            GL.TexCoord2(1f, 1f); GL.Vertex3(dimensions[1][0], dimensions[1][1], -1f);
            GL.TexCoord2(0f, 1f); GL.Vertex3(dimensions[0][0], dimensions[0][1], -1f);

            GL.End();
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.Blend);
        }

        public virtual void draw()
        {
            GL.PushMatrix();

            positionX += velocityX;
            positionY += velocityY;
            wrapAround();

            GL.Scale(scale, scale, scale);
            GL.Translate(positionX, positionY, 0);
            GL.Rotate(heading, 0, 0, 1);

            drawShape();
            if (textureNumber >= 0)
            {
                GL.Color3(1.0f, 1.0f, 1.0f);
                renderTexture();
            }

            GL.PopMatrix();
        }

        protected virtual void drawShape()
        {
            // Faithful to the original: a black polygon under additive blend, which
            // contributes nothing — it is effectively invisible (the texture is the
            // visible part). Bullets override this with a coloured shape.
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcColor, BlendingFactor.One);
            GL.Begin(PrimitiveType.Polygon);
            for (int i = 0; i < 4; i++)
            {
                GL.Color3(0f, 0f, 0f);
                GL.Vertex2(dimensions[i][0], dimensions[i][1]);
            }
            GL.End();
            GL.Disable(EnableCap.Blend);
        }

        protected void wrapAround()
        {
            if (positionX < caller.wLeft) positionX = caller.wRight;
            if (positionX > caller.wRight) positionX = caller.wLeft;
            if (positionY < caller.wBottom) positionY = caller.wTop;
            if (positionY > caller.wTop) positionY = caller.wBottom;
        }

        public void randomPosition()
        {
            positionX = caller.Rng.NextDouble() * 200;
            if (positionX > 100) positionX = caller.Rng.NextDouble() * -100;
            positionY = caller.Rng.NextDouble() * 200;
            if (positionY > 100) positionY = caller.Rng.NextDouble() * -100;
        }

        public void collisionDetection(Sprite other)
        {
            double dx = positionX - other.positionX;
            double dy = positionY - other.positionY;
            double hyp = Math.Sqrt(dx * dx + dy * dy);

            if (hyp < (collisionRadius + other.collisionRadius))
            {
                collisionEffect(other);
                other.collisionEffect(this);
            }
        }

        public virtual void collisionEffect(object collidedWith)
        {
            drawExplode();
        }

        public void drawExplode()
        {
            textureNumber = 6;
            this.draw();
        }

        public void reset()
        {
            heading = 0;
            positionX = 0;
            positionY = 0;
            velocityX = 0;
            velocityY = 0;
        }
    }
}
