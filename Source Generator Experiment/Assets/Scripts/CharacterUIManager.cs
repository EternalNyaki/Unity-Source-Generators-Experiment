using UnityEngine;

using Int = System.Int32;

public class CharacterUIManager : MonoBehaviour
{
    public Character character;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        character = new Character();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnNameChanged(string value)
    {
        character.name = value;
    }

    public void OnHealthChanged(string value)
    {
        int n;
        if (Int.TryParse(value, out n))
        {
            character.health = n;
        }
    }

    public void OnStrengthChanged(string value)
    {
        int n;
        if (Int.TryParse(value, out n))
        {
            character.strength = n;
        }
    }

    public void OnIntelligenceChanged(string value)
    {
        int n;
        if (Int.TryParse(value, out n))
        {
            character.intelligence = n;
        }
    }

    public void OnDexterityChanged(string value)
    {
        int n;
        if (Int.TryParse(value, out n))
        {
            character.dexterity = n;
        }
    }
}
