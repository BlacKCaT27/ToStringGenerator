using Bcss.ToStringGenerator.Attributes;

namespace Bcss.ToStringGenerator.TestData;

[GenerateToString]
public partial class GenerateToStringTestClass
{
    public string? Name { get; set; }
    
    public int Age { get; set; }
    
    public bool IsActive { get; set; }

    [SensitiveData]
    public string? Password { get; set; }

    [SensitiveData("***SECRET***")]
    public string? SecretKey { get; set; }
    
    public SubClass? SubClass { get; set; }

    public List<int>? Numbers { get; set; } = [];
    
    public Dictionary<string, int>? Scores { get; set; } = [];

    [SensitiveData("***")]
    public Dictionary<string, string> Secrets { get; set; } = [];
}