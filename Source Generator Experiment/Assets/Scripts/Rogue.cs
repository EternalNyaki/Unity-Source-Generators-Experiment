using UnityEngine;

public class Rogue : Character
{
    public OneHandedWeapon weapon;

    public Rogue() : base() { }

    public Rogue(Character character) : base(character) { }
}
