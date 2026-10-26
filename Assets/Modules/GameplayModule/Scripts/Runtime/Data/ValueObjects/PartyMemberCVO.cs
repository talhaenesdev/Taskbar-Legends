using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>One member of the party: its share of the party's HP and attack.</summary>
    [Serializable]
    public class PartyMemberCVO
    {
        public PartyRole Role;
        public string Name = "Warrior";
        public float HpShare = 1f;
        public float AttackShare = 1f;
    }
}
