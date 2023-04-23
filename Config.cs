using Newtonsoft.Json;
using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace NPCUnlimiter
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        /*
        public Config()
        {
            // Doesn't set defaults :(
            NewNPCLimit = MaxNPCHandler.VanillaMaxNPCs;
        }
        */

        [DefaultValue(null)]
        [JsonProperty] // Force property on private so it's not shown in tML
        private int? NewNPCLimit = null;

        [Label("NPC Limit")]
        [Range(0, 1000)]
        //[DefaultValue(Terraria.Main.maxNPCs)] // @TODO: When this will not be a constant need to set it dynamically?
        [Tooltip("Changing NPC Limit BELOW 200 can cause problems for Mods (expecially in 1.4.3)")]
        [JsonIgnore]
        public int _NewNPCLimit {
            get => NewNPCLimit ?? MaxNPCHandler.VanillaMaxNPCs;
            set
            {
                NewNPCLimit = value == MaxNPCHandler.VanillaMaxNPCs ? null : value;
            }
        }

        [Label("Debug mode")]
        [JsonIgnore]
        public bool DebugMode => NPCUnlimiter.OutInfosLogFile is not null; 

        public override void OnChanged()
        {
            base.OnChanged();
            var handler = (Mod as NPCUnlimiter).Handler;
            if (handler is not null)
            {
                Console.WriteLine($"[NPCUnlimiter]: Changing limit to {_NewNPCLimit}");
                handler.Resize(_NewNPCLimit);
            }
        }
    }
}
