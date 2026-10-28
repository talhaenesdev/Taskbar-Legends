using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.UnityObjects;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// Rewards the time since the last save as kills on the floor the party was on: so many kills a
    /// minute at the offline efficiency, each worth an average creature of that floor. Too short a
    /// time away earns nothing; a long one is capped.
    /// </summary>
    internal class GrantOfflineRewardsCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            CD_Gameplay config = _heroModel.Config;
            double seconds = System.Math.Min(_heroModel.SecondsAway, config.OfflineMaxHours * 3600d);
            _heroModel.ClearSecondsAway();

            if (seconds < config.OfflineMinSeconds)
                return;

            int kills = Mathf.FloorToInt((float) (seconds / 60d * config.OfflineKillsPerMinute * config.OfflineEfficiency));
            if (kills <= 0 || !TryAverageReward(config, _heroModel.Floor, out float exp, out float gold))
                return;

            (int gainedExp, int gainedGold) = _heroModel.GrantRewards(Mathf.RoundToInt(exp * kills), Mathf.RoundToInt(gold * kills));
            _heroModel.CountKills(kills);

            _signals.Outgoing.SystemAnnounced.Dispatch(
                $"Welcome back! While you were away ({Describe(seconds)}) your party slew {kills:N0}.  +{gainedExp:N0} EXP  +{gainedGold:N0} G");
        }

        /// <summary>The reward of an average creature the floor has unlocked, at the floor's level.</summary>
        private static bool TryAverageReward(CD_Gameplay config, int floor, out float exp, out float gold)
        {
            exp = 0f;
            gold = 0f;
            int count = 0;

            foreach (EnemyCVO enemy in config.Enemies)
            {
                if (enemy.MinFloor > floor) continue;
                exp += enemy.BaseExp;
                gold += enemy.BaseGold;
                count++;
            }

            if (count == 0)
                return false;

            int level = 1 + (floor - 1) * config.EnemyLevelsPerFloor;
            float growth = 1f + config.EnemyGrowthPerLevel * (level - 1);
            exp = exp / count * growth;
            gold = gold / count * growth;
            return true;
        }

        private static string Describe(double seconds)
        {
            int minutes = Mathf.FloorToInt((float) (seconds / 60d));
            return minutes >= 60 ? $"{minutes / 60}h {minutes % 60}m" : $"{minutes}m";
        }
    }
}
