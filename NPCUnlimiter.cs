using MonoMod.Utils;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Terraria.ModLoader;

namespace NPCUnlimiter
{
    using PatchInfo = Dictionary<string, MaxNPCHandler.PatchResultInfo>;

    public class NPCUnlimiter : Mod
	{
        public const string OutInfosLogFile =
#if DEBUG
    "C:\\Users\\Dario\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\NPCUnlimiter\\log\\outInfos.json"
#else
    null
#endif
        ;

        public MaxNPCHandler Handler = null;

        public T LoadInternalJson<T>(string path)
        {
            using (var stream = this.GetFileStream(path))
            {
                return JsonSerializer.Deserialize<T>(
                    stream,
                    new JsonSerializerOptions()
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        IncludeFields = true
                    }
                );
            }
        }

        public override void Load()
        {
            base.Load();

            PatchInfo patchInfo = new();

            patchInfo.AddRange(LoadInternalJson<PatchInfo>("patchlist/hardcodedLimits.json"));

            // Only for 1.4.3
            patchInfo.AddRange(LoadInternalJson<PatchInfo>("patchlist/constUsage.json"));

            Handler = new();
            Handler.Patch(patchInfo);

            if (OutInfosLogFile is not null)
            {
                using (var stream = File.Create(OutInfosLogFile))
                {
                    JsonSerializer.Serialize(
                        stream,
                        Handler.OutInfos,
                        new JsonSerializerOptions()
                        {
                            WriteIndented = true,
                            IncludeFields = true
                        }
                    );
                }
            }

            // Apply the configuration
            ModContent.GetInstance<Config>().OnChanged();
        }

        public override void Unload()
        {
            Handler?.Dispose(); // If crashes on config Handler could be null
            Handler = null;

            base.Unload();
        }
    }
}