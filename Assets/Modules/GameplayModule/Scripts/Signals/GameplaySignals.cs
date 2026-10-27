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

        /// <summary>The player picked a card of a level-up choice: the offer's id, and the card's index.</summary>
        public Signal<int, int> ChooseUpgrade = new();
    }

    public class GameplaySignalsOutgoing
    {
        /// <summary>The run is up: the party and the first creature, for whatever shows them.</summary>
        public Signal<HeroStateVO, EnemyStateVO> RunStarted = new();

        public Signal<HeroStateVO> HeroChanged = new(hideCommandLog: true);
        public Signal<EnemyStateVO> EnemyChanged = new(hideCommandLog: true);

        /// <summary>A party member struck the creature: who, the damage, and whether it was critical.</summary>
        public Signal<PartyRole, int, bool> EnemyHit = new(hideCommandLog: true);

        /// <summary>The creature struck a party member: who, and the damage.</summary>
        public Signal<PartyRole, int> HeroHit = new(hideCommandLog: true);

        /// <summary>A healer mended a party member: the healer, the one healed, and how much.</summary>
        public Signal<PartyRole, PartyRole, int> MemberHealed = new(hideCommandLog: true);

        /// <summary>A plain line of the battle log.</summary>
        public Signal<string> CombatLogged = new(hideCommandLog: true);

        /// <summary>A System window notice - a level gained, a rank reached, a floor cleared, a death.</summary>
        public Signal<string> SystemAnnounced = new();

        public Signal<HeroRank> RankedUp = new();

        /// <summary>A level-up choice is waiting for the player.</summary>
        public Signal<UpgradeOfferVO> UpgradeOffered = new();

        /// <summary>The choice with that id has been answered, by the player or for them.</summary>
        public Signal<int> UpgradeOfferClosed = new();
    }
}
