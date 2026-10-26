using FlowIoC.BaseModule.Connectors;
using FlowIoC.BaseModule.Contexts;
using Modules.GameplayModule.Signals;
using Modules.LoadingModule.LoadingScreenModule.Signals;
using Modules.MainModule.Signals;

namespace Modules.ConnectorModule.RootsContexts
{
    /// <summary>
    /// An idle game has no title screen to stop at: the application coming up starts the run.
    /// </summary>
    public class MainConnectorSubContext : Context
    {
        private MainSignals _mainSignals;
        private GameplaySignals _gameplaySignals;
        private LoadingScreenSignals _loadingScreenSignals;

        public override void Setup()
        {
            base.Setup();

            _mainSignals = InjectionBinderCrossContext.GetInstance<MainSignals>();
            _gameplaySignals = InjectionBinderCrossContext.GetInstance<GameplaySignals>();
            _loadingScreenSignals = InjectionBinderCrossContext.GetInstance<LoadingScreenSignals>();

            IncomingSignals();
        }

        private void IncomingSignals()
        {
            _mainSignals.Outgoing.Started.Connect(_gameplaySignals.Incoming.StartRun);

            // The loading screen's retry button asks Main to boot again; the set it names is Main's boot.
            _loadingScreenSignals.Outgoing.RetryClicked.Connect(_ => _mainSignals.Incoming.RetryBoot.Dispatch());
        }

        public override void DestroyContext()
        {
            UnbindIncomingSignals();

            base.DestroyContext();
        }

        private void UnbindIncomingSignals()
        {
            _mainSignals.Outgoing.Started.Disconnect();
            _loadingScreenSignals.Outgoing.RetryClicked.Disconnect();
        }
    }
}
