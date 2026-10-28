using Modules.GameplayModule.GameplayScreenModule.Data.ValueObjects;

namespace Modules.GameplayModule.GameplayScreenModule.Models
{
    /// <summary>How the player wants the window: how see-through, and whether it stays on top. Kept between sessions.</summary>
    internal interface IWindowSettingsModel
    {
        float Opacity { get; }
        bool IsTopmost { get; }

        /// <summary>Steps to the next opacity of the cycle, from fully opaque down and round again.</summary>
        void CycleOpacity();

        void ToggleTopmost();

        WindowSettingsVO GetState(bool startsWithWindows);
        void Save();
    }
}
