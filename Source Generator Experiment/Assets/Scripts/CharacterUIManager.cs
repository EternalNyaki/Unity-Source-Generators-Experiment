using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        foreach (var f in Character.GetTypeOfSubClass(characterClass).GetFields())
        {
            if (System.Array.Find(typeof(Character).GetFields(), field => field.Name == f.Name) != null) { continue; }

            if (f.FieldType == typeof(int))
            {
                CreateIntField(f.Name, value =>
                {
                    int n;
                    if (Int.TryParse(value, out n))
                    {
                        f.SetValue(character, n);
                    }
                });
            }
            else if (f.FieldType.BaseType == typeof(System.Enum))
            {
                TMP_Dropdown dropdown = CreateDropdownField(f.Name, f.FieldType, value => f.SetValue(character, value));
                dropdown.value = (int)f.GetValue(character);
            }
        }
    }

    public void OnClassChanged(int value)
    {
        Character.CharacterSubClass cClass = (Character.CharacterSubClass)value;
        System.Type cType = Character.GetTypeOfSubClass(cClass);
        if (character == null)
        {
            character = (Character)cType.GetConstructor(new System.Type[0]).Invoke(new object[0]);
        }
        else
        {
            character = (Character)cType.GetConstructor(new System.Type[1] { typeof(Character) }).Invoke(new object[1] { character });
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
            name = character.name,
            health = character.health,
            strength = character.strength,
            intelligence = character.intelligence,
            dexterity = character.dexterity
        };

        System.Type cType = character.GetType();
        data.characterClass = Character.GetSubClassFromType(cType);

        List<int> miscData = new List<Int>();
        foreach (var f in cType.GetFields())
        {
            if (System.Array.Find(typeof(Character).GetFields(), field => field.Name == f.Name) != null) { continue; }

            int value = (int)System.Convert.ChangeType(f.GetValue(character), f.FieldType);
            miscData.Add(value);
        }
        data.miscData = miscData.ToArray();

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

        System.Type cType = Character.GetTypeOfSubClass(data.characterClass);
        character = (Character)cType.GetConstructor(new System.Type[0]).Invoke(new object[0]);
        character.name = data.name;
        character.health = data.health;
        character.strength = data.strength;
        character.intelligence = data.intelligence;
        character.dexterity = data.dexterity;

        int i = 0;
        foreach (var f in cType.GetFields())
        {
            if (System.Array.Find(typeof(Character).GetFields(), field => field.Name == f.Name) != null) { continue; }

            if (i >= data.miscData.Length) { break; }

            if (f.FieldType.IsEnum)
            {
                object a = f.FieldType.GetEnumValues().GetValue(data.miscData[i]);
                f.SetValue(character, f.FieldType.GetEnumValues().GetValue(data.miscData[i]));
            }
            else
            {
                f.SetValue(character, data.miscData[i]);
            }

            i++;
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
