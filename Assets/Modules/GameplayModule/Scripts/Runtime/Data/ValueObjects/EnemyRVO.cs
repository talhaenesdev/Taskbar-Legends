namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The creature in play: its config entry and the values it was spawned with.</summary>
    internal class EnemyRVO
    {
        public int SpawnId;
        public EnemyCVO Config;
        public int Level;
        public int MaxHp;
        public int Hp;
        public int Attack;
        public int Exp;
        public int Gold;

        /// <summary>Turns left while the creature is still walking in; nobody strikes until it has arrived.</summary>
        public int ArrivalTurns;

        public bool IsArriving => ArrivalTurns > 0;
    }
}
