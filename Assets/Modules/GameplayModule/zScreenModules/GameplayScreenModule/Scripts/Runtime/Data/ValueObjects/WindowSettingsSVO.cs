using System;

namespace Modules.GameplayModule.GameplayScreenModule.Data.ValueObjects
{
    /// <summary>How the player left the window, as it is saved.</summary>
    [Serializable]
    public class WindowSettingsSVO
    {
        public float Opacity = 1f;
        public bool IsTopmost = true;
    }
}
