using Bcss.ToStringGenerator.Attributes;

namespace Bcss.ToStringGenerator.TestData;

[GenerateToString(true)]
public partial class UnnamedArgSubClass
{
    public bool IsActive { get; set; }
    
    private bool IsPrivate { get; set; }
}