using System.Diagnostics.CodeAnalysis;
using Bcss.ToStringGenerator.Attributes;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Bcss.ToStringGenerator.Example
{
    [ExcludeFromCodeCoverage(Justification = "Example app")]
    [GenerateToString(includePrivateDataMembers: true)]
    public partial class User
    {
        public string PublicField = "publicField";
        public string? Username { get; set; }
        
        [SensitiveData] // Masks sensitive data - default value is '[REDACTED]'
        public string? Password { get; set; }

        [SensitiveData("***")] // Custom masking values supported
        public string? CreditCardNumber { get; set; }

        public List<string> Addresses { get; set; } = [];
    
        public Dictionary<string, string> Preferences {get; set; } = [];
        
        private string MyPrivateProperty { get; set; } = "privateProperty";

#pragma warning disable CS0414 // Field is assigned but its value is never used
        private string _myPrivateField = "privateField";
#pragma warning restore CS0414 // Field is assigned but its value is never used
    }

    [ExcludeFromCodeCoverage(Justification = "Example app")]
    class Program
    {
        static void Main(string[] _)
        {
            var user = new User
            {
                Username = "john.doe",
                Password = "MySecretPassword",
                CreditCardNumber = "WouldntYouLikeToKnow",
                Addresses = ["123 Main St, Apt 4B, New York, NY 10001"],
                Preferences = new Dictionary<string, string>
                {
                    {"Color", "Blue"}, {"Font", "Arial"}
                }
            };
            Console.WriteLine("User details:");
            Console.WriteLine(user.ToString());
            
        }
    }
}
