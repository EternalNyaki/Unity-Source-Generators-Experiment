using UnityEngine;

public class HelloFromSourceGenerator : MonoBehaviour
{
    private static string GetStringFromSourceGenerator()
    {
        return ExampleSourceGenerated.ExampleSourceGenerated.GetTestText();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var output = "Test";
        output = GetStringFromSourceGenerator();
        Debug.Log(output);
    }
}
