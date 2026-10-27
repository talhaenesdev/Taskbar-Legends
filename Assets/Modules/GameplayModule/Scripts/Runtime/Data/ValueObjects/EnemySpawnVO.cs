namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>What the spawning Command decided about the next creature: which kind, how strong, and what it is.</summary>
    internal class EnemySpawnVO
    {
        public EnemyCVO Config;
        public int Level;
        public float HpMultiplier = 1f;
        public float AttackMultiplier = 1f;
        public float RewardMultiplier = 1f;
        public float ScaleMultiplier = 1f;
        public bool IsElite;
        public bool IsBoss;
    }
}
