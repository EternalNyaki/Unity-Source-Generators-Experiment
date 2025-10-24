using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubClassEnumGenerator
{
    [Generator]
    public class EnumGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // Register syntax receiver factory
            context.RegisterForSyntaxNotifications(() => new SuperClassSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Retrieve populated SyntaxReceiver instance
            SuperClassSyntaxReceiver syntaxReceiver = (SuperClassSyntaxReceiver)context.SyntaxReceiver;

            // Get the recorded user classes
            List<ClassDeclarationSyntax> userClasses = syntaxReceiver.SuperClassesToEnumerate;
            if (userClasses == null || userClasses.Count < 0)
            {
                // If there are no recorded user classes, we don't need to do anything
                return;
            }

            Compilation compilation = context.Compilation;

            List<ClassDeclarationSyntax> nonPartialClasses = new List<ClassDeclarationSyntax>();
            foreach (var userClass in userClasses)
            {
                // Check if class is partial to ensure we can properly generate an extension to it
                bool isPartial = userClass.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
                if (!isPartial)
                {
                    // If class isn't partial, mark it for error logging and move to next class
                    nonPartialClasses.Add(userClass);
                    continue;
                }

                INamedTypeSymbol userClassSymbol = compilation.GetTypeByMetadataName(userClass.Identifier.ToString());

                List<INamedTypeSymbol> subClasses;
                string test = "test";
                AttributeData attribute = null;
                foreach (var attr in userClassSymbol.GetAttributes().ToArray())
                {
                    if (attr.AttributeClass.Name == "EnumerateChildrenAttribute")
                    {
                        attribute = attr;
                    }
                }

                bool arg = (bool)attribute.ConstructorArguments[0].Value;
                if (arg)
                {
                    subClasses = SubClassFinder.FindSubClasses(compilation, userClassSymbol);
                }
                else
                {
                    subClasses = SubClassFinder.FindImmediateSubClasses(compilation, userClassSymbol);
                }

                // Generate class extension
                var sourceBuilder = new StringBuilder($@"
using System;
using System.Reflection;

public partial class {userClass.Identifier}
{{
    public static string test = ""{test}"";

    public enum {userClass.Identifier}SubClass
    {{");

                foreach (var sc in subClasses)
                {
                    sourceBuilder.Append($@"
        {sc.Name},");
                }

                sourceBuilder.Append($@"
    }}

    public static Type GetTypeOfSubClass ({userClass.Identifier}SubClass subClass)
    {{
        switch(subClass)
        {{");
                foreach (var sc in subClasses)
                {
                    sourceBuilder.Append($@"
            case {userClass.Identifier}SubClass.{sc.Name}:
                return typeof({sc.Name});
                ");
                }

                sourceBuilder.Append($@"
            default:
                throw new ArgumentException(""Subclass "" + subClass.ToString() + "" does not exist"");
        }}
    }}

    public static {userClass.Identifier}SubClass GetSubClassFromType (Type type)
    {{");
                foreach (var sc in subClasses)
                {
                    sourceBuilder.Append($@"
        if(type == typeof({sc.Name})) {{ return {userClass.Identifier}SubClass.{sc.Name}; }}
                ");
                }
                sourceBuilder.Append($@"
        throw new ArgumentException(""Type "" + type.FullName + "" is not a subclass of {userClass.Identifier}"");
    }}
}}
");

                string sourceName = userClass.Identifier.ToString() + ".Generated.cs";
                context.AddSource(sourceName, SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }

            if (nonPartialClasses.Count > 0)
            {
                // If there were any invalid classes, throw an exception after all other classes have been generated
                StringBuilder exceptionMessage = new StringBuilder
                    ("The following classes have been marked for generation but not made partial: ");
                foreach (var c in nonPartialClasses)
                {
                    exceptionMessage.Append(c.Identifier.ToString());
                    exceptionMessage.Append(", ");
                }
                exceptionMessage.Append("make sure all classes marked for generation have the partial modifier");

                throw new InvalidOperationException(exceptionMessage.ToString());
            }
        }
    }

    public class SuperClassSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> SuperClassesToEnumerate { get; private set; }

        // Name of the attribute used to mark classes for generation
        private const string k_targetAttributeName = "EnumerateChildren";

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0)
            {
                if (SuperClassesToEnumerate == null)
                {
                    SuperClassesToEnumerate = new List<ClassDeclarationSyntax>();
                }

                foreach (var list in cds.AttributeLists)
                {
                    if (DoesAttributeListContainAttributeWithName(list, k_targetAttributeName))
                    {
                        SuperClassesToEnumerate.Add(cds);
                        break;
                    }
                }
            }
        }

        private bool DoesAttributeListContainAttributeWithName(AttributeListSyntax list, string name)
        {
            if (list.Attributes.Count < 0) { return false; }

            foreach (var attr in list.Attributes)
            {
                if (attr.Name.ToString() == name)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public static class SubClassFinder
    {
        public static List<INamedTypeSymbol> FindSubClasses(Compilation compilation, INamedTypeSymbol baseClass)
        {
            List<INamedTypeSymbol> subClasses = new List<INamedTypeSymbol>();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                SyntaxNode root = syntaxTree.GetRoot();
                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
                foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    var t = compilation.GetTypeByMetadataName(classDeclaration.Identifier.ToString());
                    if (t != null && IsSubClassOf(t, baseClass))
                    {
                        subClasses.Add(t);
                    }
                }
            }

            return subClasses;
        }

        /// <summary>
        /// Finds and returns a list of all immediate sub-classes of the given
        /// base class within the given compilation
        /// Only goes one level deep in the inheritance of potential sub-classes,
        /// so it won't identify sub-classes of sub-classes
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="baseClass"></param>
        /// <returns> A list of symbols representing all of the sub-classes </returns>
        public static List<INamedTypeSymbol> FindImmediateSubClasses(Compilation compilation, INamedTypeSymbol baseClass)
        {
            List<INamedTypeSymbol> subClasses = new List<INamedTypeSymbol>();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                SyntaxNode root = syntaxTree.GetRoot();
                SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
                foreach (var classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
                {
                    var t = compilation.GetTypeByMetadataName(classDeclaration.Identifier.ToString());
                    if (t != null && IsImmediateSubClassOf(t, baseClass))
                    {
                        subClasses.Add(t);
                    }
                }
            }

            return subClasses;
        }

        public static bool IsSubClassOf(INamedTypeSymbol potentialSubClass, INamedTypeSymbol baseClass)
        {
            INamedTypeSymbol currentBase = potentialSubClass.BaseType;

            while (currentBase != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentBase, baseClass))
                {
                    return true;
                }

                currentBase = currentBase.BaseType;
            }

            return false;
        }

        public static bool IsImmediateSubClassOf(INamedTypeSymbol potentialSubClass, INamedTypeSymbol baseClass)
        {
            INamedTypeSymbol immediateBase = potentialSubClass.BaseType;

            if (immediateBase == null) { return false; }

            return SymbolEqualityComparer.Default.Equals(immediateBase, baseClass);
        }
    }
}
