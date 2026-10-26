using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>Marks the battle as running and readies the hero. A run already going ends the flow here.</summary>
    internal class StartCombatCommand : Command
    {
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute()
        {
            if (_combatModel.IsRunning)
            {
                Retain();
                Stop();
                return;
            }

            _combatModel.IsRunning = true;
            _heroModel.RestoreFullHp();
        }
    }
}
