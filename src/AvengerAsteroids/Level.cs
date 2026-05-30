namespace Asteroids
{
    /// <summary>Level progression, score and end-game handling. Port of the
    /// GL4Java <c>Level</c>.</summary>
    public class Level
    {
        public int currentLevel; // -1 == game over
        public int backgroundNumber;
        public int totalScore;
        public int endGameDelay = 40;
        private readonly Game caller;

        public Level(Game caller)
        {
            this.caller = caller;
            currentLevel = 1;
            backgroundNumber = 0;
        }

        public void setLevel(int level)
        {
            currentLevel = level;
            backgroundNumber++;
            if (backgroundNumber > 2) backgroundNumber = 0; // cycle the three backdrops
            caller.setup();
        }

        public void levelChange()
        {
            if (caller.rocks.empty()) setLevel(currentLevel + 1);
        }

        public void score() => totalScore += currentLevel;

        public void gameOver()
        {
            if (caller.goodie.destroyed())
            {
                caller.controler = null;
                caller.goodie.heading += 15; // spin the wreck
                if (endGameDelay < 0)
                {
                    backgroundNumber = 4; // end-game backdrop (texture[4]; faithful quirk)
                    currentLevel = -1;
                    caller.clearup();
                }
                endGameDelay--;
            }
        }
    }
}
