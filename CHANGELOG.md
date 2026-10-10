# Changelog

All notable changes to this project are documented here. The project follows [Semantic Versioning](https://semver.org/).

## [3.0.0] - 2026-10-10

### Fixed

- **VKN validation rejected ~60% of valid tax numbers.** The official algorithm counts a non-zero
  intermediate value whose weighted result is divisible by 9 as `9`; 2.x counted it as `0`.
  The 2.x sample `1111111113` was actually invalid (the correct check digit is `4`).
- **Process crash (stack overflow) on long input.** `IbanValidator` and `TurkishTaxNumberValidator`
  allocated stack memory proportional to the input length; a multi-megabyte string sent to an API
  terminated the whole process. Input is now bounded before any stack allocation.
- **`OneTimeCodeGenerator` produced weak 10–18 digit codes.** They were drawn from `[0, int.MaxValue)` and
  never started with 3–9. Every digit is now drawn independently and uniformly.
- `MinimumAgeAttribute.IsValid(object)` threw `NullReferenceException` (null validation context).
- `MinimumAgeAttribute` failed `null` values on parameters/fields but accepted them on nullable properties.
- `AllowedExtensionsAttribute` reported "0 MB" for limits under one megabyte.
- `AllowedExtensionsAttribute` called `File.Exists` with user-supplied strings, letting clients probe the server's file system.
- `NameEdit` methods returned `null` despite a non-nullable `string` return type.
- `StandardizeCharacters` / `CreateUserName` silently dropped Turkish letters in globalization-invariant mode (e.g. `Ayşe` → `aye`).
- `NameEdit.cs` was saved as Windows-1254, so the Turkish character table only compiled correctly on Turkish-locale machines.
- `ReverseSafe` split surrogate pairs and produced invalid strings for emoji (`"a😀b"` → `"b��a"`).
- `Tap` validated its arguments only when the result was enumerated.
- The custom `DistinctBy` clashed with `System.Linq.Enumerable.DistinctBy` and caused CS0121 (ambiguous call) when a comparer was passed.
- `IbanValidator` accepted non-ASCII letters and digits (e.g. Arabic-Indic digits, `ı`).
- The README contained broken emoji, methods that do not exist, and an example that did not compile.

### Added

- `SpectraUtils.AspNetCore` package (contains `AllowedExtensionsAttribute`).
- `TurkishIdentityNumberValidator` (public static TCKN validator, used by `TCIDValidationAttribute`).
- `IPasswordHasher.NeedsRehash(hash)` for transparent upgrades of old hashes.
- `Pbkdf2Options` (iterations, salt size, key size, SHA-256/384/512, max iterations accepted on verify) and
  `SpectraUtilsOptions`, configurable through `AddSpectraUtils(options => ...)`.
- `MinimumAgeAttribute` supports `DateTimeOffset` and reads "today" from the registered `TimeProvider`.
- `AllowedExtensionsAttribute` supports `IFormFileCollection` / `IEnumerable<IFormFile>`, `FileSizeErrorMessage`,
  and `{0}` / `{1}` / `{2}` placeholders; sizes are shown as bytes / KB / MB / GB.
- `IbanValidator` checks country-specific lengths from the SWIFT IBAN registry (e.g. TR = 26).
- `NameEdit(CultureInfo)` constructor and `SpectraUtilsOptions.NameCulture`.
- All validation attributes support `{0}` (display name), `ErrorMessageResourceType` localization, and report `MemberNames`.
- SourceLink, symbol packages (`.snupkg`), deterministic builds and a GitHub Actions CI workflow.

### Changed (breaking)

- Target frameworks: `net8.0` and `net10.0` (was `net9.0` only; .NET 9 support ends in November 2026).
- Namespaces: `SpectraUtils.Abstract` → `SpectraUtils.Abstractions`; `SpectraUtils.Concerete` and
  `SpectraUtils.Concrete` → `SpectraUtils.Services`. New: `SpectraUtils.Configuration`.
- `AllowedExtensionsAttribute` moved to the `SpectraUtils.AspNetCore` package (same namespace).
  The core package no longer depends on the deprecated `Microsoft.AspNetCore.Http.Features` 5.0.
- `SirNameCorrection` → `SurnameCorrection`; `FullNameCorrection` parameters renamed to `surname(s)`.
- `CreateStardartPassword` (property) → `CreateStandardPassword()` (method); length 8 → 12.
- `NameCorrection` keeps word boundaries: `"ahmet can"` → `"Ahmet Can"` (was `"Ahmetcan"`).
  `SurnameCorrection` does the same: `"kara güzel"` → `"KARA GÜZEL"` (was `"KARAGÜZEL"`).
- `CreateUserName` returns lowercase ASCII letters and digits only (`"Ayşe Nur"` → `"aysenur0427"`) and
  throws `ArgumentException` when the name contains no letters or digits.
- Default error messages changed and now include the display name.
- `Pbkdf2PasswordHasher` default iterations 210,000 → 600,000 (OWASP 2023 for SHA-256). Its constructor takes
  `Pbkdf2Options` instead of `(iterations, saltSize, keySize)`; minimum salt 8 bytes, minimum key 16 bytes.
- `PasswordHelper.CreatePassword` throws `ArgumentOutOfRangeException` for invalid lengths (max 4096).
- `OneTimeCodeGenerator` supports up to 32 digits; `SecureTokenGenerator` up to 1024 bytes.
- `AddSpectraUtils` uses `TryAdd` and registers `TimeProvider.System`. `AddSpectraUtilsScoped` was removed
  (the services are stateless; use `AddSpectraUtils`).
- `SpectraUtil` no longer uses `Lazy<T>`; services and validation attributes are now `sealed`.
- `LinqExtensions.DistinctBy` removed — use `System.Linq.Enumerable.DistinctBy`.

### Deprecated

- `IPasswordHelper.HashPassword` (unsalted SHA-256) is `[Obsolete]` with diagnostic id `SPECTRA001`.
  Use `IPasswordHasher`. It still works so legacy hashes can be verified during a migration.

### Migrating from 2.x to 3.0

1. **Packages:** if you use `[AllowedExtensions]`, add `SpectraUtils.AspNetCore`.
2. **Usings:** replace `SpectraUtils.Abstract` with `SpectraUtils.Abstractions`, and `SpectraUtils.Concerete` /
   `SpectraUtils.Concrete` with `SpectraUtils.Services`.
3. **Renames:** `SirNameCorrection` → `SurnameCorrection`, `CreateStardartPassword` → `CreateStandardPassword()`.
4. **DI:** replace `AddSpectraUtilsScoped()` with `AddSpectraUtils()`. Replace
   `new Pbkdf2PasswordHasher(310_000, 32, 64)` with
   `AddSpectraUtils(o => { o.Pbkdf2.Iterations = 310_000; o.Pbkdf2.SaltSize = 32; o.Pbkdf2.KeySize = 64; })`.
5. **Stored password hashes:** nothing to do — 2.x PBKDF2 hashes still verify. Call `NeedsRehash` after a
   successful login to upgrade them to 600,000 iterations.
6. **Legacy SHA-256 hashes** (`HashPassword`): on the next successful login, verify with `HashPassword`
   (suppress `SPECTRA001`), then store `IPasswordHasher.Hash(password)` instead.
7. **Stored names:** if you saved names produced by `NameCorrection` / `SirNameCorrection`, note the new
   word-preserving behavior before comparing old and new values.
8. **VKN:** tax numbers that 2.x rejected may now be accepted. That is the fix, but review any data
   that was rejected because of it.
9. **Nullable validation:** `[MinimumAge]` no longer fails `null`; add `[Required]` where needed.
   `[TCIDValidation]` still rejects `null` and empty values (unchanged from 2.x).

## [2.0.0]

- PBKDF2 password hashing, secure token and one-time code generation.
- IBAN and Turkish tax number validators and attributes.
- `DateOnly` support for `MinimumAgeAttribute`, dependency injection integration.

## [1.0.0]

- Initial release.
