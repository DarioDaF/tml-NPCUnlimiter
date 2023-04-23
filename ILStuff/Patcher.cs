using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;

namespace NPCUnlimiter.ILStuff
{
    public class Patcher
    {
        private PatchHelper helper;

        public Patcher(ILContext ctx) {
            helper = new PatchHelper(ctx);
        }

        private static void ReplaceWithMaxNPCs(ILCursor cur)
        {
            using (new PatchHelper.ReplaceInstr(cur))
            {
                cur.Emit<MaxNPCHandler>(OpCodes.Ldsfld, nameof(MaxNPCHandler.maxNPCs));
            }
        }

        /**
         * <summary>
         *  Patch all entries of the constant `x`
         * </summary>
         */
        public int Patch_Unsafe_AllX(int x)
        {
            var cur = helper.NewCursor();

            int entries = 0;

            while (cur.TryGotoNext(MoveType.Before,
                i => i.MatchLdcI4(x)
            ))
            {
                ++entries;

                ReplaceWithMaxNPCs(cur);
            }

            return entries;
        }

        /**
         * <summary>
         *  Patch creation of arrays that contains stuff seeming NPC ids
         * </summary>
         */
        public int Patch_NewArray()
        {
            var npcIdLocals = helper.FindLoc();

            // Find arrays where ids get pushed to
            var arrTargets = new HashSet<int>();
            {
                var cur = helper.NewCursor();
                int target = -1;
                int arrTarget = -1;
                while (cur.TryGotoNextWithNop(MoveType.Before,
                    i => i.MatchLdloc(out arrTarget), // Array
                    i => i.MatchLdloc(out _), // Idx
                    i => i.MatchLdloc(out target), // Value
                    i => i.MatchStelemI4()
                ))
                {
                    if (npcIdLocals.Contains(target))
                        arrTargets.Add(arrTarget);
                }
            }

            int entries = 0;

            // Find constructors for these arrays that have NPC lenght
            {
                var cur = helper.NewCursor();
                int arrTarget = -1;
                while (cur.TryGotoNextWithNop(MoveType.Before,
                    i => i.MatchLdcI4(MaxNPCHandler.VanillaMaxNPCs),
                    i => i.MatchNewarr<int>(),
                    i => i.MatchStloc(out arrTarget)
                ))
                {
                    if (arrTargets.Contains(arrTarget))
                    {
                        // Valid location
                        ++entries;

                        ReplaceWithMaxNPCs(cur);
                    }
                }
            }

            return entries;
        }

        /**
         * <summary>
         *  Patch initaializations of locals seeming NPC ids
         * </summary>
         */
        public int Patch_DefaultVal()
        {
            var npcIdLocals = helper.FindLoc();

            int entries = 0;

            // Find assigners using those vars
            var cur = helper.NewCursor();
            int target = -1;
            while (cur.TryGotoNextWithNop(MoveType.Before,
                i => i.MatchLdcI4(MaxNPCHandler.VanillaMaxNPCs),
                i => i.MatchStloc(out target)
            ))
            {
                if (npcIdLocals.Contains(target))
                {
                    // Valid location
                    ++entries;

                    ReplaceWithMaxNPCs(cur);
                }
            }

            return entries;
        }

        /**
         * <summary>
         *  Patch comparisons referencing BigProgressBarInfo.npcIndexToAimAt
         * </summary>
         */
        public int Patch_IndexToAimAt()
        {
            int entries = 0;

            // Find lt comparison using npc field of BigProgressBarInfo object
            var cur = helper.NewCursor();
            while (cur.TryGotoNextWithNop(MoveType.Before,
                i => i.MatchLdfld<Terraria.GameContent.UI.BigProgressBar.BigProgressBarInfo>(nameof(Terraria.GameContent.UI.BigProgressBar.BigProgressBarInfo.npcIndexToAimAt)),
                i => i.MatchLdcI4(MaxNPCHandler.VanillaMaxNPCs),
                i =>
                    i.MatchClt() || i.MatchBlt(out _) || // Can get simplified to just a branch
                    i.MatchBge(out _) || // Or be inverted logic (note `cge` does not exist)
                    i.MatchCgt() || i.MatchBle(out _) || i.MatchBgt(out _) // Or even include the 201 dummy???
            ))
            {
                // Valid location
                ++entries;

                cur.TryGotoNextWithNop(MoveType.Before); // Move being aware of nops!!! (don't Index += 1)
                ReplaceWithMaxNPCs(cur);
            }

            return entries;
        }

        /**
         * <summary>
         *  Patch comparisons on locals that seem NPC ids (includes "for loop"-like condition too)
         * </summary>
         */
        public int Patch_VarCompare()
        {
            var npcIdLocals = helper.FindLoc();

            int entries = 0;

            // Find lt comparison using these ids
            var target = -1;
            var cur = helper.NewCursor();
            while (cur.TryGotoNextWithNop(MoveType.Before,
                i => i.MatchLdloc(out target),
                i => i.MatchLdcI4(MaxNPCHandler.VanillaMaxNPCs),
                i =>
                    i.MatchClt() || i.MatchBlt(out _) || // Can get simplified to just a branch
                    i.MatchBge(out _) // Or be inverted logic (note `cge` does not exist)
            ))
            {
                if (npcIdLocals.Contains(target))
                {
                    // Valid location
                    ++entries;

                    cur.TryGotoNextWithNop(MoveType.Before); // Move being aware of nops!!! (don't Index += 1)
                    ReplaceWithMaxNPCs(cur);
                }
            }

            return entries;
        }

    }
}
