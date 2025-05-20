using System.Text;
using Bcss.ToStringGenerator.Attributes;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bcss.ToStringGenerator.Tests.Unit
{
    [TestClass]
    public class ClassToStringGeneratorTests
    {
        [TestMethod]
        public void _Caching_TestGeneratorProperlyCreatesCacheableResults()
        {
            const string input = @"
using Bcss.ToStringGenerator.Attributes;
using System.Collections.Generic;

public class ChildClass
{
    public string ChildStr;
}

[GenerateToString]
public partial class SensitiveCollectionsExample
{
    public int? NullInt;
    public string TestString;

    public ChildClass Child;
    
    public List<int> Numbers { get; set; } = new() { 1, 2, 3 };
    [SensitiveData(""""***"""")]
    public Dictionary<string, string> Secrets { get; set; } = new()
    {
        { """"key1"""", """"value1"""" },
        { """"key2"""", """"value2"""" }
    };
}
";
            const string expected = @"
#if !TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES
namespace Bcss.ToStringGenerator.Attributes
{
    /// <summary>
    /// <p>Generates a ToString() method for the marked class or struct at compile time.</p>
    /// <p>By default, the string will be in the format:</p>
    /// <code>[className: member1Name = member1value, member2Name = member2value, ... ]</code>
    /// <br />
    /// <p>Collection members that implement IEnumerable or IEnumerableT will have each element written in square brackets, comma separated.</p>
    /// <code>[className: collectionMember = [value1, value2, value3] ... ]</code>
    /// <br />
    /// <p>Dictionary members that implement IDictionary or DictionaryT1, T2 will have each key-value pair written in brackets, comma separated.</p>
    /// <code>[className: dictionaryMember = [{key1 = value1}, {key2 = value2}] ... ]</code>
    /// <br />
    /// </summary>
    /// <remarks>
    /// <p>This attribute will be automatically loaded at compile time by the ToString source generator. You should not need to reference
    /// the project containing this attribute directly.</p>
    /// <br />
    /// <p>If your project exposes internal classes via [InternalsVisibleTo] and you reference the source generator package in multiple
    /// projects in one solution, you may end up with duplicate class definitions due to multiple generators being invoked. If this occurs,
    /// define the following constant in your projects .csproj file, then add a direct reference to the <c>Bcss.ToStringGenerator.Attributes</c>
    /// nuget package.</p>
    /// <br />
    /// <code><DefineConstants>TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES</DefineConstants></code>
    /// <br />
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class GenerateToStringAttribute : Attribute
    {
    }
}
#endif

#if !TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES
namespace Bcss.ToStringGenerator.Attributes
{
    /// <summary>
    /// An attribute for marking a field or property as sensitive. Sensitive members will not have their value output
    /// in the generated ToString() method. Instead, the provided `maskingValue` string is written. If no value is given,
    /// a default value is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SensitiveDataAttribute : Attribute
    {
        /// <summary>
        /// Gets the masking value to be used for the member marked with this attribute.
        /// </summary>
        public string MaskingValue { get; }

        /// <summary>
        /// Masks the value of the field or property that is marked with this attribute when
        /// generating a ToString() method.
        /// </summary>
        /// <remarks>
        /// <p>This attribute will be automatically loaded at compile time by the ToString source generator. You should not need to reference
        /// the project containing this attribute directly.</p>
        /// <br />
        /// <p>If your project exposes internal classes via [InternalsVisibleTo] and you reference the source generator package in multiple
        /// projects in one solution, you may end up with duplicate class definitions due to multiple generators being invoked. If this occurs,
        /// define the following constant in your projects .csproj file, then add a direct reference to the <c>Bcss.ToStringGenerator.Attributes</c>
        /// nuget package.</p>
        /// <br />
        /// <code><DefineConstants>TO_STRING_GENERATOR_EXCLUDE_GENERATED_ATTRIBUTES</DefineConstants></code>
        /// <br />
        /// </remarks>
        /// <param name=""maskingValue"">Sets the value that will be used in the ToString output instead of the members actual value.</param>
        public SensitiveDataAttribute(string maskingValue = ""[REDACTED]"")
        {
            MaskingValue = maskingValue;
        }
    }
}
#endif
using System;
using System.Text;
using System.Collections.Generic;

namespace <global namespace>;

public partial class SensitiveCollectionsExample
{
   public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(""[SensitiveCollectionsExample: "");

        sb.Append(""NullInt = "");
        if (NullInt == null)
        {
            sb.Append(""null"");
        }
        else
        {
            sb.Append(NullInt.ToString());
        }
        sb.Append("", TestString = "");
        if (TestString == null)
        {
            sb.Append(""null"");
        }
        else
        {
            sb.Append(TestString.ToString());
        }
        sb.Append("", Child = "");
        if (Child == null)
        {
            sb.Append(""null"");
        }
        else
        {
            sb.Append(Child.ToString());
        }
        sb.Append("", Numbers = "");
        if (Numbers == null)
        {
            sb.Append(""null"");
        }
        else
        {
            sb.Append('[');
            var NumbersEnumerator = Numbers.GetEnumerator();
            if (NumbersEnumerator.MoveNext())
            {
                sb.Append(NumbersEnumerator.Current.ToString());

                while (NumbersEnumerator.MoveNext())
                {
                    sb.Append("", "");
                    sb.Append(NumbersEnumerator.Current.ToString());
                }
            }
            sb.Append(']');
        }
        sb.Append("", Secrets = "");
        sb.Append(""[REDACTED]"");

        sb.Append(""]"");
        return sb.ToString();
    }
}
";

            // run the generator, passing in the inputs and the tracking names
            var (diagnostics, output) 
                = TestHelpers.GetGeneratedTrees<ClassToStringGenerator>([input]);

            // Assert the output
            diagnostics.Should()!.BeEmpty();
            string fullOutput = string.Join(Environment.NewLine, output);
            fullOutput.Should()!.BeEquivalentTo(expected);
        }
        
        [TestMethod]
        public void GenerateToString_WithPublicProperties_GeneratesCorrectToString()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[GenerateToString]
public partial class TestClass
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class NoAttr1{}

[TestClass]
public class NoAttr2{}";

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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name `Name`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should call ToString() on `Name` property");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name `Age`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Age.ToString())"), "Generated code should  call ToString() on `Age` property");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"]\")"), "Generated code should contain closing bracket");
        }
        
        [TestMethod]
        public void GenerateToString_Generates_ToString_For_Multiple_Classes()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[GenerateToString]
