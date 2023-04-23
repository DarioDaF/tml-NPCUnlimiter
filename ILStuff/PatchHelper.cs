using MonoMod.Cil;
using System;
using System.Collections.Generic;

namespace NPCUnlimiter.ILStuff
{
    public class PatchHelper
    {
        private ILContext ctx;

        public ILCursor NewCursor() => new(ctx);

        public class ReplaceInstr : IDisposable
        {
            private ILCursor cur;
            public ReplaceInstr(ILCursor _cur)
            {
                cur = _cur;
                cur.MoveAfterLabels();
            }
            public void Dispose()
            {
                cur.Remove();
            }
        }

        public PatchHelper(ILContext ctx)
        {
            this.ctx = ctx;
        }

        public HashSet<int> FindLoc() => ArrayHelper.SetUnion(FindLoc_MainNPCAccess(), FindLoc_NewNPC());

        private HashSet<int> Loc_NewNPC = null;
        public HashSet<int> FindLoc_NewNPC()
        {
            if (Loc_NewNPC is not null)
                return Loc_NewNPC;

            ILCursor cur = new(ctx);
            int target = -1;
            // Find all loc containing NPC ids
            Loc_NewNPC = new HashSet<int>();
            while (cur.TryGotoNextWithNop(MoveType.Before,
                i => i.MatchCall<Terraria.NPC>("NewNPC"),
                i => i.MatchStloc(out target)
            ))
            {
                Loc_NewNPC.Add(target);
            }
            return Loc_NewNPC;
        }

        private HashSet<int> Loc_MainNPCAccess = null;
        public HashSet<int> FindLoc_MainNPCAccess()
        {
            if (Loc_MainNPCAccess is not null)
                return Loc_MainNPCAccess;

            ILCursor cur = new(ctx);
            int target = -1;
            // Find all loc containing NPC ids
            Loc_MainNPCAccess = new HashSet<int>();
            while (cur.TryGotoNextWithNop(MoveType.Before,
                i => i.MatchLdsfld<Terraria.Main>("npc"),
                i => i.MatchLdloc(out target),
                i => i.MatchLdelemRef()
            ))
            {
                Loc_MainNPCAccess.Add(target);
            }
            return Loc_MainNPCAccess;
        }

    }
}
