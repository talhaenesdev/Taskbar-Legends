using FlowIoC.BaseModule.Connectors;
using FlowIoC.BaseModule.Contexts;
using Modules.GameplayModule.GameplayScreenModule.Signals;
using Modules.GameplayModule.Signals;

namespace Modules.ConnectorModule.RootsContexts
{
    /// <summary>
    /// The battle and its window meet here. RunStarted opens the window already filled; every
    /// change after that reaches the open window on a signal of its own. The one thing the window
    /// sends back is the card the player picked.
    /// </summary>
    public class GameplayConnectorSubContext : Context
    {
        private const string GROUP = nameof(GameplayConnectorSubContext);

        private GameplaySignals _gameplaySignals;
        private GameplayScreenSignals _gameplayScreenSignals;

        public override void Setup()
        {
            base.Setup();

            _gameplaySignals = InjectionBinderCrossContext.GetInstance<GameplaySignals>();
            _gameplayScreenSignals = InjectionBinderCrossContext.GetInstance<GameplayScreenSignals>();

            IncomingSignals();
            OutgoingSignals();
        }

        private void IncomingSignals()
        {
            GameplaySignalsOutgoing outgoing = _gameplaySignals.Outgoing;
            GameplayScreenSignalsIncoming screen = _gameplayScreenSignals.Incoming;

            outgoing.RunStarted.Connect(screen.OpenGameplayScreen, GROUP);
            outgoing.HeroChanged.Connect(screen.ApplyHero, GROUP);
            outgoing.EnemyChanged.Connect(screen.ApplyEnemy, GROUP);
            outgoing.EnemyHit.Connect(screen.ShowEnemyHit, GROUP);
            outgoing.HeroHit.Connect(screen.ShowHeroHit, GROUP);
            outgoing.CombatLogged.Connect(screen.ShowLog, GROUP);
            outgoing.SystemAnnounced.Connect(screen.ShowAnnouncement, GROUP);
            outgoing.MemberHealed.Connect(screen.ShowHeal, GROUP);
            outgoing.UpgradeOffered.Connect(screen.ShowUpgradeOffer, GROUP);
            outgoing.UpgradeOfferClosed.Connect(screen.HideUpgradeOffer, GROUP);
        }

        private void OutgoingSignals()
        {
            GameplayScreenSignalsOutgoing screen = _gameplayScreenSignals.Outgoing;
            GameplaySignalsIncoming incoming = _gameplaySignals.Incoming;

            screen.UpgradeChosen.Connect(incoming.ChooseUpgrade, GROUP);
            screen.ShopItemChosen.Connect(incoming.BuyShopItem, GROUP);
            screen.ReawakenChosen.Connect(incoming.Reawaken, GROUP);
        }

        public override void DestroyContext()
        {
            SignalConnector.DisconnectGroup(GROUP);
            base.DestroyContext();
        }
    }
}
