using System.Collections.Generic;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>The level-up choice waiting for an answer.</summary>
    internal class UpgradeOfferRVO
    {
        public int OfferId;
        public List<UpgradeOptionRVO> Options = new();
    }
}
