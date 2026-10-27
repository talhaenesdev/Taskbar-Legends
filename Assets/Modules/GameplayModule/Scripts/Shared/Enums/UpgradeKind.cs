namespace Modules.GameplayModule.Shared.Enums
{
    /// <summary>What a level-up choice improves. Serialized as an int, so every value keeps its number.</summary>
    public enum UpgradeKind
    {
        Recruit = 0,
        Attack = 1,
        Health = 2,
        CritChance = 3,
        CritDamage = 4,
        Regen = 5,
        Gold = 6,
        Exp = 7,
        Speed = 8,
    }
}
