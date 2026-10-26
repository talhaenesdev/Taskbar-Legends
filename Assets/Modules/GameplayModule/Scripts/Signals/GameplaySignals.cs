using FlowIoC.BaseModule.Signals;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Signals
{
    public class GameplaySignals : ISignalHolder
    {
        public GameplaySignalsIncoming Incoming = new();
        public GameplaySignalsOutgoing Outgoing = new();
    }

    public class GameplaySignalsIncoming
    {
        /// <summary>Starts the idle battle loop. A second start while it runs does nothing.</summary>
        public Signal StartRun = new();
    }

    public class GameplaySignalsOutgoing
    {
        /// <summary>The run is up: the hero and the first creature, for whatever shows them.</summary>
        public Signal<HeroStateVO, EnemyStateVO> RunStarted = new();

        public Signal<HeroStateVO> HeroChanged = new(hideCommandLog: true);
        public Signal<EnemyStateVO> EnemyChanged = new(hideCommandLog: true);

        /// <summary>The damage of one of the hero's blows, and whether it was critical.</summary>
        public Signal<int, bool> EnemyHit = new(hideCommandLog: true);

        /// <summary>The damage of one of the creature's blows.</summary>
        public Signal<int> HeroHit = new(hideCommandLog: true);

        /// <summary>A plain line of the battle log.</summary>
        public Signal<string> CombatLogged = new(hideCommandLog: true);

        /// <summary>A System window notice - a level gained, a rank reached, a death.</summary>
        public Signal<string> SystemAnnounced = new();

        public Signal<HeroRank> RankedUp = new();
    }
}
