using UnityEngine;

public class Paladin : Character
{
    public int faith;
    public TwoHandedWeapon weapon;

    public Paladin() : base() { }

    public Paladin(Character character) : base(character) { }
}
