using FlowIoC.BaseModule.Contexts;
using Modules.DesktopWindowModule.Services;

namespace Modules.DesktopWindowModule.RootsContexts
{
    public class DesktopWindowServiceContext : Context
    {
        public override void SignalBindings()
        {
            base.SignalBindings();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();

            // The one type other modules reference directly, which is what makes this a Service.
            InjectionBinderCrossContext.Bind<IDesktopWindowService, DesktopWindowService>();
        }

        public override void MediationBindings()
        {
            base.MediationBindings();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();
        }

        public override void Setup()
        {
            base.Setup();
        }

        public override void Launch()
        {
            base.Launch();
        }
    }
}
