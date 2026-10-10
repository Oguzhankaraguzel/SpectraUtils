using System.Globalization;

namespace SpectraUtils.Configuration;

/// <summary>
/// Options for the services registered by
/// <see cref="Extensions.ServiceCollectionExtensions.AddSpectraUtils(Microsoft.Extensions.DependencyInjection.IServiceCollection, Action{SpectraUtilsOptions}?)"/>.
/// </summary>
public sealed class SpectraUtilsOptions
{
    /// <summary>
    /// PBKDF2 password hashing settings.
    /// </summary>
    public Pbkdf2Options Pbkdf2 { get; } = new();

    /// <summary>
    /// Culture used by <see cref="Services.NameEdit"/> for upper/lower case conversion.
    /// <see langword="null"/> (default) means Turkish (<c>tr-TR</c>), so <c>i</c> becomes <c>İ</c> and <c>I</c> becomes <c>ı</c>.
    /// </summary>
    public CultureInfo? NameCulture { get; set; }
}
