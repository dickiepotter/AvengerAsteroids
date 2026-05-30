using System;
using OpenTK.Graphics.OpenGL;

namespace Asteroids
{
    /// <summary>A projectile fired by the ship or the alien. Port of the GL4Java
    /// <c>Bullet</c>.</summary>
    public class Bullet : Sprite
    {
        private readonly double maxTravelDistance;
        protected double distance;
        public bool spent = false;
        public bool baddieBullet = false;

        public Bullet(Game caller, bool bad, Sprite actualCaller) : base(caller)
        {
            dimensions = new[] { new[] { -1f, 2f }, new[] { 1f, 2f }, new[] { 1f, -1f }, new[] { -1f, -1f } };
            baddieBullet = bad;
            acceleration = 6;
            maxTravelDistance = caller.wRight * 1.2;

            if (baddieBullet)
            {
                double distX = caller.baddie.positionX - caller.goodie.positionX;
                double distY = caller.baddie.positionY - caller.goodie.positionY;
                double hyp = Math.Sqrt(distX * distX + distY * distY);
                distX /= hyp;
                distY /= hyp;
                velocityX += acceleration * -distX;
                velocityY += acceleration * -distY;
            }
            else
            {
                heading = actualCaller.heading;
                double rHeading = heading * (Math.PI / 180);
                velocityY += acceleration * Math.Cos(-rHeading);
                velocityX += acceleration * Math.Sin(-rHeading);
            }

            positionX = actualCaller.positionX;
            positionY = actualCaller.positionY;
        }

        public void outOfRange()
        {
            distance += acceleration;
            if (distance > maxTravelDistance) spent = true;
        }

        protected override void drawShape()
        {
            GL.Begin(PrimitiveType.Polygon);
            GL.Color3(0.6f, 0f, 0f); GL.Vertex2(dimensions[0][0], dimensions[0][1]);
            GL.Color3(0.6f, 0f, 0f); GL.Vertex2(dimensions[1][0], dimensions[1][1]);
            GL.Color3(0.9f, 0.9f, 0f); GL.Vertex2(dimensions[2][0], dimensions[2][1]);
            GL.Color3(0.9f, 0.9f, 0f); GL.Vertex2(dimensions[3][0], dimensions[3][1]);
            GL.End();
        }

        public override void collisionEffect(object collidedWith)
        {
            if (collidedWith is Ship && !baddieBullet) return;   // our own bullet hitting us: ignore
            if (collidedWith is Alien && baddieBullet) return;   // alien's bullet hitting alien: ignore
            spent = true;
        }
    }
}
