using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>A boss raised as a shadow soldier, as it is saved.</summary>
    [Serializable]
    public class ShadowSVO
    {
        public string Name;
        public EnemyLook Look;
    }
}
