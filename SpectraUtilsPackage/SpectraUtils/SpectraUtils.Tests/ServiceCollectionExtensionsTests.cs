using Microsoft.Extensions.DependencyInjection;
using SpectraUtils.Abstractions;
using SpectraUtils.Configuration;
using SpectraUtils.Extensions;
using SpectraUtils.Services;
using System.Globalization;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSpectraUtils_NullServices_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddSpectraUtils(null!));
    }

    [Fact]
    public void AddSpectraUtils_RegistersAllServicesAsSingletons()
    {
        var services = new ServiceCollection().AddSpectraUtils();

        foreach (Type serviceType in new[]
        {
            typeof(INameEdit), typeof(IPasswordHelper), typeof(IPasswordHasher),
            typeof(ISecureTokenGenerator), typeof(IOneTimeCodeGenerator), typeof(ISpectraUtil), typeof(TimeProvider),
        })
        {
            ServiceDescriptor descriptor = Assert.Single(services, d => d.ServiceType == serviceType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }

    [Fact]
    public void AddSpectraUtils_ResolvesWorkingServices()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddSpectraUtils(o => o.Pbkdf2.Iterations = 1_000)
            .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });

        Assert.IsType<NameEdit>(provider.GetRequiredService<INameEdit>());
        Assert.IsType<PasswordHelper>(provider.GetRequiredService<IPasswordHelper>());
        Assert.IsType<SecureTokenGenerator>(provider.GetRequiredService<ISecureTokenGenerator>());
        Assert.IsType<OneTimeCodeGenerator>(provider.GetRequiredService<IOneTimeCodeGenerator>());
        Assert.Same(TimeProvider.System, provider.GetRequiredService<TimeProvider>());

        ISpectraUtil util = provider.GetRequiredService<ISpectraUtil>();
        Assert.Same(provider.GetRequiredService<INameEdit>(), util.NameEdit);
        Assert.Same(provider.GetRequiredService<IPasswordHasher>(), util.PasswordHasher);
        Assert.Same(util, provider.GetRequiredService<ISpectraUtil>());
    }

    [Fact]
    public void AddSpectraUtils_AppliesPbkdf2Options()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddSpectraUtils(o =>
            {
                o.Pbkdf2.Iterations = 1_234;
                o.Pbkdf2.Algorithm = Pbkdf2HashAlgorithm.Sha512;
            })
            .BuildServiceProvider();

        string hash = provider.GetRequiredService<IPasswordHasher>().Hash("pwd");
        Assert.StartsWith("PBKDF2$SHA512$1234$", hash, StringComparison.Ordinal);
    }

    [Fact]
    public void AddSpectraUtils_AppliesNameCulture()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddSpectraUtils(o => o.NameCulture = CultureInfo.InvariantCulture)
            .BuildServiceProvider();

        Assert.Equal("Isabel", provider.GetRequiredService<INameEdit>().NameCorrection("isabel"));
    }

    [Fact]
    public void AddSpectraUtils_DefaultNameCulture_IsTurkish()
    {
        using ServiceProvider provider = new ServiceCollection().AddSpectraUtils().BuildServiceProvider();
        Assert.Equal("İpek", provider.GetRequiredService<INameEdit>().NameCorrection("ipek"));
    }

    [Fact]
    public void AddSpectraUtils_KeepsExistingRegistrations()
    {
        var custom = new Pbkdf2PasswordHasher(new Pbkdf2Options { Iterations = 1_000 });
        var fixedTime = new TestAssets.FixedTimeProvider(DateTimeOffset.UnixEpoch);

        var services = new ServiceCollection();
        services.AddSingleton<IPasswordHasher>(custom);
        services.AddSingleton<TimeProvider>(fixedTime);
        services.AddSpectraUtils();

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Same(custom, provider.GetRequiredService<IPasswordHasher>());
        Assert.Same(fixedTime, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void AddSpectraUtils_CalledTwice_DoesNotDuplicateRegistrations()
    {
        var services = new ServiceCollection().AddSpectraUtils().AddSpectraUtils();
        Assert.Single(services, d => d.ServiceType == typeof(IPasswordHasher));
    }

    [Fact]
    public void AddSpectraUtils_InvalidOptions_FailOnResolve()
    {
        using ServiceProvider provider = new ServiceCollection()
            .AddSpectraUtils(o => o.Pbkdf2.Iterations = 0)
            .BuildServiceProvider();

        Assert.Throws<ArgumentOutOfRangeException>(() => provider.GetRequiredService<IPasswordHasher>());
    }
}