public partial class TestClass
{
    public string Name { get; set; }
    public int Age { get; set; }
}

[GenerateToString]
public partial class TestClassTwo
{
    public string Name { get; set; }
}";

            var compilation = CreateCompilation(source);
            var generator = new ClassToStringGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

            // Assert
            var testClassGeneratedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClass.ToString.g.cs"));
            Assert.IsNotNull(testClassGeneratedSyntax, "Generated syntax tree should not be null");

            var generatedCode = testClassGeneratedSyntax.ToString();
            Assert.IsTrue(generatedCode.Contains("public override string ToString()"), "Generated code should contain ToString method");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"[TestClass: \")"), "Generated code should contain class name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name `Name`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should call ToString() on `Name` property");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name `Age`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Age.ToString())"), "Generated code should  call ToString() on `Age` property");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"]\")"), "Generated code should contain closing bracket");
            
            // Assert
            var testClassTwoGeneratedSyntax = outputCompilation.SyntaxTrees
                .FirstOrDefault(st => st.FilePath.EndsWith("TestClassTwo.ToString.g.cs"));
            Assert.IsNotNull(testClassTwoGeneratedSyntax, "Generated syntax tree two should not be null");

            var generatedCodeTwo = testClassTwoGeneratedSyntax.ToString();
            Assert.IsTrue(generatedCodeTwo.Contains("public override string ToString()"), "Generated code should contain ToString method");
            Assert.IsTrue(generatedCodeTwo.Contains("sb.Append(\"[TestClassTwo: \")"), "Generated code should contain class name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name `Name`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should call ToString() on `Name` property");
            Assert.IsTrue(generatedCodeTwo.Contains("sb.Append(\"]\")"), "Generated code should contain closing bracket");
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
    public bool Name { get; set; }
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name `Name`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should call ToString() on `Name` property");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name `Age`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Age.ToString())"), "Generated code should call ToString() on `Age` property");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Password = \")"), "Generated code should contain sensitive property name");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"[REDACTED]\")"), "Generated code should redact sensitive values with default redaction value");
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain field name `Name`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should call ToString() on `Name` field");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain field name `Age`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Age.ToString())"), "Generated code should  call ToString() on `Age` field");
        }
        
        [TestMethod]
        public void GenerateToString_WithNullableFields_IncludesNullCheck()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

[GenerateToString]
public partial class TestClass
{
    public string? Name;
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain field name `Name`");
            Assert.IsTrue(generatedCode.Contains("if (Name == null)"), "Generated code should contain if check for nullability");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"null\")"), "Generated code should contain value of null for when field is null");
            Assert.IsTrue(generatedCode.Contains("sb.Append(Name.ToString())"), "Generated code should contain field value for when field is not null");
        }
        
        [TestMethod]
        public void GenerateToString_WithNullableReferenceFields_IncludesNullCheck()
        {
            // Arrange
            var source = @"
using Bcss.ToStringGenerator.Attributes;

public class SubClass {
    public int count = 1;
}
[GenerateToString]
public partial class TestClass
{
    public SubClass SubClass;
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"SubClass = \")"), "Generated code should contain field name");
            Assert.IsTrue(generatedCode.Contains("if (SubClass == null)"), "Generated code should contain if check for nullability");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"null\")"), "Generated code should contain value of null for when field is null");
            Assert.IsTrue(generatedCode.Contains("sb.Append(SubClass.ToString())"), "Generated code should contain field value for when field is not null");
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Name = \")"), "Generated code should contain property name `Name`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Password = \")"), "Generated code should contain sensitive property name `Password`");
            Assert.IsTrue(generatedCode.Contains("***SECRET***"), "Generated code should contain custom redaction value");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Age = \")"), "Generated code should contain property name `Age`");
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Numbers = \")"), "Generated code should contain property `Numbers`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Strings = \")"), "Generated code should contain property `Strings`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", EmptyList = \")"), "Generated code should contain property `EmptyList`");
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Scores = \")"), "Generated code should contain property `Scores`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", EmptyDict = \")"), "Generated code should contain property EmptyDict");
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
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"Numbers = \""), "Generated code should contain property name `Numbers`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\", Secrets = \")"), "Generated code should contain property name `Secrets`");
            Assert.IsTrue(generatedCode.Contains("sb.Append(\"***\")"), "Generated code should contain custom redaction value");
        }

        private static CSharpCompilation CreateCompilation(string source)
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