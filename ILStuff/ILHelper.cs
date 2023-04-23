using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;

using InstrList = Mono.Collections.Generic.Collection<Mono.Cecil.Cil.Instruction>;

namespace NPCUnlimiter.ILStuff
{
    public static class ILHelper
    {
        class InstrNoNopEnumerator : IEnumerator<Instruction>, ICloneable //, IEnumerable<Instruction>
        {
            private readonly InstrList insts;
            public int Idx { get; private set; }

            public InstrNoNopEnumerator(InstrList insts, int idx = 0)
            {
                this.insts = insts;
                // Skip initial nops
                this.Idx = idx - 1;
                //this.MoveNext(); // Don't call cause it's expected to be called BEFORE operation
            }

            public Instruction Current => insts[Idx];

            object IEnumerator.Current => this.Current;

            public InstrNoNopEnumerator Clone() => new InstrNoNopEnumerator(insts, Idx);

            public void Dispose()
            {
                Idx = insts.Count;
            }

            public bool MoveNext()
            {
                for (++Idx; Idx < insts.Count; ++Idx)
                {
                    if (!Current.MatchNop())
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Reset()
            {
                Idx = 0;
            }

            object ICloneable.Clone() => this.Clone();

            /*
            // Should be clone or just this?
            public IEnumerator<Instruction> GetEnumerator() => this.Clone();
            IEnumerator IEnumerable.GetEnumerator() => this.Clone();
            */
        }

        public static bool TryGotoNextWithNop(this ILCursor cur, MoveType moveType = MoveType.Before, params Func<Instruction, bool>[] predicates)
        {
            // Based on https://github1s.com/MonoMod/MonoMod/blob/master/MonoMod.Utils/Cil/ILCursor.cs#L275-L293

            int matchStart = cur.Index;
            if (cur.SearchTarget == SearchTarget.Next)
                ++matchStart;

            var startIter = new InstrNoNopEnumerator(cur.Instrs, matchStart);
            while (startIter.MoveNext())
            {
                // I should check if at this start position there is a match

                var matchIter = startIter.Clone();
                for (var i = 0; i < predicates.Length; ++i)
                {
                    if (!matchIter.MoveNext())
                    {
                        // Reached end, not enough valid instructions anymore, search failed
                        return false;
                    }
                    if (!predicates[i](matchIter.Current))
                    {
                        // Skip to next start position
                        goto SkipToNextStart;
                    }
                }
                // All matched correctly, move the cursor and exit
                cur.Goto(moveType == MoveType.After ? matchIter.Idx : startIter.Idx, moveType, true);
                return true;

                SkipToNextStart: ;
            }

            // Should reach here only if you start from the end of the function itself
            return false;
        }
    }
}
