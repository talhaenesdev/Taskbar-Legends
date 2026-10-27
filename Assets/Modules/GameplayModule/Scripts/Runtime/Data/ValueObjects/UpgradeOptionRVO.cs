using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>One card of the open offer: either an upgrade or a hero to recruit, and how the card is drawn.</summary>
    internal class UpgradeOptionRVO
    {
        public UpgradeCVO Upgrade;
        public PartyMemberCVO Recruit;
        public UpgradeOptionVO Card;
    }
}
