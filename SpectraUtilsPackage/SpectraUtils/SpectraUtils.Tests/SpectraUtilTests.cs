using SpectraUtils.Abstractions;
using SpectraUtils.Services;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class SpectraUtilTests
{
    [Fact]
    public void DefaultConstructor_ProvidesDefaultServices()
    {
        var util = new SpectraUtil();

        Assert.IsType<NameEdit>(util.NameEdit);
        Assert.IsType<PasswordHelper>(util.PasswordHelper);
        Assert.IsType<Pbkdf2PasswordHasher>(util.PasswordHasher);
        Assert.IsType<SecureTokenGenerator>(util.TokenGenerator);
        Assert.IsType<OneTimeCodeGenerator>(util.OneTimeCodeGenerator);

        Assert.Same(util.NameEdit, util.NameEdit);
        Assert.Same(util.PasswordHelper, util.PasswordHelper);
    }

    [Fact]
    public void CustomDependencies_AreUsed()
    {
        var nameEdit = new NameEdit();
        var passwordHelper = new PasswordHelper();
        var passwordHasher = new StubPasswordHasher();
        var tokenGenerator = new SecureTokenGenerator();
        var codeGenerator = new OneTimeCodeGenerator();

        var util = new SpectraUtil(nameEdit, passwordHelper, passwordHasher, tokenGenerator, codeGenerator);

        Assert.Same(nameEdit, util.NameEdit);
        Assert.Same(passwordHelper, util.PasswordHelper);
        Assert.Same(passwordHasher, util.PasswordHasher);
        Assert.Same(tokenGenerator, util.TokenGenerator);
        Assert.Same(codeGenerator, util.OneTimeCodeGenerator);
    }

    [Fact]
    public void CustomDependencies_Null_Throws()
    {
        var nameEdit = new NameEdit();
        var passwordHelper = new PasswordHelper();
        var passwordHasher = new StubPasswordHasher();
        var tokenGenerator = new SecureTokenGenerator();
        var codeGenerator = new OneTimeCodeGenerator();

        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(null!, passwordHelper, passwordHasher, tokenGenerator, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, null!, passwordHasher, tokenGenerator, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, passwordHelper, null!, tokenGenerator, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, passwordHelper, passwordHasher, null!, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, passwordHelper, passwordHasher, tokenGenerator, null!));
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => password;
        public bool Verify(string password, string hash) => password == hash;
        public bool NeedsRehash(string hash) => false;
    }
}
