using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Int = System.Int32;

public class CharacterUIManager : MonoBehaviour
{
    public TMP_Dropdown classDropdown;
    public TMP_InputField healthField, strengthField, intelligenceField, dexterityField;

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

    private TMP_InputField CreateIntField(string name, UnityAction<string> onValueChanged)
    {
        GameObject field = Instantiate(intFieldPrefab, infoRegion);
        field.transform.Find("Label").GetComponent<TMP_Text>().text = name + ":";

        TMP_InputField inputField = field.GetComponentInChildren<TMP_InputField>();
        inputField.onValueChanged.AddListener(onValueChanged);
        return inputField;
    }

    private TMP_Dropdown CreateDropdownField(string name, System.Type enumType, UnityAction<int> onValueChanged)
    {
        GameObject field = Instantiate(dropdownPrefab, infoRegion);
        field.transform.Find("Label").GetComponent<TMP_Text>().text = name + ":";

        TMP_Dropdown dropdown = field.GetComponentInChildren<TMP_Dropdown>();
        PopulateDropdownFromEnum(dropdown, enumType);
        dropdown.onValueChanged.AddListener(onValueChanged);
        return dropdown;
    }

    private void CreateClassFields(Character.CharacterSubClass characterClass)
    {
        foreach (Transform child in infoRegion)
        {
            Destroy(child.gameObject);
        }

        switch (characterClass)
        {
            case Character.CharacterSubClass.Rogue:
                // Create info fields
                // HACK: Ideally this would be done dynamically through reflection, but that's not my focus rn
                CreateDropdownField("Weapon", typeof(OneHandedWeapon), value => ((Rogue)character).weapon = (OneHandedWeapon)value).value = (int)((Rogue)character).weapon;
                break;

            case Character.CharacterSubClass.Wizard:
                // Create info fields
                // HACK: Ideally this would be done dynamically through reflection, but that's not my focus rn
                CreateDropdownField("Catalyst", typeof(Catalyst), value => ((Wizard)character).catalyst = (Catalyst)value).value = (int)((Wizard)character).catalyst;
                CreateDropdownField("Affinity", typeof(Affinity), value => ((Wizard)character).affinity = (Affinity)value).value = (int)((Wizard)character).affinity;
                break;

            case Character.CharacterSubClass.Paladin:
                // Create info fields
                // HACK: Ideally this would be done dynamically through reflection, but that's not my focus rn
                CreateIntField("Faith", value =>
                {
                    int n;
                    if (Int.TryParse(value, out n))
                    {
                        ((Paladin)character).faith = n;
                    }
                }).text = ((Paladin)character).faith.ToString();
                CreateDropdownField("Weapon", typeof(TwoHandedWeapon), value => ((Paladin)character).weapon = (TwoHandedWeapon)value).value = (int)((Paladin)character).weapon;
                break;
        }
    }

    public void OnClassChanged(int value)
    {
        Character.CharacterSubClass cClass = (Character.CharacterSubClass)value;
        switch (cClass)
        {
            case Character.CharacterSubClass.Rogue:
                character = character == null ? new Rogue() : new Rogue(character);
                break;

            case Character.CharacterSubClass.Wizard:
                character = character == null ? new Wizard() : new Wizard(character);
                break;

            case Character.CharacterSubClass.Paladin:
                character = character == null ? new Paladin() : new Paladin(character);
                break;
        }

        CreateClassFields(cClass);
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

    public void SaveToJSON()
    {
        SerializableCharacter data = new SerializableCharacter
        {
            characterClass = default,
            name = character.name,
            health = character.health,
            strength = character.strength,
            intelligence = character.intelligence,
            dexterity = character.dexterity,
            miscInt1 = default,
            miscInt2 = default
        };

        //HACK: Yet another thing that could be done dynamically through reflection
        if (character is Rogue rogue)
        {
            data.characterClass = Character.CharacterSubClass.Rogue;
            data.miscInt1 = (int)rogue.weapon;
        }
        else if (character is Wizard wizard)
        {
            data.characterClass = Character.CharacterSubClass.Wizard;
            data.miscInt1 = (int)wizard.catalyst;
            data.miscInt2 = (int)wizard.affinity;
        }
        else if (character is Paladin paladin)
        {
            data.characterClass = Character.CharacterSubClass.Paladin;
            data.miscInt1 = paladin.faith;
            data.miscInt2 = (int)paladin.weapon;
        }

        string json = JsonUtility.ToJson(data);
        string path = Application.dataPath + "/Characters/" + data.name + ".json";
        File.WriteAllText(path, json);
        if (File.Exists(path))
        {
            Debug.Log("Character data saved to " + path);
        }
        else
        {
            Debug.Log("Saving failed");
        }
    }

    public void LoadFromJSON()
    {
        string path = Application.dataPath + "/Characters/" + character.name + ".json";
        string json = File.ReadAllText(path);
        SerializableCharacter data = JsonUtility.FromJson<SerializableCharacter>(json);

        switch (data.characterClass)
        {
            case Character.CharacterSubClass.Rogue:
                character = new Rogue
                {
                    name = data.name,
                    health = data.health,
                    strength = data.strength,
                    intelligence = data.intelligence,
                    dexterity = data.dexterity,
                    weapon = (OneHandedWeapon)data.miscInt1
                };
                break;

            case Character.CharacterSubClass.Wizard:
                character = new Wizard
                {
                    name = data.name,
                    health = data.health,
                    strength = data.strength,
                    intelligence = data.intelligence,
                    dexterity = data.dexterity,
                    catalyst = (Catalyst)data.miscInt1,
                    affinity = (Affinity)data.miscInt2
                };
                break;

            case Character.CharacterSubClass.Paladin:
                character = new Paladin
                {
                    name = data.name,
                    health = data.health,
                    strength = data.strength,
                    intelligence = data.intelligence,
                    dexterity = data.dexterity,
                    faith = data.miscInt1,
                    weapon = (TwoHandedWeapon)data.miscInt2
                };
                break;
        }

        LoadFieldValues(data.characterClass);
    }

    private void LoadFieldValues(Character.CharacterSubClass characterClass)
    {
        classDropdown.SetValueWithoutNotify((int)characterClass);
        healthField.text = character.health.ToString();
        strengthField.text = character.strength.ToString();
        intelligenceField.text = character.intelligence.ToString();
        dexterityField.text = character.dexterity.ToString();
        CreateClassFields(characterClass);
    }
}
