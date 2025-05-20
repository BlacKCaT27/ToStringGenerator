# ToStringGenerator

A C# source generator that automatically creates customizable `ToString()` implementations for your classes, with built-in support for sensitive data handling.

## Features

- 🚀 Automatic `ToString()` generation using source generators
- 🔒 Built-in support for masking sensitive data
- 📦 Works with any type including collections, dictionaries, and nullable types
- 🎯 Zero runtime overhead—all code is generated at compile time
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

    public List<string> Addresses { get; set; } = [];
    
    public Dictionary<string, string> Preferences {get; set; } = [];
}
```

#### Example Output

```csharp
var user = new User
{
    Username = "john.doe",
    Addresses = ["123 Main St, Apt 4B, New York, NY 10001"],
    Preferences = new Dictionary<string, string>
    {
        {"Color", "Blue"}, {"Font", "Arial"}
    }
};
Console.WriteLine(user.ToString()); // ToString() method automatically generated at compile time

// Output:
[User: Username = john.doe, Addresses = [123 Main St, Apt 4B, New York, NY 10001], Preferences = [{Color = Blue}, {Font = Arial}]
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

    [SensitiveData("***")] // Custom masking values supported
    public string CreditCardNumber { get; set; }
    
    public List<string> Addresses { get; set; } = [];
    
    public Dictionary<string, string> Preferences {get; set; } = [];
}
```

#### Example Output

```csharp
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
};  // Using the example class above
Console.WriteLine(user.ToString());

// Output:
// [User: Username = john.doe, Password = [REDACTED], CreditCardNumber = ***, Addresses = [123 Main St, Apt 4B, New York, NY 10001], Preferences = [{Color = Blue}, {Font = Arial}]
```

You can also override the default value globally using the `ToStringGeneratorRedactedValue` msbuild property:

```xml
<ToStringGeneratorRedactedValue>[MyNewRedactionValue]</ToStringGeneratorRedactedValue>
```

Whenever a masking value is not provided to `SensitiveData`, this property's value will be used instead.

## Attributes
The `GenerateToString` and `SensitiveData` attributes are injected into your project by the source generator by default.
This works fine for most use cases. However, in certain situations (notably when using `[InternalsVisibleTo]`), the compiler
may end up with namespace collisions when generating these attributes across multiple projects in a single solution.

If you run into issues such as this, the required attributes are available to you to be referenced explicitly via
the `Bcss.ToStringGenerator.Attributes` package.

```shell
dotnet add package Bcss.ToStringGenerator.Attributes
```

If you add an explicit reference to the Attributes package, you MUST also disable the automatic attribute generation within the source generator.
To do so, set the following msbuild constant:

```xml
<!-- Set this constant definition to disable automatic source generation of marker interfaces used by the source generator -->
<DefineConstants>GENERATE_TO_STRING_EXCLUDE_GENERATED_ATTRIBUTES</DefineConstants>
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - see [LICENSE.md](LICENSE.md)
