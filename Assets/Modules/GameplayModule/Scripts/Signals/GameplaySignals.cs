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

        /// <summary>A party member struck the creature: who, the damage, and whether it was critical.</summary>
        public Signal<PartyRole, int, bool> EnemyHit = new(hideCommandLog: true);

        /// <summary>The creature struck a party member: who, and the damage.</summary>
        public Signal<PartyRole, int> HeroHit = new(hideCommandLog: true);

        /// <summary>A plain line of the battle log.</summary>
        public Signal<string> CombatLogged = new(hideCommandLog: true);

        /// <summary>A System window notice - a level gained, a rank reached, a death.</summary>
        public Signal<string> SystemAnnounced = new();

        public Signal<HeroRank> RankedUp = new();
    }
}
