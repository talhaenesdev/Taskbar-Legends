using FlowIoC.BaseModule.Signals;

namespace Modules.GameplayModule.Signals
{
    internal class GameplayInternalSignals : ISignalHolder
    {
        /// <summary>One turn of combat, paced by itself: its first step waits for the turn to be due.</summary>
        public Signal Tick = new(hideCommandLog: true);

        public Signal EnemyDefeated = new();
        public Signal HeroDefeated = new();
    }
}
