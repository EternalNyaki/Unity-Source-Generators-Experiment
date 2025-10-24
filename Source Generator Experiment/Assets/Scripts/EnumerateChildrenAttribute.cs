using UnityEngine;

/// <summary>
/// Attribute to mark a class for whom the source generator should generate an
/// enum from its children
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
public sealed class EnumerateChildrenAttribute : System.Attribute
{
    public bool shouldSearchRecursively;

    public EnumerateChildrenAttribute(bool shouldSearchRecursively)
    {
        this.shouldSearchRecursively = shouldSearchRecursively;
    }
}
