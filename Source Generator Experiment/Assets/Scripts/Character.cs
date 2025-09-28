using UnityEngine;

[System.Serializable, EnumerateChildren]
public abstract partial class Character
{
    public string name;
    public int health, strength, intelligence, dexterity;

    public Character() { }

    public Character(Character character)
    {
        name = character.name;
        health = character.health;
        strength = character.strength;
        intelligence = character.intelligence;
        dexterity = character.dexterity;
    }
}


public struct SerializableCharacter
{
    public Character.CharacterSubClass characterClass;
    public string name;
    public int health, strength, intelligence, dexterity;
    public int miscInt1, miscInt2;
}
