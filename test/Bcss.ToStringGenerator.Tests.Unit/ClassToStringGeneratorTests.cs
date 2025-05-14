using System.Text;
using Bcss.ToStringGenerator.Attributes;
using Bcss.ToStringGenerator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bcss.ToStringGenerator.Tests.Unit
{
    [TestClass]
    public class ClassToStringGeneratorTests
    {
        [TestMethod]
        public void GenerateToString_WithPublicProperties_GeneratesCorrectToString()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

[GenerateToString]
public partial class TestClass
{
    public string Name { get; set; }
    public int Age { get; set; }
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClass.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("public override string ToString()"), "Generated code should contain ToString method");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"[TestClass: \")"), "Generated code should contain class name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should contain property value");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Age.ToString())"), "Generated code should contain property value");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"]\")"), "Generated code should contain closing bracket");
        }

        [TestMethod]
        public void GenerateToString_WithSensitiveField_ExcludesSensitiveField()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

[GenerateToString]
public partial class TestClass
{
    public string Name { get; set; }
    [SensitiveData]
    public string Password { get; set; }
    public int Age { get; set; }
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClass.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Password = \")"), "Generated code should contain sensitive property name");
            Assert.IsTrue(generatedCode.Contains("[REDACTED]"), "Generated code should contain redaction value");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name");
        }

        [TestMethod]
        public void GenerateToString_WithPublicFields_IncludesFields()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

[GenerateToString]
public partial class TestClass
{
    public string Name;
    public int Age;
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClass.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain field name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should contain field value");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain field name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Age.ToString())"), "Generated code should contain field value");
        }

        [TestMethod]
        public void GenerateToString_WithoutAttribute_DoesNotGenerateToString()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

public partial class TestClass
{
    public string Name { get; set; }
    public int Age { get; set; }
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClass.ToString.g.cs"));
            Assert.IsNull(generatedSyntax, "No syntax tree should be generated for class without attribute");
        }

        [TestMethod]
        public void GenerateToString_WithCustomRedactionValue_UsesCustomValue()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

[GenerateToString]
public partial class TestClass
{
    public string Name { get; set; }
    [SensitiveData(""***SECRET***"")]
    public string Password { get; set; }
    public int Age { get; set; }
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClass.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Password = \")"), "Generated code should contain sensitive property name");
            Assert.IsTrue(generatedCode.Contains("***SECRET***"), "Generated code should contain custom redaction value");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name");
        }

        [TestMethod]
        public void ToString_WithEnumerable_FormatsCorrectly()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;
using System.Collections.Generic;

[GenerateToString]
public partial class EnumerableExample
{
    public List<int> Numbers { get; set; } = new() { 1, 2, 3 };
    public List<string> Strings { get; set; } = new() { ""a"", ""b"", ""c"" };
    public List<string> EmptyList { get; set; } = new();
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("EnumerableExample.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Numbers = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Strings = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", EmptyList = \")"), "Generated code should contain property name");
        }

        [TestMethod]
        public void ToString_WithDictionary_FormatsCorrectly()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;
using System.Collections.Generic;

[GenerateToString]
public partial class DictionaryExample
{
    public Dictionary<string, int> Scores { get; set; } = new()
    {
        { ""Alice"", 100 },
        { ""Bob"", 90 }
    };
    public Dictionary<string, int> EmptyDict { get; set; } = new();
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("DictionaryExample.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Scores = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("EmptyDict = \")"), "Generated code should contain property name");
        }

        [TestMethod]
        public void ToString_WithMixedCollections_FormatsCorrectly()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;
using System.Collections.Generic;

[GenerateToString]
public partial class MixedCollectionsExample
{
    public List<int> Numbers { get; set; } = new() { 1, 2, 3 };
    public Dictionary<string, int> Scores { get; set; } = new()
    {
        { ""Alice"", 100 },
        { ""Bob"", 90 }
    };
    public string Name { get; set; } = ""Test"";
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("MixedCollectionsExample.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Numbers = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Scores = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Name = \")"), "Generated code should contain property name");
        }

        [TestMethod]
        public void ToString_WithSensitiveCollections_RedactsCorrectly()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;
using System.Collections.Generic;

[GenerateToString]
public partial class SensitiveCollectionsExample
{
    public List<int> Numbers { get; set; } = new() { 1, 2, 3 };
    [SensitiveData(""***"")]
    public Dictionary<string, string> Secrets { get; set; } = new()
    {
        { ""key1"", ""value1"" },
        { ""key2"", ""value2"" }
    };
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var generatedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("SensitiveCollectionsExample.ToString.g.cs"));
            Assert.IsNotNull(generatedSyntax, "Generated syntax tree should not be null");

            var generatedCode = generatedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Numbers = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Secrets = \")"), "Generated code should contain property name");
            Assert.IsTrue(generatedCode.Contains("\"***\""), "Generated code should contain redaction value");
        }

        private static Compilation CreateCompilation(string source)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(source);
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GenerateToStringAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Dictionary<,>).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(StringBuilder).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)
            };

            return CSharpCompilation.Create(
                "TestAssembly",
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }
    }
} 