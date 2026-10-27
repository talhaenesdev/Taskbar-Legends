using System;
using System.Collections.Generic;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>A level-up choice: the cards on offer, and how long before the first is taken for the player.</summary>
    [Serializable]
    public class UpgradeOfferVO
    {
        public int OfferId;
        public float AutoPickSeconds;
        public List<UpgradeOptionVO> Options = new();
    }
}
