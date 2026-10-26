using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    /// <summary>The fight in progress: the creature on the field, whose turn it is, and whether the battle loop is running.</summary>
    internal interface ICombatModel
    {
        bool IsRunning { get; set; }

        EnemyRVO Enemy { get; }
        bool IsEnemyDead { get; }

        /// <summary>Puts a creature of that kind on the field, scaled to the level, walking in for arrivalTurns turns.</summary>
        void Spawn(EnemyCVO config, int level, float growthPerLevel, int arrivalTurns);

        void DamageEnemy(int amount);

        /// <summary>The index of the party member whose turn it is, then moves the turn on.</summary>
        int TakeMemberTurn(int partySize);

        /// <summary>True on the turns the creature strikes - once every attackEveryTurns turns it is on the field.</summary>
        bool TakeEnemyTurn(int attackEveryTurns);

        EnemyStateVO GetEnemyState();
    }
}
