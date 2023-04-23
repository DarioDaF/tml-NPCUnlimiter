using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace NPCUnlimiter
{
#if DEBUG
    public class BreakPointSystem : ModSystem
    {
        public override void Load()
        {
            base.Load();

            On.Terraria.Main.Update += Main_Update;
        }

        public override void Unload()
        {
            On.Terraria.Main.Update -= Main_Update;

            base.Unload();
        }

        private void Main_Update(On.Terraria.Main.orig_Update orig, Terraria.Main self, GameTime gameTime)
        {
            orig(self, gameTime);
        }
    }
#endif
}
