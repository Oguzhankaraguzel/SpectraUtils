using SpectraUtils.Abstract;
using SpectraUtils.Concerete;
using SpectraUtils.Concrete;
using System.Linq;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class SpectraUtilTests
{
    [Fact]
    public void DefaultConstructor_ProvidesServices()
    {
        var util = new SpectraUtil();

        Assert.NotNull(util.NameEdit);
        Assert.NotNull(util.PasswordHelper);
        Assert.NotNull(util.PasswordHasher);
        Assert.NotNull(util.TokenGenerator);
        Assert.NotNull(util.OneTimeCodeGenerator);

        Assert.Same(util.NameEdit, util.NameEdit);
        Assert.Same(util.PasswordHelper, util.PasswordHelper);
    }

    [Fact]
    public void CustomDependencies_AreUsed()
    {
        var nameEdit = new StubNameEdit();
        var passwordHelper = new StubPasswordHelper();
        var passwordHasher = new StubPasswordHasher();
        var tokenGenerator = new StubTokenGenerator();
        var codeGenerator = new StubCodeGenerator();

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
        var nameEdit = new StubNameEdit();
        var passwordHelper = new StubPasswordHelper();
        var passwordHasher = new StubPasswordHasher();
        var tokenGenerator = new StubTokenGenerator();
        var codeGenerator = new StubCodeGenerator();

        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(null!, passwordHelper, passwordHasher, tokenGenerator, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, null!, passwordHasher, tokenGenerator, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, passwordHelper, null!, tokenGenerator, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, passwordHelper, passwordHasher, null!, codeGenerator));
        Assert.Throws<ArgumentNullException>(() => new SpectraUtil(nameEdit, passwordHelper, passwordHasher, tokenGenerator, null!));
    }

    private sealed class StubNameEdit : INameEdit
    {
        public string CreateUserName(string? name) => name ?? string.Empty;
        public string FullNameCorrection(string? name, string? sirName) => $"{name} {sirName}".Trim();
        public string FullNameCorrection(string?[] names, string?[] sirNames) => string.Join(" ", names.Concat(sirNames));
        public string NameCorrection(string? name) => name ?? string.Empty;
        public string NameCorrection(params string?[] names) => string.Join(" ", names);
        public string SirNameCorrection(string? sirName) => sirName ?? string.Empty;
        public string SirNameCorrection(params string?[] sirNames) => string.Join(" ", sirNames);
        public string StandardizeCharacters(string? input) => input ?? string.Empty;
    }

    private sealed class StubPasswordHelper : IPasswordHelper
    {
        public string CreatePassword(int passwordLength, bool includeUppercaseLetter = true, bool includeLowercaseLetter = true, bool includeSpecialCharacter = true, bool includeNumber = true) => new('x', passwordLength);
        public string CreateStardartPassword => "standard";
        public string HashPassword(string password) => password;
    }

    private sealed class StubPasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => password;
        public bool Verify(string password, string hash) => password == hash;
    }

    private sealed class StubTokenGenerator : ISecureTokenGenerator
    {
        public string GenerateToken(int byteLength = 32) => new('a', byteLength);
    }

    private sealed class StubCodeGenerator : IOneTimeCodeGenerator
    {
        public string GenerateNumericCode(int digits = 6) => new('1', digits);
    }
}
