using UnityEngine;

public class HelloFromSourceGenerator : MonoBehaviour
{
    private static string GetStringFromSourceGenerator()
    {
        return TestBaseClass.test;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var output = "";
        output = GetStringFromSourceGenerator();
        foreach (var s in System.Enum.GetNames(typeof(TestBaseClass.TestBaseClassSubClass)))
        {
            output += s + " ";
        }
        Debug.Log(output);
    }
}
