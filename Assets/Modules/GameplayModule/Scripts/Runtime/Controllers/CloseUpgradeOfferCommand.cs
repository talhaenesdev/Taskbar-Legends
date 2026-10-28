using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Withdraws the level-up choice waiting for an answer, if there is one.</summary>
    internal class CloseUpgradeOfferCommand : Command
    {
        [Inject] private IUpgradeOfferModel _offerModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        public override void Execute()
        {
            if (!_offerModel.HasOpenOffer)
                return;

            int offerId = _offerModel.OpenOffer.OfferId;
            _offerModel.Close();
            _signals.Outgoing.UpgradeOfferClosed.Dispatch(offerId);
        }
    }
}
