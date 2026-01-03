using Microsoft.Extensions.DependencyInjection;
using SpectraUtils;
using SpectraUtils.Abstract;
using SpectraUtils.Concerete;
using SpectraUtils.Concrete;
using SpectraUtils.Extensions;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSpectraUtils_RegistersSingletons()
    {
        var services = new ServiceCollection();

        services.AddSpectraUtils();

        AssertDescriptor<INameEdit, NameEdit>(services, ServiceLifetime.Singleton);
        AssertDescriptor<IPasswordHelper, PasswordHelper>(services, ServiceLifetime.Singleton);
        AssertDescriptor<ISpectraUtil, SpectraUtil>(services, ServiceLifetime.Singleton);
        AssertDescriptor<IPasswordHasher, Pbkdf2PasswordHasher>(services, ServiceLifetime.Singleton);
        AssertDescriptor<ISecureTokenGenerator, SecureTokenGenerator>(services, ServiceLifetime.Singleton);
        AssertDescriptor<IOneTimeCodeGenerator, OneTimeCodeGenerator>(services, ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddSpectraUtilsScoped_RegistersScoped()
    {
        var services = new ServiceCollection();

        services.AddSpectraUtilsScoped();

        AssertDescriptor<INameEdit, NameEdit>(services, ServiceLifetime.Scoped);
        AssertDescriptor<IPasswordHelper, PasswordHelper>(services, ServiceLifetime.Scoped);
        AssertDescriptor<ISpectraUtil, SpectraUtil>(services, ServiceLifetime.Scoped);
        AssertDescriptor<IPasswordHasher, Pbkdf2PasswordHasher>(services, ServiceLifetime.Scoped);
        AssertDescriptor<ISecureTokenGenerator, SecureTokenGenerator>(services, ServiceLifetime.Scoped);
        AssertDescriptor<IOneTimeCodeGenerator, OneTimeCodeGenerator>(services, ServiceLifetime.Scoped);
    }

    private static void AssertDescriptor<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(TImplementation), descriptor!.ImplementationType);
        Assert.Equal(lifetime, descriptor.Lifetime);
    }
}
