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

        [Header("DEBUG (Require Restart!)")]

        [Label("Check Patches on Release")]
        [DefaultValue(true)]
        [Tooltip("Wether or not to check for correct amount of patches on first application in Release build")]
        public bool CheckPatchesOnRelease { get; set; }

        [Label("Prevent double IL application")]
        [DefaultValue(false)]
        [Tooltip("This check allows the mod to reload correctly (more than once), but can cause problems with other mods")]
        public bool PreventDoubleIL { get; set; }

        [Label("Prevent patches on Unload")]
        [DefaultValue(true)]
        [Tooltip("This will prevent conflict from the mod to itself (notably ScreenDarkness borken), but will also mean that on 2nd reload overlapped IL hooks won't work")]
        public bool PreventPatchOnUnload { get; set; }

        [Label("Debug mode")]
        [JsonIgnore]
        public bool DebugMode => NPCUnlimiter.IsDebug;

        [Label("Patch log")]
        public string PatchLog => string.Join("\n", MaxNPCHandler.patchLog);

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
