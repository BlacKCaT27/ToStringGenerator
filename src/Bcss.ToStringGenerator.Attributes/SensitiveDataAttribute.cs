namespace Bcss.ToStringGenerator.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SensitiveDataAttribute : Attribute
    {
        public string MaskingValue { get; }

        public SensitiveDataAttribute(string maskingValue = "[REDACTED]")
        {
            MaskingValue = maskingValue;
        }
    }
} 