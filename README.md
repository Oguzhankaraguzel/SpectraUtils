# SpectraUtils

[![CI](https://github.com/Oguzhankaraguzel/SpectraUtils/actions/workflows/ci.yml/badge.svg)](https://github.com/Oguzhankaraguzel/SpectraUtils/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/SpectraUtils.svg)](https://www.nuget.org/packages/SpectraUtils/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
![.NET 8 | 10](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4)

**SpectraUtils** is a small, dependency-light utility library for .NET with a focus on Turkish applications:
identity / tax number / IBAN validation, Turkish-aware name formatting, PBKDF2 password hashing and
secure random generators.

| Package | What's inside |
|---|---|
| [`SpectraUtils`](https://www.nuget.org/packages/SpectraUtils/) | Validators, validation attributes, name formatting, password hashing, token / OTP / password generation, LINQ & string extensions. No ASP.NET Core dependency. |
| [`SpectraUtils.AspNetCore`](https://www.nuget.org/packages/SpectraUtils.AspNetCore/) | `AllowedExtensionsAttribute` for `IFormFile` / `IFormFileCollection` uploads. |

> **Upgrading from 2.x?** Version 3.0 contains important bug fixes and breaking changes.
> See the [migration guide](https://github.com/Oguzhankaraguzel/SpectraUtils/blob/main/CHANGELOG.md#migrating-from-2x-to-30).

---

## Installation

```bash
dotnet add package SpectraUtils
dotnet add package SpectraUtils.AspNetCore   # only if you validate file uploads
```

Supported frameworks: **.NET 8** and **.NET 10** (and later).

## Quick start

```csharp
using SpectraUtils.Extensions;

builder.Services.AddSpectraUtils();

// or with options
builder.Services.AddSpectraUtils(options =>
{
    options.Pbkdf2.Iterations = 700_000;
    options.NameCulture = CultureInfo.InvariantCulture; // default: Turkish casing
});
```

All services are stateless and registered as singletons with `TryAdd`, so your own registrations win.

| Service | Default implementation |
|---|---|
| `IPasswordHasher` | `Pbkdf2PasswordHasher` |
| `IPasswordHelper` | `PasswordHelper` |
| `ISecureTokenGenerator` | `SecureTokenGenerator` |
| `IOneTimeCodeGenerator` | `OneTimeCodeGenerator` |
| `INameEdit` | `NameEdit` |
| `ISpectraUtil` | `SpectraUtil` (facade over all of the above) |

Not using DI? Every service has a public parameterless constructor, e.g. `new Pbkdf2PasswordHasher()`,
or use the facade: `var utils = new SpectraUtil();`.

Namespaces: `SpectraUtils.Abstractions` (interfaces), `SpectraUtils.Services` (implementations),
`SpectraUtils.Validation` (static validators), `SpectraUtils.Attributes.ValidationAttributes`,
`SpectraUtils.Configuration` (options), `SpectraUtils.Extensions`.

---

## Security

### Password hashing (PBKDF2)

```csharp
IPasswordHasher hasher = ...; // injected

string hash = hasher.Hash("UserPassword123!");
// PBKDF2$SHA256$600000$<salt>$<key>

if (hasher.Verify(input, user.PasswordHash))
{
    // Upgrade old hashes (e.g. created with fewer iterations) transparently.
    if (hasher.NeedsRehash(user.PasswordHash))
        user.PasswordHash = hasher.Hash(input);
}
```

- Defaults follow OWASP: **PBKDF2-HMAC-SHA256, 600,000 iterations**, 16-byte salt, 32-byte key.
  SHA-384 and SHA-512 are available through `Pbkdf2Options.Algorithm`.
- Every hash stores its own parameters, so changing the options never breaks existing hashes.
  Hashes created by SpectraUtils 2.x are still verified.
- Comparison is constant-time. A malformed hash returns `false` instead of throwing, and tampered
  hashes with absurd iteration counts are rejected (`Pbkdf2Options.MaxIterations`).

### Tokens, one-time codes and passwords

```csharp
string token = tokenGenerator.GenerateToken();            // 32 random bytes, Base64Url (43 chars)
string resetToken = tokenGenerator.GenerateToken(64);

string otp = otpGenerator.GenerateNumericCode();          // "048213" (6 digits, uniformly random)
string longOtp = otpGenerator.GenerateNumericCode(10);    // up to 32 digits

string password = passwordHelper.CreateStandardPassword(); // 12 chars, all character categories
string pin = passwordHelper.CreatePassword(8,
    includeUppercaseLetter: false, includeLowercaseLetter: false,
    includeSpecialCharacter: false, includeNumber: true);
```

Everything uses `System.Security.Cryptography.RandomNumberGenerator` without modulo bias.
`CreatePassword` guarantees at least one character from every selected category.

> Treat OTPs as short-lived secrets: expire them after a few minutes, limit attempts, and compare them in constant time.

---

## Validation

`[TCIDValidation]` makes the value mandatory: `null`, empty or whitespace is invalid. The other attributes treat
`null` as valid, so add `[Required]` when those values are mandatory.
Error messages support `{0}` for the display name and work with `ErrorMessageResourceType` for localization.

```csharp
using SpectraUtils.Attributes.ValidationAttributes;

public class RegisterRequest
{
    [TCIDValidation]                                   // null/empty is invalid too
    public string? IdentityNumber { get; set; }        // "10000000146"

    [TurkishTaxNumberValidation]
    public string? TaxNumber { get; set; }             // 10-digit VKN

    [IbanValidation("TR")]
    public string? Iban { get; set; }                  // "TR33 0006 1005 1978 6457 8413 26"

    [MinimumAge(18, ErrorMessage = "{0}: en az {1} yaşında olmalısınız.")]
    public DateOnly? BirthDate { get; set; }           // DateOnly, DateTime or DateTimeOffset
}
```

| Attribute | Notes |
|---|---|
| `TCIDValidationAttribute` | Turkish identity number (TCKN), both check digits. Mandatory: `null` / empty is invalid. Whitespace is ignored. |
| `TurkishTaxNumberValidationAttribute` | Turkish tax number (VKN) check digit. Whitespace is ignored. |
| `IbanValidationAttribute` | ISO 13616 / MOD-97, country-specific length (e.g. TR = 26). Optional country code. |
| `MinimumAgeAttribute` | Placeholders `{0}` (name) and `{1}` (age). "Today" comes from the registered `TimeProvider`, so it is testable. |
| `AllowedExtensionsAttribute` | In **SpectraUtils.AspNetCore**. See below. |

### Programmatic validators

```csharp
using SpectraUtils.Validation;

bool tckn = TurkishIdentityNumberValidator.IsValid("10000000146");
bool vkn  = TurkishTaxNumberValidator.IsValid("1739750504");
bool iban = IbanValidator.IsValid("TR33 0006 1005 1978 6457 8413 26", expectedCountryCode: "TR");
```

Validators never throw: `null`, empty, malformed or oversized input simply returns `false`.

### File uploads (SpectraUtils.AspNetCore)

```csharp
public class UploadRequest
{
    [Required]
    [AllowedExtensions(2 * 1024 * 1024, ".jpg", ".png")]       // max 2 MB
    public IFormFile? Avatar { get; set; }

    [AllowedExtensions(10 * 1024 * 1024, ".pdf", ".docx",
        FileSizeErrorMessage = "{0} must be smaller than {2}.")]
    public IFormFileCollection? Documents { get; set; }        // every file is checked
}
```

Only the file **name** is checked. If you store or serve uploads, also verify the content (magic bytes).

---

## Name formatting

```csharp
INameEdit names = ...; // injected

names.NameCorrection("  aYşe   nur ");               // "Ayşe Nur"
names.NameCorrection("ismail");                      // "İsmail"  (Turkish casing by default)
names.NameCorrection("mehmet", "ali");               // "Mehmet Ali"
names.SurnameCorrection("yılmaz");                   // "YILMAZ"
names.FullNameCorrection("oğuzhan", "karagüzel");    // "Oğuzhan KARAGÜZEL"
names.StandardizeCharacters("Çağdaş Işık Crème");    // "Cagdas Isik Creme"
names.CreateUserName("Ayşe Nur");                    // "aysenur0427"
```

- Casing uses Turkish rules (`i ↔ İ`, `ı ↔ I`) by default; pass another culture with
  `new NameEdit(CultureInfo.InvariantCulture)` or `options.NameCulture`.
- Works in globalization-invariant mode (e.g. small Docker images) as well.
- Passing `null` returns `null`; the return types are annotated for nullable reference types.

---

## Extensions

```csharp
using SpectraUtils.Extensions;

if (!name.IsNullOrWhiteSpace()) { /* name is not null here */ }

"a😀b".ReverseSafe();                                 // "b😀a" (emoji and accents stay intact)

query.WhereIf(filter.OnlyActive, x => x.IsActive);   // IEnumerable and IQueryable
items.IsNullOrEmpty();                                // count-aware, IQueryable-aware
items.Tap(x => logger.LogDebug("{Item}", x));         // deferred side effect in a pipeline
items.ForEach(Console.WriteLine);
```

---

## Development

```bash
cd SpectraUtilsPackage/SpectraUtils
dotnet test                       # runs on net8.0 and net10.0
dotnet pack -c Release -o artifacts
```

Changes are listed in [CHANGELOG.md](https://github.com/Oguzhankaraguzel/SpectraUtils/blob/main/CHANGELOG.md). Contributions are welcome — please open an issue or a pull request.

## Security disclosure

If you discover a security vulnerability, please report it privately through
[GitHub Security Advisories](https://github.com/Oguzhankaraguzel/SpectraUtils/security/advisories/new)
instead of the public issue tracker.

## License

[MIT](https://github.com/Oguzhankaraguzel/SpectraUtils/blob/main/SpectraUtilsPackage/SpectraUtils/LICENSE) © Oğuzhan KARAGÜZEL — [oguzhankaraguzel.com](https://www.oguzhankaraguzel.com/)
