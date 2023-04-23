using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NPCUnlimiter.ILStuff
{
    public class MyILHook : IDisposable
    {
        private MethodBase method;
        private Action<ILContext> manipulator;

        public MyILHook(MethodBase method, Action<ILContext> manipulator)
        {
            this.method = method;
            this.manipulator = manipulator;

            HookEndpointManager.Modify(this.method, this.manipulator);
        }

        public void Dispose()
        {
            HookEndpointManager.Unmodify(this.method, this.manipulator);
        }
    }
}
