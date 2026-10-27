namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>An upgrade as one record: its config entry and its saved stack count.</summary>
    internal class UpgradeVO
    {
        public UpgradeCVO Config;
        public UpgradeSVO Save;
    }
}
