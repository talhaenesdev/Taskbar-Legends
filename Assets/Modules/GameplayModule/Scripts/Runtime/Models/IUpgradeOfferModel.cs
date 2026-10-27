using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    /// <summary>The level-up choice waiting for an answer, if there is one. One is open at a time.</summary>
    internal interface IUpgradeOfferModel
    {
        UpgradeOfferRVO OpenOffer { get; }
        bool HasOpenOffer { get; }

        /// <summary>Opens an offer of these cards under a new id.</summary>
        UpgradeOfferRVO Open(System.Collections.Generic.List<UpgradeOptionRVO> options);

        /// <summary>The open offer's option at that index, when the offer is the one named; null otherwise.</summary>
        UpgradeOptionRVO FindOption(int offerId, int index);

        void Close();

        UpgradeOfferVO GetOpenOfferState(float autoPickSeconds);
    }
}
