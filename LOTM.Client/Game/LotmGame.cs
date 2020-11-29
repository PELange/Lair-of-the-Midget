using LOTM.Client.Engine;
using LOTM.Client.Game.Objects;
using LOTM.Shared.Engine.Math;

namespace LOTM.Client.Game
{
    public class LotmGame : GuiGame
    {
        public LotmGame(int windowWidth, int windowHeight) : base(windowWidth, windowHeight, "Lair of the Midget")
        {
        }

        protected override void OnInit()
        {
            //Register main texture atlas
            AssetManager.RegisterTexture("Game/Assets/Textures/0x72_DungeonTilesetII_v1.3.png", "dungeonTiles");

            //Register indivual sprites on the atlas using 16x16 grid indices
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(1, 23, 2, 24), "demonboss_idle_0");

            //Wizard
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(8, 10, 8, 11), "wizzard_m_idle_anim_f0");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(9, 10, 9, 11), "wizzard_m_idle_anim_f1");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(10, 10, 10, 11), "wizzard_m_idle_anim_f2");
            AssetManager.RegisterSpriteByGridIndex("dungeonTiles", 16, new Vector4Int(11, 10, 11, 11), "wizzard_m_idle_anim_f3");

            World.Objects.Add(new DemonBoss(new Vector2(100, 100), 45, new Vector2(32 * 4, 32 * 4)));

            World.Objects.Add(new WizardOfWisdom(new Vector2(300, 0), 0, new Vector2(16 * 4, 16 * 2 * 4)));
        }

        protected override void OnFixedUpdate(double deltaTime)
        {
        }

        protected override void OnUpdate(double deltaTime)
        {
        }
    }
}
