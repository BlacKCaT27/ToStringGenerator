using Bcss.ToStringGenerator.Attributes;

namespace Bcss.ToStringGenerator.TestData;

[GenerateToString]
public partial class SubClass
{
    public bool IsActive { get; set; }
}