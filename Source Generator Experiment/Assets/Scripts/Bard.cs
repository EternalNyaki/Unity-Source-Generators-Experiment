using UnityEngine;

[System.Serializable]
public class Bard : Character
{
    public int charisma;

    public Bard() : base() { }

    public Bard(Character character) : base(character) { }
}
