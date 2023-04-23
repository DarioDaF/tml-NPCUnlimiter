using System;
using System.Reflection;
using System.Collections.Generic;
using NPCUnlimiter.ILStuff;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System.Text.Json.Serialization;

namespace NPCUnlimiter
{
    using PatchInfo = Dictionary<string, MaxNPCHandler.PatchResultInfo>;

    public class MaxNPCHandler : IDisposable
    {
        class PatchException : Exception
        {
            public PatchException(string message) : base(message) { }
        }

        [Serializable]
        public class PatchResultInfo
        {
            [JsonIgnore]
            public string Name;

            public int Safe;
            public int AllX_199;
            public int AllX_200;
            public int AllX_201;
        }

        const bool CheckPatchResult =
#if DEBUG
    false
#else
    true
#endif
        ;

        public static int VanillaMaxNPCs = Terraria.Main.maxNPCs; // Used mainly during hooking
        public static int maxNPCs = VanillaMaxNPCs; // @WARN: NEVER SET THIS OUTSIDE THIS CLASS

        public Dictionary<string, Dictionary<string, int>> OutInfos = new();

        public void Patch(PatchInfo pi)
        {
            foreach (var (target, info) in pi)
            {
                info.Name = target;
                var m = MethodSearcher.GetFromString(target);
                var ilManager = (ILContext ctx) => ILManager(ctx, info);

                HookEndpointManager.Modify(m, ilManager);
            }
        }

        private void ILManager(ILContext ctx, PatchResultInfo info)
        {
            //var name = ctx.Method.Name;
            var name = info.Name;

            var p = new Patcher(ctx);

            Dictionary<string, int> outInfo = new();

            if (info.Safe > 0)
            {
                int varCmpEntries = p.Patch_VarCompare();
                int defValEntries = p.Patch_DefaultVal();
                int newArrEntries = p.Patch_NewArray();
                int itaaEntries = p.Patch_IndexToAimAt();
                int entries = varCmpEntries + defValEntries + newArrEntries + itaaEntries;
                if (entries != info.Safe && CheckPatchResult)
                    throw new PatchException($"Wrong number of patches for {name}: [Safe] {entries} != {info.Safe}");

                outInfo["Safe:varCmp"] = varCmpEntries;
                outInfo["Safe:defVal"] = defValEntries;
                outInfo["Safe:newArr"] = newArrEntries;
                outInfo["Safe:itaa"] = itaaEntries;
            }
            if (info.AllX_200 > 0)
            {
                int entries = p.Patch_Unsafe_AllX(200);
                if (entries != info.AllX_200 && CheckPatchResult)
                    throw new PatchException($"Wrong number of patches for {name}: [AllX_200] {entries} != {info.AllX_200}");

                outInfo["AllX_200"] = entries;
            }
            if (info.AllX_199 > 0)
            {
                int entries = p.Patch_Unsafe_AllX(199);
                if (entries != info.AllX_199 && CheckPatchResult)
                    throw new PatchException($"Wrong number of patches for {name}: [AllX_199] {entries} != {info.AllX_199}");

                outInfo["AllX_199"] = entries;
            }
            if (info.AllX_201 > 0)
            {
                int entries = p.Patch_Unsafe_AllX(201);
                if (entries != info.AllX_201 && CheckPatchResult)
                    throw new PatchException($"Wrong number of patches for {name}: [AllX_201] {entries} != {info.AllX_201}");

                outInfo["AllX_201"] = entries;
            }

            OutInfos[info.Name] = outInfo;
        }

        public void UnPatch()
        {
            HookEndpointManager.RemoveAllOwnedBy(typeof(MaxNPCHandler).Assembly);
        }

        public void Dispose()
        {
            this.UnPatch();
        }

        public void Resize(int newMaxNPCs) {
            if (newMaxNPCs == maxNPCs)
                return;

            // Keep always the array to the highest size to avoid out of bounds errors (only relevant if done IN GAME?)
            if (newMaxNPCs < maxNPCs)
            {
                maxNPCs = newMaxNPCs;
            }

            // @TODO: In 1.4.4 should patch the value of Main.maxNPCs too (and probably use only that)

            // Do the actual resizing of fields etc...
            // @TODO: should be done maybe in a pre update something in the life cycle?
            {
                FieldInfo fi;

                fi = typeof(Terraria.GameContent.PortalHelper).GetField("PortalCooldownForNPCs", BindingFlags.Static | BindingFlags.NonPublic);
                ArrayHelper.ResizeFieldArray(fi, null, newMaxNPCs);

                fi = typeof(Terraria.GameInput.LockOnHelper).GetField("_drawProgress", BindingFlags.Static | BindingFlags.NonPublic);
                ArrayHelper.ResizeFieldArrayMulti(fi, null, new ArrayHelper.ArrayDim(newMaxNPCs, 2));

                var oldNpcSize = Terraria.Main.npc.Length;
                var newNpcSize = newMaxNPCs + 1;
                Array.Resize(ref Terraria.Main.npc, newNpcSize);
                for (var i = oldNpcSize; i < newNpcSize; ++i)
                {
                    Terraria.Main.npc[i] = new Terraria.NPC();
                }

                for (var i = 0; i < Terraria.Projectile.perIDStaticNPCImmunity.Length; ++i)
                {
                    Array.Resize(ref Terraria.Projectile.perIDStaticNPCImmunity[i], newMaxNPCs);
                }

                Array.Resize(ref Terraria.NPC.lazyNPCOwnedProjectileSearchArray, newMaxNPCs);

                foreach (var p in Terraria.Main.projectile)
                {
                    Array.Resize(ref p.localNPCImmunity, newMaxNPCs);
                }
            }

            maxNPCs = newMaxNPCs; // In case it was not done before (resizing up for e.g.)
        }
    }
}
