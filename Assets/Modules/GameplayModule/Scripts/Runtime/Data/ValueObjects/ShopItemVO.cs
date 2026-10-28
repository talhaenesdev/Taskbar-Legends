namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>A shop item as one record: its config entry and the levels bought, saved under the same id.</summary>
    internal class ShopItemVO
    {
        public ShopItemCVO Config;
        public UpgradeSVO Save;
    }
}
