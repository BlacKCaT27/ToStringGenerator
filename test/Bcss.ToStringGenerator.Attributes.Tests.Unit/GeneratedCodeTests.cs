// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PartialTypeWithSinglePart

using Bcss.ToStringGenerator.TestData;

namespace Bcss.ToStringGenerator.Attributes.Tests.Unit
{
    /// <summary>
    /// These tests act as a set of "client" tests for the generator library. Rather than testing the output of the source generator,
    /// these tests assert on the output of the generated code itself.
    /// </summary>
    [TestClass]
    public class GeneratedCodeTests
    {
        /// <summary>
        /// Sanity check test to ensure that the generator is actually generating code.
        /// Relies on a specific path to another test project, may need tweaking/removal/ignoring in builds.
        /// </summary>
        [TestMethod]
        public void Generator_ProducesGeneratedCode()
        {
            var _ = new GenerateToStringTestClass();
            // Arrange
            var generatedFilesPath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "Bcss.ToStringGenerator.TestData", "obj", 
                "Debug",
                "net9.0",
                "Generated");

            // Act
            var generatedFiles = Directory.GetFiles(generatedFilesPath, "*.g.cs", SearchOption.AllDirectories);

            // Assert
            Assert.IsTrue(generatedFiles.Any(), "No generated files found");
            var testClassFile = generatedFiles.FirstOrDefault(f => f.Contains("GenerateToStringTestClass.ToString.g.cs"));
            Assert.IsNotNull(testClassFile, "Generated file for GenerateToStringTestClass not found");
            
            var generatedCode = File.ReadAllText(testClassFile);
            Assert.IsTrue(generatedCode.Contains("public override string ToString()"), "Generated code should contain ToString method");
        }
        
        [TestMethod]
        public void ToString_WithBasicProperties_FormatsCorrectly()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = "John Doe",
                Age = 30,
                IsActive = true,
                SubClass = new SubClass
                {
                    IsActive = false
                }
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = John Doe, Age = 30, IsActive = True, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = [SubClass: IsActive = False], Numbers = [], Scores = [], Secrets = ***]", result);
        }

        [TestMethod]
        public void ToString_WithSensitiveData_RedactsSensitiveProperties()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = "John Doe",
                Password = "secret123",
                Age = 30
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = John Doe, Age = 30, IsActive = False, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = null, Numbers = [], Scores = [], Secrets = ***]", result);
        }

        [TestMethod]
        public void ToString_WithCustomSensitiveData_RedactsWithCustomValue()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = "John Doe",
                SecretKey = "abc123",
                Age = 30
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = John Doe, Age = 30, IsActive = False, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = null, Numbers = [], Scores = [], Secrets = ***]", result);
        }

        [TestMethod]
        public void ToString_WithCollections_FormatsCollectionsCorrectly()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = "John Doe",
                Numbers = new List<int> { 1, 2, 3 },
                Scores = new Dictionary<string, int>
                {
                    { "Alice", 100 },
                    { "Bob", 90 }
                }
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = John Doe, Age = 0, IsActive = False, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = null, Numbers = [1, 2, 3], Scores = [{Alice, 100}, {Bob, 90}], Secrets = ***]", result);
        }

        [TestMethod]
        public void ToString_WithEmptyCollections_FormatsEmptyCollectionsCorrectly()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = "John Doe",
                Numbers = [],
                Scores = []
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = John Doe, Age = 0, IsActive = False, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = null, Numbers = [], Scores = [], Secrets = ***]", result);
        }

        [TestMethod]
        public void ToString_WithSensitiveCollections_RedactsSensitiveCollections()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = "John Doe",
                Numbers = [ 1, 2, 3 ],
                Secrets = new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                }
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = John Doe, Age = 0, IsActive = False, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = null, Numbers = [1, 2, 3], Scores = [], Secrets = ***]", result);
        }

        [TestMethod]
        public void ToString_WithNullValues_HandlesNullValuesCorrectly()
        {
            // Arrange
            var testClass = new GenerateToStringTestClass
            {
                Name = null,
                Age = 30,
                Numbers = null,
                Scores = null
            };

            // Act
            var result = testClass.ToString();

            // Assert
            Assert.AreEqual("[GenerateToStringTestClass: Name = null, Age = 30, IsActive = False, Password = [REDACTED], SecretKey = ***SECRET***, SubClass = null, Numbers = null, Scores = null, Secrets = ***]", result);
        }
    }
}