using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Coroutine;
using Modules.GameplayModule.Models;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// Holds the turn until it is due, which is what paces the battle loop. A battle that has
    /// stopped meanwhile ends the loop here.
    /// </summary>
    internal class WaitCombatTurnCommand : Command
    {
        [Inject] private ICoroutineProvider _coroutineProvider { get; set; }
        [Inject] private ICombatModel _combatModel { get; set; }
        [Inject] private IHeroModel _heroModel { get; set; }

        public override void Execute()
        {
            Retain();

            // Taken into a local: the wait outlives nothing of this instance but the model it reads.
            ICombatModel combatModel = _combatModel;

            _coroutineProvider.WaitForSeconds(_heroModel.TurnSeconds, () =>
            {
                if (combatModel.IsRunning)
                    Release();
                else
                    Stop();
            });
        }
    }
}
