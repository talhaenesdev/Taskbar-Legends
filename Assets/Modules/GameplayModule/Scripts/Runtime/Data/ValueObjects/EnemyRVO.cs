namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The creature in play: its config entry and the values it was spawned with.</summary>
    internal class EnemyRVO
    {
        public EnemyCVO Config;
        public int Level;
        public int MaxHp;
        public int Hp;
        public int Attack;
        public int Exp;
        public int Gold;

        /// <summary>Set on spawn, so a creature that appears mid-turn does not strike before the hero sees it.</summary>
        public bool JustSpawned;
    }
}
