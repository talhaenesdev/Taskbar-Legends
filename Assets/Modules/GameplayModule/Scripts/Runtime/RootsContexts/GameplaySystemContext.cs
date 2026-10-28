using FlowIoC.BaseModule.Contexts;
using FlowIoC.BaseModule.Controller.Commands;
using Modules.GameplayModule.Controllers;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.RootsContexts
{
    /// <summary>
    /// The idle battle. A run starts once and then loops for ever: each turn waits until it is due,
    /// one party member acts, the creature strikes back, and a defeat on either side runs its own
    /// flow inside the turn before the next one is dispatched. Level-up choices are offered between
    /// kills and answered on their own flow, so the battle never waits on the player.
    /// </summary>
    public class GameplaySystemContext : Context
    {
        private GameplaySignals _signals;
        private GameplayInternalSignals _internalSignals;

        public override void SignalBindings()
        {
            base.SignalBindings();
            _signals = InjectionBinderCrossContext.Bind<GameplaySignals>();
            _internalSignals = InjectionBinder.Bind<GameplayInternalSignals>();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();

            InjectionBinder.Bind<IHeroModel, HeroModel>();
            InjectionBinder.Bind<ICombatModel, CombatModel>();
            InjectionBinder.Bind<IUpgradeOfferModel, UpgradeOfferModel>();
        }

        public override void MediationBindings()
        {
            base.MediationBindings();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();

            CommandBinder.Bind(_signals.Incoming.StartRun)
                .ToSequence<StartCombatCommand>()
                .ToSequence<SpawnEnemyCommand>()
                .ToSequence<AnnounceRunStartedCommand>()
                .ToSequence<SignalDispatchCommand<string>>(_signals.Outgoing.SystemAnnounced, "You have been chosen as a Player.")
                .ToSequence<GrantOfflineRewardsCommand>()
                .ToSequence<ApplyLevelUpsCommand>()
                .ToSequence<ApplyRankCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>()
                .ToSequence<OfferUpgradeCommand>()
                .ToSequence<SignalDispatchCommand>(_internalSignals.Tick);

            // One turn, paced by its first step. The last step starts the next turn as a run of its own.
            CommandBinder.Bind(_internalSignals.Tick)
                .ToSequence<WaitCombatTurnCommand>()
                .ToSequence<PartyStrikeCommand>()
                .ToSequence<JudgeEnemyDefeatCommand>()
                .ToSequence<EnemyStrikeCommand>()
                .ToSequence<JudgeHeroDefeatCommand>()
                .ToSequence<RegenerateHeroCommand>()
                .ToSequence<SignalDispatchCommand>(_internalSignals.Tick);

            CommandBinder.Bind(_internalSignals.EnemyDefeated)
                .ToSequence<GrantRewardsCommand>()
                .ToSequence<JudgeFloorProgressCommand>()
                .ToSequence<ExtractShadowCommand>()
                .ToSequence<ApplyLevelUpsCommand>()
                .ToSequence<ApplyRankCommand>()
                .ToSequence<ReviveFallenCommand>()
                .ToSequence<SaveHeroCommand>()
                .ToSequence<SpawnEnemyCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<OfferUpgradeCommand>();

            CommandBinder.Bind(_internalSignals.HeroDefeated)
                .ToSequence<SignalDispatchCommand<string>>(_signals.Outgoing.SystemAnnounced, "Your party has fallen. The floor starts over...")
                .ToSequence<RestorePartyCommand>()
                .ToSequence<ResetFloorProgressCommand>()
                .ToSequence<SpawnEnemyCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>();

            // A level-up choice: waited out on its own, answered by the player or for them, and the next one offered.
            CommandBinder.Bind(_internalSignals.WatchUpgradeOffer).ToSequence<WatchUpgradeOfferCommand>();
            CommandBinder.Bind(_signals.Incoming.ChooseUpgrade)
                .ToSequence<ApplyUpgradeCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>()
                .ToSequence<OfferUpgradeCommand>();

            // Gold spent in the shop: a level that outlives every Reawakening.
            CommandBinder.Bind(_signals.Incoming.BuyShopItem)
                .ToSequence<BuyShopItemCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>();

            // The climb starts over from floor 1 for Essence; any choice left open goes with it.
            CommandBinder.Bind(_signals.Incoming.Reawaken)
                .ToSequence<ReawakenCommand>()
                .ToSequence<CloseUpgradeOfferCommand>()
                .ToSequence<SpawnEnemyCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>();

            CommandBinder.Bind(_internalSignals.AutoChooseUpgrade)
                .ToSequence<ApplyUpgradeCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>()
                .ToSequence<OfferUpgradeCommand>();
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
