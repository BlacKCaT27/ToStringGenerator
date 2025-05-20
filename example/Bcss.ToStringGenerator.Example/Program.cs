using Bcss.ToStringGenerator.Attributes;

namespace Bcss.ToStringGenerator.Example
{
    [GenerateToString]
    public partial class User
    {
        public string Test = "te";
        public string Username { get; set; } = "john.doe";

        [SensitiveData]
        public string Password { get; set; } = "secret123";

        public int Age { get; set; } = 30;

        [SensitiveData("***")]
        public string CreditCardNumber { get; set; } = "4111111111111111";

        [SensitiveData]
        public string SSN { get; set; } = "123-45-6789";

        public List<string> Addresses { get; set; } = new List<string> { "123 Main St", "Apt 4B", "New York, NY 10001" };

        public Dictionary<string, string> Preferences { get; set; } = new Dictionary<string, string> { { "Color", "Blue" }, { "Font", "Arial" } };
    }

    [GenerateToString]
    public partial class TestClass
    {
        public string Test { get; set; } = "test";
    }

    class Program
    {
        static void Main(string[] args)
        {
            var user = new User();
            var test = new TestClass();
            Console.WriteLine("User details:");
            Console.WriteLine(user.ToString());
            
            Console.WriteLine("TestClass details:");
            Console.WriteLine(test.ToString());
        }
    }
}
