using FlowIoC.BaseModule.Signals;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.GameplayScreenModule.Signals
{
    public class GameplayScreenSignals : ISignalHolder
    {
        public GameplayScreenSignalsIncoming Incoming = new();
        public GameplayScreenSignalsOutgoing Outgoing = new();
    }

    public class GameplayScreenSignalsIncoming
    {
        /// <summary>Opens the battle window, filled with the hero and the creature it is given.</summary>
        public Signal<HeroStateVO, EnemyStateVO> OpenGameplayScreen = new();

        // The values below change while the window is up, each on its own, so each has a signal.
        public Signal<HeroStateVO> ApplyHero = new(hideCommandLog: true);
        public Signal<EnemyStateVO> ApplyEnemy = new(hideCommandLog: true);
        public Signal<PartyRole, int, bool> ShowEnemyHit = new(hideCommandLog: true);
        public Signal<PartyRole, int> ShowHeroHit = new(hideCommandLog: true);
        public Signal<string> ShowLog = new(hideCommandLog: true);
        public Signal<string> ShowAnnouncement = new();
    }

    public class GameplayScreenSignalsOutgoing
    {
    }
}
