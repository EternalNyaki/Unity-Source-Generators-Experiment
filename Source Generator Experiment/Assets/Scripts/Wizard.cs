using UnityEngine;

[System.Serializable]
public class Wizard : Character
{
    public Catalyst catalyst;
    public Affinity affinity;

    public Wizard() : base() { }

    public Wizard(Character character) : base(character) { }
}
