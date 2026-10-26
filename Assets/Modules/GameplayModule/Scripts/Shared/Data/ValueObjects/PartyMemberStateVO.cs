using System;
using Modules.GameplayModule.Shared.Enums;

namespace Modules.GameplayModule.Shared.Data.ValueObjects
{
    /// <summary>One party member as a screen draws it.</summary>
    [Serializable]
    public class PartyMemberStateVO
    {
        public PartyRole Role;
        public string Name;
        public int Hp;
        public int MaxHp;
    }
}
