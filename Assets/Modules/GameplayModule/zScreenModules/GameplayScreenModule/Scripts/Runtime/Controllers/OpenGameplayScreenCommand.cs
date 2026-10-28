using System;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using FlowIoC.ScreenModule.Service;
using Modules.DesktopWindowModule.Services;
using Modules.GameplayModule.GameplayScreenModule.Data.ValueObjects;
using Modules.GameplayModule.GameplayScreenModule.Models;
using Modules.GameplayModule.GameplayScreenModule.ViewsMediators;
using Modules.GameplayModule.Shared.Data.ValueObjects;

namespace Modules.GameplayModule.GameplayScreenModule.Controllers
{
    internal class OpenGameplayScreenCommand : Command
    {
        [Inject] private IScreenService _screenService { get; set; }
        [Inject] private IWindowSettingsModel _settingsModel { get; set; }
        [Inject] private IDesktopWindowService _desktopWindowService { get; set; }

        [SignalParam] private HeroStateVO _hero { get; set; }
        [SignalParam] private EnemyStateVO _enemy { get; set; }

        /// <summary>
        /// Opens the battle window and fills it with the hero and creature the run started with,
        /// before anybody can see it. Three ways out, and every one of them resolves the retain.
        /// </summary>
        public override async void Execute()
        {
            Retain();

            FlowLogger.Log("Execute - OpenGameplayScreenCommand");

            // Read into locals: the await hands this instance's properties nothing to rely on.
            HeroStateVO hero = _hero;
            EnemyStateVO enemy = _enemy;
            WindowSettingsVO settings = _settingsModel.GetState(_desktopWindowService.IsStartingWithWindows);

            try
            {
                GameplayScreenView screen = await _screenService.Open<GameplayScreenView>().Show<GameplayScreenView>();

                if (screen == null)
                {
                    FlowLogger.LogError("OpenGameplayScreenCommand - the screen did not open.");
                    Stop();
                    return;
                }

                screen.ShowHero(hero);
                screen.ShowEnemy(enemy);
                screen.ShowSettings(settings);
                Release();
            }
            catch (Exception exception)
            {
                FlowLogger.LogError($"OpenGameplayScreenCommand threw while opening the screen: {exception}");
                Stop();
            }
        }
    }
}
