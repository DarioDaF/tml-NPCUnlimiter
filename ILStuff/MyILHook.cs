using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;

namespace NPCUnlimiter.ILStuff
{
    public class MyILHook : IDisposable
    {
        private MethodBase method;
        private Action<ILContext> manipulator;
        private bool disposed;

        public MyILHook(MethodBase method, Action<ILContext> manipulator)
        {
            this.method = method;
            this.manipulator = manipulator;

            this.disposed = false;
            HookEndpointManager.Modify(this.method, this.manipulator);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            HookEndpointManager.Unmodify(this.method, this.manipulator);
        }
    }
}
