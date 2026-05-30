using System;
using OpenTK.Graphics.OpenGL;

namespace Asteroids
{
    /// <summary>The player's ship. Port of the GL4Java <c>Ship</c>.</summary>
    public class Ship : Sprite
    {
        public const int MAX_SPEED = 5;
        public int emergencyBrakes = 2;
        private bool showThrust = false;

        public bool xHeadingSwitch = false;
        public bool yHeadingSwitch = false;

        public Bullets shots;
        public int shields = 100;

        public Ship(Game caller) : base(caller)
        {
            dimensions = new[] { new[] { -6f, 6f }, new[] { 6f, 6f }, new[] { 6f, -6f }, new[] { -6f, -6f } };
            maxBullets = 4;
            shots = new Bullets(caller, this, false);
            textureNumber = 4;
        }

        public void rotate(bool rleft)
        {
            double tempHeading = heading;
            if (rleft) heading -= manoeuvrability;
            else heading += manoeuvrability;
            if (heading > 360) heading = 0;
            if (heading < 0) heading = 360;

            // Track crossing the X axis (used so thrust keeps working through turns).
            if (((tempHeading >= 90 && tempHeading < 270) && (heading >= 270 && heading < 360 || heading < 90 && heading >= 0)) ||
                ((heading >= 90 && heading < 270) && (tempHeading >= 270 && tempHeading < 360 || tempHeading < 90 && tempHeading >= 0)))
                xHeadingSwitch = true;

            // Track crossing the Y axis.
            if (((tempHeading >= 0 && tempHeading < 180) && (heading >= 180 && heading < 360)) ||
                ((heading >= 0 && heading < 180) && (tempHeading >= 180 && tempHeading < 360)))
                yHeadingSwitch = true;
        }

        public void emergencyStop()
        {
            if (emergencyBrakes > 0 && (velocityX != 0 || velocityY != 0))
            {
                velocityX = 0;
                velocityY = 0;
                emergencyBrakes--;
            }
        }

        public void fire() => shots.createBullet();

        public void thrust()
        {
            double rHeading = heading * (Math.PI / 180);

            if (velocityX < MAX_SPEED && velocityX > -MAX_SPEED || xHeadingSwitch)
            {
                velocityX += acceleration * Math.Sin(-rHeading);
                showThrust = true;
                if (velocityX < MAX_SPEED && velocityX > -MAX_SPEED) xHeadingSwitch = false;
            }

            if (velocityY < MAX_SPEED && velocityY > -MAX_SPEED || yHeadingSwitch)
            {
                velocityY += acceleration * Math.Cos(-rHeading);
                showThrust = true;
                if (velocityY < MAX_SPEED && velocityY > -MAX_SPEED) yHeadingSwitch = false;
            }
        }

        public override void collisionEffect(object collidedWith)
        {
            // A collision with our own bullet does nothing.
            if (collidedWith is Bullet shot && !shot.baddieBullet) return;
            shields -= 5;
            drawShields();
        }

        public bool destroyed()
        {
            if (shields <= 0)
            {
                velocityX = 0;
                velocityY = 0;
                acceleration = 0;
                drawExplode();
                return true;
            }
            return false;
        }

        public float[] shieldColor()
        {
            float[] color = { 0, 0, 0 };
            if (caller.goodie.shields <= 30) color[0] = 1;      // red
            else if (caller.goodie.shields <= 60) color[2] = 1; // blue
            else color[1] = 1;                                  // green
            return color;
        }

        public void drawShields()
        {
            float[] color = shieldColor();
            float width = shields / 10f;
            if (width > 6) width = 6;

            GL.LineWidth(width); // (the original called this inside glBegin, where it is ignored)
            GL.Begin(PrimitiveType.LineStrip);
            GL.Color3(color[0], color[1], color[2]);
            for (double theta = 0; theta <= 2 * Math.PI; theta += Math.PI / 180)
            {
                double y = Math.Sin(theta) * collisionRadius;
                double x = Math.Cos(theta) * collisionRadius;
                GL.Vertex2(x + positionX, y + positionY);
            }
            GL.End();
        }

        public void drawThrust()
        {
            GL.PushMatrix();
            GL.Translate(positionX, positionY, 0);
            GL.Rotate(heading, 0, 0, 1);

            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(1.0f, 1.0f, 0.0f); GL.Vertex2(0, dimensions[2][1] * 2);            // point
            GL.Color3(0.8f, 0.0f, 0.0f); GL.Vertex2(dimensions[2][0] / 3, dimensions[2][1]); // right
            GL.Color3(0.8f, 0.0f, 0.0f); GL.Vertex2(dimensions[3][0] / 3, dimensions[3][1]); // left
            GL.End();

            GL.PopMatrix();
            showThrust = false;
        }

        public override void draw()
        {
            base.draw();
            if (showThrust) drawThrust();
        }
    }
}
