using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.ViewsMediators.Mediator;
using FlowIoC.ScreenModule.Enums;
using FlowIoC.ScreenModule.Extensions;
using FlowIoC.ScreenModule.ViewsMediators.Screen;
using Modules.GameplayModule.GameplayScreenModule.Signals;

namespace Modules.GameplayModule.GameplayScreenModule.ViewsMediators
{
    /// <summary>
    /// Applies what changes while the window is up straight to the view - nothing there is a
    /// decision - and turns a drag of the window into the internal signals that move it.
    /// </summary>
    public class GameplayScreenMediator : IMediator
    {
        [Inject] private GameplayScreenView _view { get; set; }
        [InjectSignal] private GameplayScreenSignals _signals { get; set; }
        [InjectSignal] private GameplayScreenInternalSignals _internalSignals { get; set; }

        public virtual void OnRegister()
        {
            _view.ShowCompleted += OnScreenShown;
            _view.HideCompleted += OnScreenHidden;
        }

        public virtual void OnRemove()
        {
            _view.ShowCompleted -= OnScreenShown;
            _view.HideCompleted -= OnScreenHidden;
        }

        private void OnScreenShown(IScreenBody screen)
        {
            _signals.Incoming.ApplyHero.AddListener(_view.ShowHero);
            _signals.Incoming.ApplyEnemy.AddListener(_view.ShowEnemy);
            _signals.Incoming.ShowEnemyHit.AddListener(_view.ShowEnemyHit);
            _signals.Incoming.ShowHeroHit.AddListener(_view.ShowHeroHit);
            _signals.Incoming.ShowLog.AddListener(_view.AddLog);
            _signals.Incoming.ShowAnnouncement.AddListener(_view.Announce);

            _view.DragBegan += OnDragBegan;
            _view.Dragged += OnDragged;
        }

        private void OnScreenHidden(IScreenBody screen)
        {
            _signals.Incoming.ApplyHero.RemoveListener(_view.ShowHero);
            _signals.Incoming.ApplyEnemy.RemoveListener(_view.ShowEnemy);
            _signals.Incoming.ShowEnemyHit.RemoveListener(_view.ShowEnemyHit);
            _signals.Incoming.ShowHeroHit.RemoveListener(_view.ShowHeroHit);
            _signals.Incoming.ShowLog.RemoveListener(_view.AddLog);
            _signals.Incoming.ShowAnnouncement.RemoveListener(_view.Announce);

            _view.DragBegan -= OnDragBegan;
            _view.Dragged -= OnDragged;
        }

        private void OnDragBegan()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal))
                return;

            _internalSignals.WindowDragBegan.Dispatch();
        }

        private void OnDragged()
        {
            if (!_view.Data.HasState(ScreenState.AvailableToSendSignal))
                return;

            _internalSignals.WindowDragged.Dispatch();
        }
    }
}
