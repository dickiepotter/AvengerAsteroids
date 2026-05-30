namespace Asteroids
{
    /// <summary>Stores and processes a ship's or alien's bullets. Port of the
    /// GL4Java <c>Bullets</c>.</summary>
    public class Bullets
    {
        public Bullet[] bulletCollection;
        public bool baddieBullet = false;
        private readonly Game caller;
        private readonly Sprite actualCaller;

        public Bullets(Game caller, Sprite owner, bool bad)
        {
            this.caller = caller;
            this.actualCaller = owner;
            this.baddieBullet = bad;
            bulletCollection = new Bullet[owner.maxBullets];
        }

        public void createBullet()
        {
            for (int i = 0; i < bulletCollection.Length; i++)
            {
                if (bulletCollection[i] == null)
                {
                    bulletCollection[i] = new Bullet(caller, baddieBullet, actualCaller);
                    return;
                }
            }
        }

        public void checkSpent()
        {
            for (int i = 0; i < bulletCollection.Length; i++)
            {
                if (bulletCollection[i] != null)
                {
                    bulletCollection[i].outOfRange();
                    if (bulletCollection[i].spent) bulletCollection[i] = null;
                }
            }
        }

        public void draw()
        {
            checkSpent();
            for (int i = 0; i < bulletCollection.Length; i++)
                if (bulletCollection[i] != null)
                    bulletCollection[i].draw();
        }

        public void collisionDetection()
        {
            for (int i = 0; i < bulletCollection.Length; i++)
            {
                if (bulletCollection[i] == null) continue;
                Bullet shot = bulletCollection[i];
                shot.collisionDetection(caller.goodie);
                if (caller.baddie != null) shot.collisionDetection(caller.baddie);

                for (int j = 0; j < caller.rocks.asteroidCollection.Length; j++)
                    if (caller.rocks.asteroidCollection[j] != null)
                        shot.collisionDetection(caller.rocks.asteroidCollection[j]);
            }
        }
    }
}
