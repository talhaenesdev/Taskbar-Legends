using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Signals;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Buys the next level of the shop item at that index. An unknown item, a maxed one or short gold ends the flow here.</summary>
    internal class BuyShopItemCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }

        [SignalParam] private int _index { get; set; }

        public override void Execute()
        {
            ShopItemVO item = _heroModel.FindShopItem(_index);
            if (item == null || !_heroModel.TryBuy(item))
            {
                if (item != null)
                    _signals.Outgoing.CombatLogged.Dispatch(_heroModel.IsMaxed(item) ? $"{item.Config.Title} is maxed." : "Not enough gold.");

                Retain();
                Stop();
                return;
            }

            _signals.Outgoing.CombatLogged.Dispatch($"Bought {item.Config.Title} (Lv {item.Save.Count})");
        }
    }
}
