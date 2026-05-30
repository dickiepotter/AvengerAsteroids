namespace Asteroids
{
    /// <summary>Stores and processes the field of asteroids (creation, splitting,
    /// drawing, collisions). Port of the GL4Java <c>Asteroids</c>.</summary>
    public class Asteroids
    {
        public Asteroid[] asteroidCollection;
        private readonly Game caller;
        public int noOfAsteroids;

        public Asteroids(Game caller)
        {
            this.caller = caller;
            noOfAsteroids = caller.level.currentLevel;
        }

        public void createAsteroids()
        {
            asteroidCollection = new Asteroid[noOfAsteroids];
            for (int i = 0; i < asteroidCollection.Length; i++)
                asteroidCollection[i] = new Asteroid(caller, 0);
        }

        public void checkSpent()
        {
            for (int i = 0; i < asteroidCollection.Length; i++)
            {
                if (asteroidCollection[i] != null && asteroidCollection[i].spent)
                {
                    if (asteroidCollection[i].subAsteroid < 2)
                        createSubAsteroids(asteroidCollection[i].positionX, asteroidCollection[i].positionY,
                            asteroidCollection[i].subAsteroid + 1);
                    asteroidCollection[i] = null;
                }
            }
        }

        public void createSubAsteroids(double x, double y, int sub)
        {
            Asteroid[] temp = asteroidCollection;
            int tempLength = asteroidCollection.Length;
            asteroidCollection = new Asteroid[tempLength + 4];

            for (int i = 0; i < tempLength; i++)
                asteroidCollection[i] = temp[i];

            for (int i = tempLength; i < asteroidCollection.Length; i++)
            {
                asteroidCollection[i] = new Asteroid(caller, sub);
                asteroidCollection[i].positionX = x;
                asteroidCollection[i].positionY = y;
            }
        }

        public void draw()
        {
            checkSpent();
            for (int i = 0; i < asteroidCollection.Length; i++)
                if (asteroidCollection[i] != null)
                    asteroidCollection[i].draw();
        }

        public void collisionDetection()
        {
            for (int i = 0; i < asteroidCollection.Length; i++)
            {
                if (asteroidCollection[i] == null) continue;
                Asteroid rock = asteroidCollection[i];
                rock.collisionDetection(caller.goodie);
                if (caller.baddie != null) rock.collisionDetection(caller.baddie);

                for (int j = 0; j < asteroidCollection.Length; j++)
                    if (j != i && asteroidCollection[j] != null)
                        rock.collisionDetection(asteroidCollection[j]);
            }
        }

        public bool empty()
        {
            for (int i = 0; i < asteroidCollection.Length; i++)
                if (asteroidCollection[i] != null) return false;
            return true;
        }
    }
}
