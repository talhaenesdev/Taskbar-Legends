using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Coroutine;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// Waits as long as the player is given to choose. An idle game cannot wait for ever, so an
    /// offer still open when the time is up has its first card taken for the player.
    /// </summary>
    [LongRetain(60)]
    internal class WatchUpgradeOfferCommand : Command
    {
        [Inject] private ICoroutineProvider _coroutineProvider { get; set; }
        [Inject] private IUpgradeOfferModel _offerModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }

        [SignalParam] private int _offerId { get; set; }

        public override void Execute()
        {
            Retain();

            // Taken into locals: the wait outlives what this instance may rely on.
            int offerId = _offerId;
            IUpgradeOfferModel offerModel = _offerModel;
            GameplayInternalSignals internalSignals = _internalSignals;

            _coroutineProvider.WaitForSeconds(_heroModel.Config.AutoPickSeconds, () =>
            {
                Release();

                if (offerModel.OpenOffer != null && offerModel.OpenOffer.OfferId == offerId)
                    internalSignals.AutoChooseUpgrade.Dispatch(offerId, 0);
            });
        }
    }
}
