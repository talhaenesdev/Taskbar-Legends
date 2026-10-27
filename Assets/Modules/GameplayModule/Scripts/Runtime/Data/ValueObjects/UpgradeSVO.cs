using System;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>How many times an upgrade has been picked, as it is saved.</summary>
    [Serializable]
    public class UpgradeSVO
    {
        public string Id;
        public int Count;
    }
}
