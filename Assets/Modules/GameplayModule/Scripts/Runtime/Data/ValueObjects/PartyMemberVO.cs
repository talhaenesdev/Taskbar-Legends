namespace Modules.GameplayModule.Data.ValueObjects
{
    /// <summary>A party member in play: its config entry and its HP.</summary>
    internal class PartyMemberVO
    {
        public PartyMemberCVO Config;
        public int Hp;

        public bool IsDown => Hp <= 0;
    }
}
