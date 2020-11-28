using LOTM.Client.Engine;

namespace LOTM.Client.Game
{
    public class LotmGame : GuiGame
    {
        public LotmGame(int windowWidth, int windowHeight) : base(windowWidth, windowHeight, "Lair of the Midget")
        {
        }

        protected override void OnInit()
        {
            //World.Objects.Add(new SpinnyCubeOfDeath(5, new Vector2(10, 10), 0));
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
        }

        protected override void OnUpdate(double deltaTime)
        {
        }
    }
}
