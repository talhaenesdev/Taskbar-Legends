using FlowIoC.BaseModule.Signals;

namespace Modules.GameplayModule.Signals
{
    internal class GameplayInternalSignals : ISignalHolder
    {
        /// <summary>One turn of combat, paced by itself: its first step waits for the turn to be due.</summary>
        public Signal Tick = new(hideCommandLog: true);

        public Signal EnemyDefeated = new();
        public Signal HeroDefeated = new();

        /// <summary>Waits out the offer with that id, and answers it for the player when nobody has.</summary>
        public Signal<int> WatchUpgradeOffer = new();

        /// <summary>A card is taken for the player: the offer's id, and the card's index.</summary>
        public Signal<int, int> AutoChooseUpgrade = new();
    }
}
