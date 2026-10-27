using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>One card of a level-up choice, as a screen draws it.</summary>
    [Serializable]
    public class UpgradeOptionVO
    {
        public UpgradeKind Kind;

        /// <summary>The member the card is about; meaningless when AllRoles is set.</summary>
        public PartyRole Role;
        public bool AllRoles;

        public string Title;
        public string Description;
    }
}
