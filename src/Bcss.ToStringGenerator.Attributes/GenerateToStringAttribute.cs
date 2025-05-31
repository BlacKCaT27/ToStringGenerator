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
        /// <summary>
        /// Whether to include private data members when generating ToString() methods. Default is false.
        /// </summary>
        public bool IncludePrivateDataMembers { get; }

        /// <summary>
        /// Instantiates a new instance of the <see cref="GenerateToStringAttribute"/> class.
        /// </summary>
        /// <param name="includePrivateDataMembers">If true, include private fields and properties from the generated ToString() method. Default is false.</param>
        public GenerateToStringAttribute(bool includePrivateDataMembers = false)
        {
            IncludePrivateDataMembers = includePrivateDataMembers;
        }
    }
} 