using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Int = System.Int32;

public class CharacterUIManager : MonoBehaviour
{
    public TMP_Dropdown classDropdown;
    public RectTransform infoRegion;

    public GameObject intFieldPrefab;
    public GameObject dropdownPrefab;

    [HideInInspector] public Character character;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulateDropdownFromEnum(classDropdown, typeof(Character.CharacterSubClass));

        OnClassChanged((int)default(Character.CharacterSubClass));
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void PopulateDropdownFromEnum(TMP_Dropdown dropdown, System.Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new System.ArgumentException("Type must be an enum");
        }

        dropdown.ClearOptions();

        List<string> valuesList = new List<string>();
        foreach (string v in System.Enum.GetNames(enumType))
        {
            valuesList.Add(v);
        }
        dropdown.AddOptions(valuesList);
    }

    private void CreateIntField(string name, UnityAction<string> onValueChanged)
    {
        GameObject field = Instantiate(intFieldPrefab, infoRegion);
        field.transform.Find("Label").GetComponent<TMP_Text>().text = name + ":";
        field.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(onValueChanged);
    }

    private void CreateDropdownField(string name, System.Type enumType, UnityAction<int> onValueChanged)
    {
        GameObject field = Instantiate(dropdownPrefab, infoRegion);
        field.transform.Find("Label").GetComponent<TMP_Text>().text = name + ":";
        TMP_Dropdown dropdown = field.GetComponentInChildren<TMP_Dropdown>();
        PopulateDropdownFromEnum(dropdown, enumType);
        dropdown.onValueChanged.AddListener(onValueChanged);
    }

    public void OnClassChanged(int value)
    {
        foreach (Transform child in infoRegion)
        {
            Destroy(child.gameObject);
        }

        Character.CharacterSubClass cClass = (Character.CharacterSubClass)value;
        switch (cClass)
        {
            case Character.CharacterSubClass.Rogue:
                character = character == null ? new Rogue() : new Rogue(character);

                // Create info fields
                // HACK: Ideally this would be done dynamically through reflection, but that's not my focus rn
                CreateDropdownField("Weapon", typeof(OneHandedWeapon), value => ((Rogue)character).weapon = (OneHandedWeapon)value);
                break;

            case Character.CharacterSubClass.Wizard:
                character = character == null ? new Wizard() : new Wizard(character);

                // Create info fields
                // HACK: Ideally this would be done dynamically through reflection, but that's not my focus rn
                CreateDropdownField("Catalyst", typeof(Catalyst), value => ((Wizard)character).catalyst = (Catalyst)value);
                CreateDropdownField("Affinity", typeof(Affinity), value => ((Wizard)character).affinity = (Affinity)value);
                break;

            case Character.CharacterSubClass.Paladin:
                character = character == null ? new Paladin() : new Paladin(character);

                // Create info fields
                // HACK: Ideally this would be done dynamically through reflection, but that's not my focus rn
                CreateIntField("Faith", value =>
                {
                    int n;
                    if (Int.TryParse(value, out n))
                    {
                        ((Paladin)character).faith = n;
                    }
                });
                CreateDropdownField("Weapon", typeof(TwoHandedWeapon), value => ((Paladin)character).weapon = (TwoHandedWeapon)value);
                break;
        }
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
