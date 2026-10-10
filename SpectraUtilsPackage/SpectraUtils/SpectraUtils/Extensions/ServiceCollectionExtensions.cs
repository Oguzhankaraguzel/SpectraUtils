using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SpectraUtils.Abstractions;
using SpectraUtils.Configuration;
using SpectraUtils.Services;

namespace SpectraUtils.Extensions;

/// <summary>
/// Extension methods for adding SpectraUtils services to the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all SpectraUtils services as singletons (they are stateless and thread-safe):
    /// <see cref="INameEdit"/>, <see cref="IPasswordHelper"/>, <see cref="IPasswordHasher"/>,
    /// <see cref="ISecureTokenGenerator"/>, <see cref="IOneTimeCodeGenerator"/>, <see cref="ISpectraUtil"/>
    /// and <see cref="TimeProvider.System"/> (used by <c>MinimumAgeAttribute</c>).
    /// </summary>
    /// <remarks>
    /// Services are added with <c>TryAdd</c>, so registrations you make before this call are kept.
    /// Calling this method more than once is safe; every <paramref name="configure"/> delegate is applied.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Optional delegate to configure <see cref="SpectraUtilsOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddSpectraUtils(this IServiceCollection services, Action<SpectraUtilsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        OptionsBuilder<SpectraUtilsOptions> options = services.AddOptions<SpectraUtilsOptions>();
        if (configure is not null)
            options.Configure(configure);

        services.TryAddSingleton(TimeProvider.System);

        services.TryAddSingleton<INameEdit>(sp =>
        {
            SpectraUtilsOptions o = sp.GetRequiredService<IOptions<SpectraUtilsOptions>>().Value;
            return o.NameCulture is null ? new NameEdit() : new NameEdit(o.NameCulture);
        });
        services.TryAddSingleton<IPasswordHelper, PasswordHelper>();
        services.TryAddSingleton<IPasswordHasher>(sp =>
            new Pbkdf2PasswordHasher(sp.GetRequiredService<IOptions<SpectraUtilsOptions>>().Value.Pbkdf2));
        services.TryAddSingleton<ISecureTokenGenerator, SecureTokenGenerator>();
        services.TryAddSingleton<IOneTimeCodeGenerator, OneTimeCodeGenerator>();
        services.TryAddSingleton<ISpectraUtil>(sp => new SpectraUtil(
            sp.GetRequiredService<INameEdit>(),
            sp.GetRequiredService<IPasswordHelper>(),
            sp.GetRequiredService<IPasswordHasher>(),
            sp.GetRequiredService<ISecureTokenGenerator>(),
            sp.GetRequiredService<IOneTimeCodeGenerator>()));

        return services;
    }
}
