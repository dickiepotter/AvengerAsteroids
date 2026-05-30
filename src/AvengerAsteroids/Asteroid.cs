using System;

namespace Asteroids
{
    /// <summary>A spinning, drifting asteroid that splits when shot. Port of the
    /// GL4Java <c>Asteroid</c>.</summary>
    public class Asteroid : Sprite
    {
        public bool spent = false;
        public int subAsteroid = 0;

        public Asteroid(Game caller, int sub) : base(caller)
        {
            subAsteroid = sub;

            if (subAsteroid == 1) // medium
            {
                dimensions = new[] { new[] { -6f, 6f }, new[] { 6f, 6f }, new[] { 6f, -6f }, new[] { -6f, -6f } };
                collisionRadius = 6;
            }
            if (subAsteroid == 2) // small
            {
                dimensions = new[] { new[] { -4f, 4f }, new[] { 4f, 4f }, new[] { 4f, -4f }, new[] { -4f, -4f } };
                collisionRadius = 5;
            }
            acceleration = 2;
            textureNumber = 3;

            randomPosition();
            heading = (int)(caller.Rng.NextDouble() * 360);
            double rHeading = heading * (Math.PI / 180);
            velocityY += acceleration * Math.Cos(-rHeading);
            velocityX += acceleration * Math.Sin(-rHeading);
        }

        public override void draw()
        {
            base.draw();
            heading += 5; // spin
        }

        public override void collisionEffect(object collidedWith)
        {
            if (collidedWith is Ship || collidedWith is Bullet)
            {
                if (collidedWith is Bullet shot && shot.baddieBullet) return; // baddie bullet: ignore
                drawExplode();
                caller.level.score();
                spent = true;
            }
            else
            {
                if (heading < 180) heading += 150;
                else heading -= 150;
            }
        }
    }
}
