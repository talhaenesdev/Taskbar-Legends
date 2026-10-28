using FlowIoC.BaseModule.Constructables;
using Modules.GameplayModule.GameplayScreenModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.GameplayModule.GameplayScreenModule.Models
{
    /// <summary>Read from PlayerPrefs in PostConstruct; an opacity off the cycle is moved onto the nearest step.</summary>
    internal class WindowSettingsModel : IWindowSettingsModel, IConstructable
    {
        private const string SAVE_KEY = "TaskbarLegends.Window";

        private static readonly float[] OpacitySteps = { 1f, 0.85f, 0.7f, 0.55f, 0.4f };

        private WindowSettingsSVO _save;

        public bool IsPostConstructed { get; set; }
        public bool IsDeconstructed { get; set; }

        public float Opacity => _save.Opacity;
        public bool IsTopmost => _save.IsTopmost;

        public void PostConstruct()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, string.Empty);
            _save = (string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<WindowSettingsSVO>(json)) ?? new WindowSettingsSVO();
            _save.Opacity = OpacitySteps[NearestStep(_save.Opacity)];
        }

        public void Deconstruct() => _save = null;

        public void CycleOpacity() => _save.Opacity = OpacitySteps[(NearestStep(_save.Opacity) + 1) % OpacitySteps.Length];

        public void ToggleTopmost() => _save.IsTopmost = !_save.IsTopmost;

        public WindowSettingsVO GetState(bool startsWithWindows) =>
            new() { Opacity = _save.Opacity, IsTopmost = _save.IsTopmost, StartsWithWindows = startsWithWindows };

        public void Save()
        {
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(_save));
            PlayerPrefs.Save();
        }

        private static int NearestStep(float opacity)
        {
            int nearest = 0;
            for (int i = 1; i < OpacitySteps.Length; i++)
            {
                if (Mathf.Abs(OpacitySteps[i] - opacity) < Mathf.Abs(OpacitySteps[nearest] - opacity))
                    nearest = i;
            }

            return nearest;
        }
    }
}
