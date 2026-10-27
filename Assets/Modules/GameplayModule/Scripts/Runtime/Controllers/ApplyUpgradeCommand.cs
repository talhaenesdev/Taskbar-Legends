using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// Applies the card picked from the open offer - a hero joins, or an upgrade takes a stack -
    /// and closes the offer. A pick for an offer that is no longer open (answered already, or a
    /// stale click) ends the flow here.
    /// </summary>
    internal class ApplyUpgradeCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [Inject] private IUpgradeOfferModel _offerModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        [SignalParam(0)] private int _offerId { get; set; }
        [SignalParam(1)] private int _index { get; set; }

        public override void Execute()
        {
            UpgradeOptionRVO option = _offerModel.FindOption(_offerId, _index);
            if (option == null)
            {
                Retain();
                Stop();
                return;
            }

            if (option.Recruit != null)
            {
                _heroModel.Recruit(option.Recruit);
                _signals.Outgoing.SystemAnnounced.Dispatch($"{option.Recruit.Name} has joined your party!");
            }
            else
            {
                _heroModel.Stack(option.Upgrade);
                _signals.Outgoing.CombatLogged.Dispatch($"Upgrade: {option.Card.Title}");
            }

            _heroModel.TakePendingUpgrade();
            _offerModel.Close();
            _signals.Outgoing.UpgradeOfferClosed.Dispatch(_offerId);
        }
    }
}
