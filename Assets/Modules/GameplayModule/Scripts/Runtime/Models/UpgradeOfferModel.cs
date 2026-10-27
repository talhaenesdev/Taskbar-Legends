using System.Collections.Generic;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.Models
{
    internal class UpgradeOfferModel : IUpgradeOfferModel
    {
        private int _lastOfferId;

        public UpgradeOfferRVO OpenOffer { get; private set; }
        public bool HasOpenOffer => OpenOffer != null;

        public UpgradeOfferRVO Open(List<UpgradeOptionRVO> options)
        {
            OpenOffer = new UpgradeOfferRVO { OfferId = ++_lastOfferId, Options = options };
            return OpenOffer;
        }

        public UpgradeOptionRVO FindOption(int offerId, int index)
        {
            if (OpenOffer == null || OpenOffer.OfferId != offerId || index < 0 || index >= OpenOffer.Options.Count)
                return null;

            return OpenOffer.Options[index];
        }

        public void Close() => OpenOffer = null;

        public UpgradeOfferVO GetOpenOfferState(float autoPickSeconds)
        {
            var state = new UpgradeOfferVO { OfferId = OpenOffer?.OfferId ?? 0, AutoPickSeconds = autoPickSeconds };
            if (OpenOffer == null)
                return state;

            foreach (UpgradeOptionRVO option in OpenOffer.Options)
                state.Options.Add(option.Card);

            return state;
        }
    }
}
