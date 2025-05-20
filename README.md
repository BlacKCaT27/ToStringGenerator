# ToStringGenerator

A C# source generator that automatically creates customizable `ToString()` implementations for your classes, with built-in support for sensitive data handling.

## Features

- 🚀 Automatic `ToString()` generation using source generators
- 🔒 Built-in support for masking sensitive data
- 📦 Works with complex types including collections and dictionaries
- 🎯 Zero runtime overhead - all code is generated at compile time
- ✨ Clean and readable output format

## Installation

Add the NuGet package to your project:

```shell
dotnet add package Bcss.ToStringGenerator
```

## Usage

### Basic Usage

1. Add the `[GenerateToString]` attribute to your class
2. Make your class `partial`
3. The source generator will automatically create a `ToString()` implementation at compile-time.

```csharp
using Bcss.ToStringGenerator.Attributes;

[GenerateToString]
public partial class User
{
    public string Username { get; set; }

    [SensitiveData]
    public string Password { get; set; }

    [SensitiveData("CC")]
    public string CreditCardNumber { get; set; }

    public List<string> Addresses { get; set; }
}
```

### Handling Sensitive Data

Use the `[SensitiveData]` attribute to mask sensitive information:

```csharp
[GenerateToString]
public partial class User
{
    public string Username { get; set; }

    [SensitiveData] // Masks sensitive data - default value is '[REDACTED]'
    public string Password { get; set; }

    [SensitiveData("CC")] // Custom masking values supported
    public string CreditCardNumber { get; set; }
    
    public List<string> Addresses { get; set; }
}
```

### Example Output

```csharp
var user = new User();  // Using the example class above
Console.WriteLine(user.ToString());

// Output:
[User: Username = john.doe, Password = ****, CreditCardNumber = CC, SSN = [REDACTED], Addresses = [123 Main St, Apt 4B, New York, NY 10001], Preferences = [{Color = Blue}, {Font = Arial}]
```

You can also override the default value globally using the `ToStringGeneratorRedactedValue` msbuild property:

```xml
<ToStringGeneratorRedactedValue>[MyNewRedactionValue]</ToStringGeneratorRedactedValue>
```

Whenever a masking value is not provided to `SensitiveData`, this property's value will be used instead.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - see [LICENSE.md](LICENSE.md)
