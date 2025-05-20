using Bcss.ToStringGenerator.Attributes;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Bcss.ToStringGenerator.Example
{
    [GenerateToString]
    public partial class User
    {
        public string? Username { get; set; }
        
        [SensitiveData] // Masks sensitive data - default value is '[REDACTED]'
        public string? Password { get; set; }

        [SensitiveData("***")] // Custom masking values supported
        public string? CreditCardNumber { get; set; }

        public List<string> Addresses { get; set; } = [];
    
        public Dictionary<string, string> Preferences {get; set; } = [];
    }

    class Program
    {
        static void Main(string[] args)
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
