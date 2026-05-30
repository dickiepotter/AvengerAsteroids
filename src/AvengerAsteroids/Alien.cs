using System;

namespace Asteroids
{
    /// <summary>The enemy ship: drifts, and periodically fires at the player.
    /// Port of the GL4Java <c>Alien</c>.</summary>
    public class Alien : Sprite
    {
        public Bullets shots;
        private int fireDelay = 25;
        public bool spent = false;

        public Alien(Game caller) : base(caller)
        {
            dimensions = new[] { new[] { -6f, 6f }, new[] { 6f, 6f }, new[] { 6f, -6f }, new[] { -6f, -6f } };
            collisionRadius = 6;
            maxBullets = 1;
            randomPosition();
            acceleration = 2;
            textureNumber = 5;

            heading = (int)(caller.Rng.NextDouble() * 360);
            double rHeading = heading * (Math.PI / 180);
            velocityY += acceleration * Math.Cos(-rHeading);
            velocityX += acceleration * Math.Sin(-rHeading);

            shots = new Bullets(caller, this, true);
        }

        public override void collisionEffect(object collidedWith)
        {
            if (collidedWith is Ship || collidedWith is Bullet)
            {
                if (collidedWith is Bullet shot && shot.baddieBullet) return;
                drawExplode();
                caller.level.score();
                caller.level.score();
                spent = true;
            }
            else
            {
                if (heading < 180) heading += 150;
                else heading -= 150;
            }
        }

        public override void draw()
        {
            base.draw();
            if (fireDelay < 0)
            {
                shots.createBullet();
                fireDelay = 20;
            }
            fireDelay--;
        }
    }
}
