using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    /// <summary>The fight in progress: the creature on the field and whether the battle loop is running.</summary>
    internal interface ICombatModel
    {
        bool IsRunning { get; set; }

        EnemyRVO Enemy { get; }
        bool IsEnemyDead { get; }

        /// <summary>Puts a creature of that kind on the field, scaled to the level.</summary>
        void Spawn(EnemyCVO config, int level, float growthPerLevel);

        void DamageEnemy(int amount);

        EnemyStateVO GetEnemyState();
    }
}
