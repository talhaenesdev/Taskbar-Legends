using FlowIoC.BaseModule.Contexts;
using FlowIoC.BaseModule.Controller.Commands;
using Modules.GameplayModule.Controllers;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.RootsContexts
{
    /// <summary>
    /// The idle battle. A run starts once and then loops for ever: each turn waits until it is due,
    /// the hero strikes, the creature strikes back, and a defeat on either side runs its own flow
    /// inside the turn before the next one is dispatched.
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
                .ToSequence<SignalDispatchCommand>(_internalSignals.Tick);

            // One turn, paced by its first step. The last step starts the next turn as a run of its own.
            CommandBinder.Bind(_internalSignals.Tick)
                .ToSequence<WaitCombatTurnCommand>()
                .ToSequence<HeroStrikeCommand>()
                .ToSequence<JudgeEnemyDefeatCommand>()
                .ToSequence<EnemyStrikeCommand>()
                .ToSequence<JudgeHeroDefeatCommand>()
                .ToSequence<RegenerateHeroCommand>()
                .ToSequence<SignalDispatchCommand>(_internalSignals.Tick);

            CommandBinder.Bind(_internalSignals.EnemyDefeated)
                .ToSequence<GrantRewardsCommand>()
                .ToSequence<ApplyLevelUpsCommand>()
                .ToSequence<ApplyRankCommand>()
                .ToSequence<SaveHeroCommand>()
                .ToSequence<SpawnEnemyCommand>()
                .ToSequence<PublishHeroCommand>();

            CommandBinder.Bind(_internalSignals.HeroDefeated)
                .ToSequence<SignalDispatchCommand<string>>(_signals.Outgoing.SystemAnnounced, "You have died. Revival in progress...")
                .ToSequence<ReviveHeroCommand>()
                .ToSequence<SpawnEnemyCommand>()
                .ToSequence<PublishHeroCommand>()
                .ToSequence<SaveHeroCommand>();
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
